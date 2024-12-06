using System;
using System.Collections.Generic;
using System.Linq;
using Camera;
using Character.Core;
using Core.Behaviour;
using Core.Domain.Map;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Manager_Temp_;
using Sirenix.OdinInspector;
using Spawn;
using Tools.Event;
using Unity.VisualScripting;
using UnityEngine;

namespace Controller.Map {
    public struct YisoMapChangeEvent {
        public YisoMap prevMap;
        public YisoMap currentMap;
        public bool isInitialMapLoad;

        public YisoMapChangeEvent(YisoMap prevMap, YisoMap currentMap, bool isInitialMapLoad) {
            this.prevMap = prevMap;
            this.currentMap = currentMap;
            this.isInitialMapLoad = isInitialMapLoad;
        }

        static YisoMapChangeEvent e;

        public static void Trigger(YisoMap prevMap, YisoMap currentMap, bool isInitialMapLoad) {
            e.prevMap = prevMap;
            e.currentMap = currentMap;
            e.isInitialMapLoad = isInitialMapLoad;
            YisoEventManager.TriggerEvent(e);
        }
    }

    [Serializable]
    public class CutsceneTriggerMapper {
        public int gameModeId;
        public GameObject cutsceneTrigger;
    }

    [AddComponentMenu("Yiso/Controller/Map/Map Controller")]
    public class YisoMapController : RunIBehaviour, IYisoEventListener<YisoInGameEvent> {
        [Title("Zones")] public List<YisoNavigationZoneController> navigationZones;

        [ShowIf("@navigationZones.Count == 0")]
        public MapBounds defaultMapBounds;

        [Title("Camera")] public PolygonCollider2D[] cameraBoundaries;

        [Title("Checkpoints")] public YisoCharacterCheckPoint[] checkPoints;

        [Title("Cutscenes")] public CutsceneTriggerMapper[] stageCutsceneTriggers;
        public CutsceneTriggerMapper[] bountyCutsceneTriggers;
        public YisoCharacterCheckPoint CurrentCheckPoint => currentCheckPoint;
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoMapController>();

        public YisoMap CurrentMap { get; protected set; }
        public PolygonCollider2D CurrentCameraBoundary { get; protected set; }

        protected List<YisoCharacterCheckPoint> currentCheckPoints;
        protected YisoCharacterCheckPoint initialCheckPoint;
        protected YisoCharacterCheckPoint currentCheckPoint; // 그 외 respawn시 사용됨

        #region Initialization

        /// <summary>
        /// Initialization (매번 맵이 바뀔때 마다 호출됨)
        /// Story
        /// </summary>
        /// <param name="map"></param>
        /// <param name="gameModeId"></param>
        /// <param name="relevantIds"></param>
        public virtual void Initialization(YisoMap map, int gameModeId, bool isInitialLoad, int saveCheckpointId, List<int> relevantIds = null) {
            // Set Map
            CurrentMap = map;
            LogService.Debug($"[YisoMapController.Initialization] Set current map (Id : {CurrentMap.Id}).");

            // Activate Cutscene Triggers
            InitializeCutsceneTriggers(gameModeId, relevantIds);

            // Set Check Points
            InitializeCheckPoints(isInitialLoad, saveCheckpointId);

            // Set Camera Boundary
            InitializeCameraBoundaries();

            // Initialize Navigation Zones
            InitializeNavigationZones();
        }

        /// <summary>
        /// Initialization
        /// Bounty
        /// </summary>
        /// <param name="map"></param>
        /// <param name="gameModeId"></param>
        /// <param name="relevantIds"></param>
        public virtual void Initialization(YisoMap map, int gameModeId, List<int> relevantIds = null) {
            // Set Map
            CurrentMap = map;
            LogService.Debug($"[YisoMapController.Initialization] Set current map (Id : {CurrentMap.Id}).");

            // Activate Cutscene Triggers
            InitializeCutsceneTriggers(gameModeId, relevantIds);

            // Set Check Points
            InitializeCheckPoints(false);

            // Set Camera Boundary
            InitializeCameraBoundaries();

            // Initialize Navigation Zones
            InitializeNavigationZones();
        }

