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
    public class YisoMapController : RunIBehaviour, IYisoEventListener<YisoStageChangeEvent>, IYisoEventListener<YisoBountyChangeEvent> {
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

        public YisoMap CurrenMap { get; protected set; }
        public PolygonCollider2D CurrentCameraBoundary { get; protected set; }

        protected List<YisoCharacterCheckPoint> currentCheckPoints;
        protected YisoCharacterCheckPoint initialCheckPoint;
        protected YisoCharacterCheckPoint currentCheckPoint;
        protected Vector2 initialSpawnPointPosition;
        protected bool initialized = false;

        #region Initialization

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="map"></param>
        /// <param name="gameModeId"></param>
        /// <param name="relevantIds"></param>
        public virtual void Initialization(YisoMap map, int gameModeId, List<int> relevantIds = null) {
            // Set Map
            CurrenMap = map;

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

        protected virtual void InitializeCutsceneTriggers(List<int> relevantIds) {
            void SetTriggersActive(CutsceneTriggerMapper[] triggers, bool active, ICollection<int> ids) {
                if (triggers == null) return;

                foreach (var trigger in triggers) {
                    var shouldActivate = active && ids.Contains(trigger.gameModeId);
                    trigger.cutsceneTrigger.SetActive(shouldActivate);
                }
            }

            switch (GameManager.Instance.CurrentGameMode) {
                case GameManager.GameMode.Story:
                    SetTriggersActive(stageCutsceneTriggers, true, relevantIds);
                    SetTriggersActive(bountyCutsceneTriggers, false, relevantIds);
                    break;
                case GameManager.GameMode.Bounty:
                    SetTriggersActive(stageCutsceneTriggers, false, relevantIds);
                    SetTriggersActive(bountyCutsceneTriggers, true, relevantIds);
                    break;
            }
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
                    initialSpawnPointPosition = initialCheckPoint.spawnPosition == null
                        ? initialCheckPoint.transform.position
                        : initialCheckPoint.spawnPosition.transform.position;
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
                currentCheckPoint.SpawnPlayer(player, isRespawn);
            }
            else {
                LogService.Warn("[MapController] No checkpoint or initial spawn point has been defined, can't respawn the Player.");
            }
        }

        public virtual void RestartGame() {
            if (currentCheckPoint == null) {
                currentCheckPoint = initialCheckPoint;
            }
        }

        #region Event

        /// <summary>
        /// Stage id는 바뀌나 Map은 바뀌지 않는 경우
        /// </summary>
        /// <param name="e"></param>
        public void OnEvent(YisoStageChangeEvent e) {
            if (e.isMapChanged || GameManager.Instance.CurrentGameMode != GameManager.GameMode.Story) return;

            // Initialize Cutscene Triggers
            if (stageCutsceneTriggers == null) return;
            var relevantIds = e.currentStage.RelevantStageIds;
            relevantIds.Add(e.currentStage.Id);
            foreach (var trigger in stageCutsceneTriggers) {
                if (!relevantIds.Contains(trigger.gameModeId)) continue;
                trigger.cutsceneTrigger.SetActive(true);
            }

            // Initialize Checkpoints
            InitializeCheckPoints(e.currentStage.Id);
            CheckpointAssignment();
        }

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
            this.YisoEventStartListening<YisoStageChangeEvent>();
            this.YisoEventStartListening<YisoBountyChangeEvent>();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.YisoEventStopListening<YisoStageChangeEvent>();
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