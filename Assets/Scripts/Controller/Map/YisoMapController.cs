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

    [Serializable]
    public class CheckPointMapper {
        public GameManager.GameMode gameMode = GameManager.GameMode.Story;
        public int gameModeId;
        public YisoCharacterCheckPoint[] checkPoints;
    }

    [AddComponentMenu("Yiso/Controller/Map/Map Controller")]
    public class YisoMapController : RunIBehaviour, IYisoEventListener<YisoInGameEvent> {
        [Title("Zones")] public List<YisoNavigationZoneController> navigationZones;

        [ShowIf("@navigationZones.Count == 0")]
        public MapBounds defaultMapBounds;

        [Title("Camera")] public PolygonCollider2D[] cameraBoundaries;

        [Title("Checkpoints")] public CheckPointMapper[] checkPoints;

        [Title("Cutscenes")] public CutsceneTriggerMapper[] stageCutsceneTriggers;
        public CutsceneTriggerMapper[] bountyCutsceneTriggers;

        public Vector2 InitialSpawnPointPosition => initialSpawnPointPosition;
        public YisoCharacterCheckPoint CurrentCheckPoint => currentCheckPoint;
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoMapController>();

        public YisoMap CurrentMap { get; protected set; }
        public PolygonCollider2D CurrentCameraBoundary { get; protected set; }

        protected List<YisoCharacterCheckPoint> currentCheckPoints;
        protected YisoCharacterCheckPoint initialCheckPoint;
        protected YisoCharacterCheckPoint currentCheckPoint; // 그 외 respawn시 사용됨
        protected Vector2 initialSpawnPointPosition; // Story Mode에서 초기 로딩시 사용됨
        protected bool initialized = false;

        #region Initialization

        /// <summary>
        /// Initialization (매번 맵이 바뀔때 마다 호출됨)
        /// Story
        /// </summary>
        /// <param name="map"></param>
        /// <param name="gameModeId"></param>
        /// <param name="relevantIds"></param>
        public virtual void Initialization(YisoMap map, Vector2 savePlayerPosition, int saveCheckpointId, bool isFirstLoad, int gameModeId, List<int> relevantIds = null) {
            // Set Map
            CurrentMap = map;
            LogService.Debug($"[YisoMapController.Initialization] Set current map (Id : {CurrentMap.Id}).");

            // Activate Cutscene Triggers
            relevantIds ??= new List<int>();
            relevantIds.Add(gameModeId);
            InitializeCutsceneTriggers(relevantIds);

            // Set Check Points
            InitializeCheckPoints(gameModeId);

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

            // Activate Cutscene Triggers
            relevantIds ??= new List<int>();
            relevantIds.Add(gameModeId);
            InitializeCutsceneTriggers(relevantIds);

            // Set Check Points
            InitializeCheckPoints(gameModeId);

            // Set Camera Boundary
            InitializeCameraBoundaries();

            // Initialize Navigation Zones
            InitializeNavigationZones();
        }

        /// <summary>
        /// Game Mode와 현재 Stage에 해당하는 cutscene trigger만 activate (나머진 inactivate)
        /// </summary>
        /// <param name="relevantIds"></param>
        protected virtual void InitializeCutsceneTriggers(List<int> relevantIds) {
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

        protected virtual void InitializeCheckPoints(int gameModeId) {
            if (initialized) return;
            if (checkPoints == null || checkPoints.Length == 0) {
                initialSpawnPointPosition = Vector2.zero;
            }
            else {
                var gameMode = GameManager.Instance.CurrentGameMode;
                // Get checkpoints according to id and sort it by order
                foreach (var checkpoint in checkPoints) {
                    // Checkpoint that does not correspond to the current game mode id
                    if (checkpoint.gameMode != gameMode || checkpoint.gameModeId != gameModeId) {
                        foreach (var c in checkpoint.checkPoints) {
                            c.gameObject.SetActive(false);
                        }

                        continue;
                    }

                    // Checkpoint that corresponds to the current game mode id
                    if (checkpoint.checkPoints != null && checkpoint.checkPoints.Length > 0) {
                        foreach (var c in checkpoint.checkPoints) {
                            c.gameObject.SetActive(true);
                        }

                        currentCheckPoints = checkpoint.checkPoints.OrderBy(o => o.checkPointOrder).ToList();
                    }
                }

                // Set checkpoints
                if (currentCheckPoints != null && currentCheckPoints.Count > 0) {
                    initialCheckPoint = currentCheckPoints[0];
                    initialSpawnPointPosition = initialCheckPoint.SpawnPosition;
                    currentCheckPoint = initialCheckPoint;
                }
            }
        }

        protected virtual void InitializeCameraBoundaries() {
            if (cameraBoundaries == null || cameraBoundaries.Length == 0) {
                LogService.Warn("[MapController] Camera boundary is not registered.");
                return;
            }

            foreach (var cameraBoundary in cameraBoundaries) {
                cameraBoundary.isTrigger = true;
                cameraBoundary.AddComponent<YisoCameraBoundarySetter>();
            }

            CurrentCameraBoundary = cameraBoundaries[0];
            YisoCameraEvent.Trigger(YisoCameraEventTypes.SetConfiner, CurrentCameraBoundary);
        }

        protected virtual void InitializeNavigationZones() {
            if (navigationZones == null || navigationZones.Count == 0) {
                CreateDefaultNavigationZones();
            }

            for (var i = 0; i < navigationZones.Count; i++) {
                navigationZones[i].ZoneName = $"Zone{i + 1}";
            }
        }

        protected virtual void CreateDefaultNavigationZones() {
            var confinerGameObject = new GameObject("Navigation Zone (Default)");
            confinerGameObject.transform.SetParent(transform);
            confinerGameObject.transform.localPosition = Vector3.zero;

            var confinerPolygonCollider2D = confinerGameObject.AddComponent<PolygonCollider2D>();
            confinerPolygonCollider2D.isTrigger = true;
            confinerPolygonCollider2D.usedByComposite = true;
            confinerPolygonCollider2D.offset = CurrentCameraBoundary.offset;
            confinerPolygonCollider2D.points = CurrentCameraBoundary.points;

            var navigationZoneController = confinerGameObject.AddComponent<YisoNavigationZoneController>();
            navigationZoneController.mapBounds = defaultMapBounds;
            navigationZones.Add(navigationZoneController);
        }

        #endregion

        #region CheckPoint

        /// <summary>
        /// 모든 IRespawnable 오브젝트를 적절한 (associatedCheckPoints) 체크포인트에 할당합니다.
        /// </summary>
        public virtual void CheckpointAssignment() {
            if (currentCheckPoints == null || currentCheckPoints.Count == 0) return;
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
        }

        /// <summary>
        /// current check point를 변경할 때 사용
        /// </summary>
        /// <param name="newCheckPoint"></param>
        public virtual void SetCurrentCheckpoint(YisoCharacterCheckPoint newCheckPoint) {
            // order랑 상관없이 그냥 바로 등록
            if (newCheckPoint.forceAssignation) {
                currentCheckPoint = newCheckPoint;
                return;
            }

            if (currentCheckPoint == null) {
                currentCheckPoint = newCheckPoint;
            }

            if (newCheckPoint.checkPointOrder >= currentCheckPoint.checkPointOrder) {
                currentCheckPoint = newCheckPoint;
            }
        }

        #endregion
        
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
        
        public virtual void SpawnPlayer(YisoCharacter player, Vector2 spawnPosition, bool isRespawn) {
            if (player.characterType != YisoCharacter.CharacterTypes.Player) return;
            if (currentCheckPoint != null) {
                LogService.Debug($"[MapController.SpawnPlayer] {(isRespawn ? "Respawn" : "Spawn")} Player in current check point.");
                currentCheckPoint.SpawnPlayer(player, isRespawn);
            }
            else {
                LogService.Warn("[MapController.SpawnPlayer] No checkpoint or initial spawn point has been defined, can't respawn the Player.");
            }
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
                LogService.Warn($"[YisoMapController.ActivateStageTriggers] Trigger for GameModeId {trigger.gameModeId} is now {(shouldActivate ? "active" : "inactive")}.");
            }
        }

        #region Event

        /// <summary>
        /// Stage 바뀌는 경우
        /// </summary>
        /// <param name="e"></param>
        public void OnEvent(YisoInGameEvent e) {
            if (e.eventType != YisoInGameEventTypes.MoveNextStage || GameManager.Instance.CurrentGameMode != GameManager.GameMode.Story) return;
            if (e.stage == null) return;
            
            // Initialize Cutscene Triggers
            // TODO: 이부분 리팩토링 하기 (ActivateStageTriggers 사용하고 Bounty Mode일때도 적용 되게끔)
            if (stageCutsceneTriggers != null) {
                var relevantIds = e.stage.RelevantStageIds;
                relevantIds.Add(e.stage.Id);
                foreach (var trigger in stageCutsceneTriggers) {
                    if (!relevantIds.Contains(trigger.gameModeId)) continue;
                    trigger.cutsceneTrigger.SetActive(true);
                }
            }

            // Initialize Checkpoints
            InitializeCheckPoints(e.stage.Id);
            CheckpointAssignment();
        }

        /// <summary>
        /// Bounty 바뀌는 경우
        /// </summary>
        /// <param name="e"></param>
        public void OnEvent(YisoBountyChangeEvent e) {
            if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.Bounty) return;

            // Initialize Cutscene Triggers
            if (stageCutsceneTriggers == null) return;
            foreach (var trigger in stageCutsceneTriggers) {
                if (e.currentBounty.Id != trigger.gameModeId) continue;
                trigger.cutsceneTrigger.SetActive(true);
            }

            // Initialize Checkpoints
            InitializeCheckPoints(e.currentBounty.Id);
            CheckpointAssignment();
        }

        protected override void OnEnable() {
            base.OnEnable();
            this.YisoEventStartListening<YisoInGameEvent>();
            this.YisoEventStartListening<YisoBountyChangeEvent>();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.YisoEventStopListening<YisoInGameEvent>();
            this.YisoEventStopListening<YisoBountyChangeEvent>();
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