        /// <summary>
        /// Game Mode와 현재 Stage에 해당하는 cutscene trigger만 activate (나머진 inactivate)
        /// </summary>
        /// <param name="relevantIds"></param>
        protected virtual void InitializeCutsceneTriggers(int gameModeId, List<int> relevantIds = null) {
            relevantIds ??= new List<int>();
            relevantIds.Add(gameModeId);
            switch (GameManager.Instance.CurrentGameMode) {
                case GameManager.GameMode.Story:
                    ActivateStageTriggers(stageCutsceneTriggers, true, relevantIds);
                    ActivateStageTriggers(bountyCutsceneTriggers, false, relevantIds);
                    break;
                case GameManager.GameMode.Bounty:
                    ActivateStageTriggers(stageCutsceneTriggers, false, relevantIds);
                    ActivateStageTriggers(bountyCutsceneTriggers, true, relevantIds);
                    break;
            }

            LogService.Debug($"[YisoMapController.InitializeCutsceneTriggers] Initialize Cutscene Triggers.");
        }

        protected virtual void InitializeCheckPoints(bool useSavedCheckpoint, int saveCheckPointId = -1) {
            if (checkPoints == null || checkPoints.Length == 0) {
                LogService.Warn("[YisoMapController.InitializeCheckPoints] No checkpoints found for this map.");
                return;
            }
            
            currentCheckPoints = checkPoints.OrderBy(o => o.checkPointOrder).ToList();

            // Set checkpoints
            if (currentCheckPoints != null && currentCheckPoints.Count > 0) {
                // First Load 인 경우 Save Data를 initial Check point로..
                // 아닌 경우 (맵 이동) 해당 맵의 initial Check point를 찾아서..
                initialCheckPoint = useSavedCheckpoint ? GetCheckPointById(saveCheckPointId) : currentCheckPoints[0];
                currentCheckPoint = initialCheckPoint;
                
                LogService.Debug($"[YisoMapController.InitializeCheckPoints] Set current check point. {currentCheckPoint}");
            }
        }

        protected virtual YisoCharacterCheckPoint GetCheckPointById(int id) {
            if (currentCheckPoints == null || currentCheckPoints.Count == 0) {
                LogService.Warn($"[YisoMapController.CheckpointManager] No checkpoints are available to search.");
                return null;
            }

            foreach (var checkPoint in currentCheckPoints.Where(checkPoint => checkPoint.id == id)) {
                LogService.Debug($"[YisoMapController.CheckpointManager] Checkpoint found with ID: {id}.");
                return checkPoint;
            }

            LogService.Warn($"[YisoMapController.CheckpointManager] No checkpoint found with ID: {id}.");
            return null;
        }
        

        protected virtual void InitializeCameraBoundaries() {
            if (cameraBoundaries == null || cameraBoundaries.Length == 0) {
                LogService.Warn("[MapController.InitializeCameraBoundaries] Camera boundary is not registered.");
                return;
            }

            foreach (var cameraBoundary in cameraBoundaries) {
                cameraBoundary.isTrigger = true;
                cameraBoundary.AddComponent<YisoCameraBoundarySetter>();
            }

            CurrentCameraBoundary = cameraBoundaries[0];
            YisoCameraEvent.Trigger(YisoCameraEventTypes.SetConfiner, CurrentCameraBoundary);
            LogService.Debug("[MapController.InitializeCameraBoundaries] Set Camera Boundary.");
        }
        
        protected virtual void InitializeNavigationZones() {
            LogService.Debug("[YisoMapController.InitializeNavigationZones] Initializing navigation zones.");

            if (navigationZones == null || navigationZones.Count == 0) {
                LogService.Debug("[YisoMapController.InitializeNavigationZones] No navigation zones found. Creating default navigation zone.");
                CreateDefaultNavigationZones();
            }

            for (var i = 0; i < navigationZones.Count; i++) {
                navigationZones[i].ZoneName = $"Zone{i + 1}";
                LogService.Debug($"[YisoMapController.InitializeNavigationZones] Navigation zone initialized: {navigationZones[i].ZoneName}.");
            }

            LogService.Info("[YisoMapController.InitializeNavigationZones] Navigation zones initialization complete.");
        }


        protected virtual void CreateDefaultNavigationZones() {
            LogService.Debug("[YisoMapController.CreateDefaultNavigationZones] Creating default navigation zone.");

            var confinerGameObject = new GameObject("Navigation Zone (Default)");
            confinerGameObject.transform.SetParent(transform);
            confinerGameObject.transform.localPosition = Vector3.zero;

            LogService.Debug("[YisoMapController.CreateDefaultNavigationZones] Navigation zone GameObject created and parent set.");

            var confinerPolygonCollider2D = confinerGameObject.AddComponent<PolygonCollider2D>();
            confinerPolygonCollider2D.isTrigger = true;
            confinerPolygonCollider2D.usedByComposite = true;
            confinerPolygonCollider2D.offset = CurrentCameraBoundary.offset;
            confinerPolygonCollider2D.points = CurrentCameraBoundary.points;

            LogService.Debug("[YisoMapController.CreateDefaultNavigationZones] PolygonCollider2D configured with camera boundary data.");

            var navigationZoneController = confinerGameObject.AddComponent<YisoNavigationZoneController>();
            navigationZoneController.mapBounds = defaultMapBounds;
            navigationZones.Add(navigationZoneController);

            LogService.Info("[YisoMapController.CreateDefaultNavigationZones] Default navigation zone created and added to navigationZones list.");
        }


        #endregion

        #region CheckPoint

        /// <summary>
        /// 모든 IRespawnable 오브젝트를 적절한 (associatedCheckPoints) 체크포인트에 할당합니다.
        /// </summary>
        public virtual void CheckpointAssignment() {
            if (currentCheckPoints == null || currentCheckPoints.Count == 0) {
                LogService.Warn($"[YisoMapController.CheckpointAssignment] No current checkpoints available for assignment.");
                return;
            }
            var respawnables = FindObjectsOfType<MonoBehaviour>(true).OfType<IRespawnable>();

            foreach (var respawnable in respawnables) {
                for (var i = currentCheckPoints.Count; i >= 0; i--) {
                    var autoRespawn = (respawnable as MonoBehaviour)?.GetComponent<YisoCharacterAutoRespawn>();
                    if (autoRespawn == null) {
                        currentCheckPoints[i].AssignObjectToCheckPoint(respawnable);
                    }
                    else {
                        if (autoRespawn.alwaysRespawnInAllCheckPoints) {
                            currentCheckPoints[i].AssignObjectToCheckPoint(respawnable);
                        }
                        else {
                            if (autoRespawn.associatedCheckPoints.Contains(currentCheckPoints[i])) {
                                currentCheckPoints[i].AssignObjectToCheckPoint(respawnable);
                            }
                        }
                    }
                }
            }
            LogService.Debug($"[YisoMapController.CheckpointAssignment] Assign IRespawnable objects to check points.");
        }

        /// <summary>
        /// current check point를 변경할 때 사용
        /// </summary>
        /// <param name="newCheckPoint"></param>
        public virtual void SetCurrentCheckpoint(YisoCharacterCheckPoint newCheckPoint) {
            if (!newCheckPoint.forceAssignation && currentCheckPoint != null && newCheckPoint.checkPointOrder < currentCheckPoint.checkPointOrder) return;
            currentCheckPoint = newCheckPoint;
            LogService.Debug($"[YisoMapController.SetCurrentCheckpoint] Set Current Checkpoint: {newCheckPoint.id}.");
        }

        #endregion

        /// <summary>
        /// Check point로 소환
        /// 마을 귀환할 때
        /// 죽었을 때 다시 Respawn 할 때
        /// </summary>
        /// <param name="player"></param>
        /// <param name="isRespawn"></param>
        public virtual void SpawnPlayer(YisoCharacter player, bool isRespawn) {
            if (player.characterType != YisoCharacter.CharacterTypes.Player) return;
            if (currentCheckPoint != null) {
                LogService.Debug($"[MapController.SpawnPlayer] {(isRespawn ? "Respawn" : "Spawn")} Player in current check point.");
                currentCheckPoint.SpawnPlayer(player, isRespawn);
            }
            else {
                LogService.Warn("[MapController.SpawnPlayer] No checkpoint or initial spawn point has been defined, can't respawn the Player.");
            }
        }

        /// <summary>
        /// 특정 위치로 소환
        /// 초기 Scene Loading 시 Player 위치 불러와서 Spawn할 때
        /// 맵과 맵 이동할 때
        /// </summary>
        /// <param name="player"></param>
        /// <param name="spawnPosition"></param>
        /// <param name="isRespawn"></param>
        public virtual void SpawnPlayer(YisoCharacter player, Vector2 spawnPosition, bool isRespawn) {
            if (player.characterType != YisoCharacter.CharacterTypes.Player) return;
            LogService.Debug($"[MapController.SpawnPlayer] {(isRespawn ? "Respawn" : "Spawn")} Player in {spawnPosition} position.");
            player.RespawnAt(spawnPosition, YisoCharacter.FacingDirections.South, isRespawn);
        }

        public virtual void RestartGame() {
            if (currentCheckPoint == null) {
                currentCheckPoint = initialCheckPoint;
            }

            if (currentCheckPoint == null) {
                LogService.Warn(
                    "[YisoMapController.RestartGame] Respawn Check: Current checkpoint is null. Player will respawn at the default starting position.");
            }
            else {
                LogService.Debug(
                    $"[YisoMapController.RestartGame] Respawn Check: Player will respawn at checkpoint '{currentCheckPoint.name}' at position {currentCheckPoint.SpawnPosition}.");
            }
        }

        private void ActivateStageTriggers(CutsceneTriggerMapper[] triggers, bool active, ICollection<int> ids) {
            if (triggers == null) {
                LogService.Warn($"[YisoMapController.ActivateStageTriggers] Triggers array is null. Exiting method.");
                return;
            }

            foreach (var trigger in triggers) {
                var shouldActivate = active && ids.Contains(trigger.gameModeId);
                trigger.cutsceneTrigger.SetActive(shouldActivate);
                LogService.Debug($"[YisoMapController.ActivateStageTriggers] Trigger for GameModeId {trigger.gameModeId} is now {(shouldActivate ? "active" : "inactive")}.");
            }
        }

        #region Event

        /// <summary>
        /// Stage 바뀌는 경우
        /// Cutscene Trigger 활성화/비활성화
        /// Checkpoint 활성화/비활성화
        /// </summary>
        /// <param name="e"></param>
        public void OnEvent(YisoInGameEvent e) {
            LogService.Debug($"[YisoMapController.OnEvent] Received event of type {e.eventType}.");

            if (e.eventType != YisoInGameEventTypes.MoveNextStage) {
                LogService.Warn($"[YisoMapController.OnEvent] Ignored event: {e.eventType} is not MoveNextStage.");
                return;
            }

            LogService.Debug($"[YisoMapController.OnEvent] Processing event for GameMode: {GameManager.Instance.CurrentGameMode}.");

            switch (GameManager.Instance.CurrentGameMode) {
                case GameManager.GameMode.Bounty:
                    LogService.Debug($"[YisoMapController.OnEvent] Initializing Bounty mode for Bounty ID: {e.bounty.Id}.");
                    InitializeCutsceneTriggers(e.bounty.Id);
                    break;

                case GameManager.GameMode.Story:
                    LogService.Debug($"[YisoMapController.OnEvent] Initializing Story mode for Stage ID: {e.stage.Id}. Relevant Stage IDs: {string.Join(", ", e.stage.RelevantStageIds ?? new List<int>())}.");
                    InitializeCutsceneTriggers(e.stage.Id, e.stage.RelevantStageIds);
                    break;

                default:
                    LogService.Error($"[YisoMapController.OnEvent] Unsupported GameMode: {GameManager.Instance.CurrentGameMode}. Aborting.");
                    return;
            }
            
            LogService.Debug($"[YisoMapController.OnEvent] Event processing complete.");
        }

        protected override void OnEnable() {
            base.OnEnable();
            this.YisoEventStartListening();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.YisoEventStopListening();
        }

        #endregion

        [Serializable]
        public class MapBounds {
            public Transform lowerLeftAnchor;
            public Transform upperRightAnchor;

            public virtual void Scan() {
                if (lowerLeftAnchor == null || upperRightAnchor == null) {
                    YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<MapBounds>().Warn("[MapBounds] Map anchors are not registered. Map scan is failed.");
                    return;
                }

                var lowerLeftPosition = lowerLeftAnchor.position;
                var upperRightPosition = upperRightAnchor.position;

                var activeGridPath = AstarPath.active.data.gridGraph;
                var width = (upperRightPosition.x - lowerLeftPosition.x);
                var depth = (upperRightPosition.y - lowerLeftPosition.y);

                var centerWidth = width / 2;
                var centerDepth = depth / 2;

                activeGridPath.center =
                    new Vector3(lowerLeftPosition.x + centerWidth, lowerLeftPosition.y + centerDepth);
                activeGridPath.SetDimensions((int) width, (int) depth, 1f);

                AstarPath.active.Scan();
            }
        }
    }
}