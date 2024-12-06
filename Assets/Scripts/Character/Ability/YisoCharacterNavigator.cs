using System.Collections.Generic;
using System.Linq;
using Controller.Map;
using Controller.Quest;
using Core.Domain.Locale;
using Core.Logger;
using Core.Service;
using Core.Service.Game;
using Core.Service.Log;
using Core.Service.ObjectPool;
using Core.Service.UI.Game;
using Core.Service.UI.HUD;
using Manager_Temp_;
using Pathfinding;
using Sirenix.OdinInspector;
using Tools.Environment;
using Tools.Event;
using Unity.VisualScripting;
using UnityEngine;
using Utils.Extensions;

namespace Character.Ability {
    [AddComponentMenu("Yiso/Character/Abilities/CharacterNavigator")]
    public class YisoCharacterNavigator : YisoCharacterAbility, IYisoEventListener<YisoMapChangeEvent>,
        IYisoEventListener<YisoNavigationZoneEnterEvent>, IYisoEventListener<YisoQuestTargetPositionRegisterEvent> {
        [Title("PathFinding")] public GameObject footprintPrefab;
        public float arrivalThreshold = 2f; // 해당 거리 이내면 navigation 종료

        private YisoNavigationZoneController prevZone;
        private YisoNavigationZoneController currentZone;
        private YisoNavigationZoneController targetZone;

        private Vector2 prevTargetPosition;
        private Vector2 currentTargetPosition;
        private Vector2 finalTargetPosition;

        private Vector2 prevCharacterPosition;
        private Vector2 currentCharacterPosition;

        private bool isArrived = false;
        private bool isFootprintShowing = false;

        private Dictionary<string, Vector2> currentPathRef;
        private Dictionary<int, YisoQuestTargetPositionRegisterer.QuestTarget> questDestinationRef;

        private IYisoObjectPoolService poolService;
        private GameObject footprintContent;
        private List<SpriteRenderer> footPrints = new();

        private Seeker seeker;
        private AIPath aiPath;
        private AIDestinationSetter destinationSetter;

        private bool CanNavigate => currentZone != null && targetZone != null;

        private readonly int maxResearchPathCount = 30;
        private readonly float interval = 1f;

        private readonly YisoLocale startNavigationMessage = new YisoLocale() {
            kr = "길 안내를 시작합니다.",
            en = "Quest navigation started."
        };

        private readonly YisoLocale stopNavigationMessage = new YisoLocale() {
            kr = "길 안내를 할 수 없습니다.",
            en = "Quest navigation is not available."
        };

        private readonly YisoLocale completeNavigationMessage = new YisoLocale() {
            kr = "목표 지점에 도착했습니다.",
            en = "You have reached your destination."
        };

        private IYisoGameUIService GameUIService => YisoServiceProvider.Instance.Get<IYisoGameUIService>();
        private IYisoHUDUIService HUDUIService => YisoServiceProvider.Instance.Get<IYisoHUDUIService>();

        private YisoLocale.Locale CurrentLoacle =>
            YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoCharacterNavigator>();

        #region Initialization

        protected override void PreInitialization() {
            base.PreInitialization();
            InitializePathfinding();
            InitializePool();
        }

        protected override void Initialization() {
            base.Initialization();
            InitializeQuestNavigation();
        }

        protected virtual void InitializePool() {
            if (footprintContent == null) {
                footprintContent = new GameObject("[Pooler] Quest Navigation Footprints");
                footprintContent.transform.localPosition = Vector3.zero;
            }

            poolService = YisoServiceProvider.Instance.Get<IYisoObjectPoolService>();
            poolService.WarmPool(footprintPrefab, 100, footprintContent);
        }

        protected virtual void InitializePathfinding() {
            aiPath = character.characterModel.GetOrAddComponent<AIPath>();
            destinationSetter = character.characterModel.GetOrAddComponent<AIDestinationSetter>();
            seeker = character.characterModel.GetOrAddComponent<Seeker>();

            aiPath.enabled = false;
            destinationSetter.enabled = false;

            aiPath.movementType = AIBase.MovementType.None;
            aiPath.orientation = OrientationMode.YAxisForward;
            aiPath.radius = 0.5f;
            aiPath.gravity = Vector3.zero;
            aiPath.endReachedDistance = arrivalThreshold;
            aiPath.maxSpeed = 0f;
            aiPath.enableRotation = false;
        }

        protected virtual void InitializeQuestNavigation() {
            HUDUIService.OnQuestPathFind(StartQuestNavigation);
        }

        #endregion

        #region On/Off

        public virtual void StartNavigation(GameObject target, YisoLocale comment) {
            isArrived = false;

            if (target == null) {
                StopQuestNavigation();
                return;
            }

            var targetPosition = target.transform.position;
            finalTargetPosition = targetPosition;
            targetZone = FindZoneByPosition(targetPosition);

            if (comment != null) GameUIService.FloatingText(comment[CurrentLoacle]);
            ActivateNavigation();
        }

        protected virtual void StartQuestNavigation(int questId) {
            isArrived = false;

            var targetPosition = GetTargetPositionByQuestId(questId, out var success);
            if (!success) {
                StopQuestNavigation();
                return;
            }

            finalTargetPosition = targetPosition;
            targetZone = FindZoneByPosition(targetPosition);

            GameUIService.FloatingText(startNavigationMessage[CurrentLoacle]);
            ActivateNavigation();
        }

        protected virtual void StopQuestNavigation() {
            isArrived = false;
            ClearPath();
            targetZone = null;
            GameUIService.FloatingText(stopNavigationMessage[CurrentLoacle]);
            ActivateNavigation(false);
        }

        #endregion

        #region Core

        /// <summary>
        /// currentZone 업데이트
        /// Player position을 체크해 footprint 업데이트
        /// </summary>
        public override void ProcessAbility() {
            if (!AbilityAuthorized) return;
            if (!GameManager.HasInstance || GameManager.Instance.CurrentMapController == null) return;
            UpdateCharacterCurrentPosition();
            UpdateZone();
            UpdateTargetPosition();
            PlaceFootprintsToTargetPosition();
        }

        /// <summary>
        /// Navigation 시작
        /// </summary>
        public virtual void ActivateNavigation(bool activate = true) {
            abilityPermitted = activate;
            if (AbilityAuthorized && CanNavigate) {
                currentPathRef =
                    YisoCharacterQuestNavigatorUtils.FindPath(currentZone, targetZone, finalTargetPosition);
            }
        }

        /// <summary>
        /// Character 현재 위치 업데이트
        /// </summary>
        protected virtual void UpdateCharacterCurrentPosition() {
            prevCharacterPosition = currentCharacterPosition;
            currentCharacterPosition = character.characterModel.transform.position;
        }

        /// <summary>
        /// Current Zone을 업데이트
        /// </summary>
        protected virtual void UpdateZone() {
            prevZone = currentZone;
            currentZone = FindZoneByPosition(character.characterModel.transform.position);
        }

        /// <summary>
        /// 최종 목적지까지 도착할 때까지 currentTargetPosition을 업데이트 
        /// </summary>
        /// <returns></returns>
        protected virtual void UpdateTargetPosition() {
            if (!AbilityAuthorized || !CanNavigate || isArrived) return;
            if (currentZone.ZoneName != targetZone.ZoneName) {
                var attemptCount = 0;
                var pathFound = false;
                while (attemptCount < maxResearchPathCount) {
                    if (currentPathRef != null && currentPathRef.ContainsKey(currentZone.ZoneName)) {
                        prevTargetPosition = currentTargetPosition;
                        currentTargetPosition = currentPathRef[currentZone.ZoneName];
                        pathFound = true;
                        break;
                    }
                    else {
                        LogService.Warn(
                            "[CharacterQuestNavigator] : Could not find current zone in path. Re-searching the route");
                        currentPathRef =
                            YisoCharacterQuestNavigatorUtils.FindPath(currentZone, targetZone, finalTargetPosition);
                    }

                    attemptCount++;
                }

                if (!pathFound && attemptCount >= maxResearchPathCount) {
                    LogService.Error(
                        "[CharacterQuestNavigator] : Maximum re-searching attempts exceeded. Terminating search.");
                    StopQuestNavigation();
                }
            }
            else {
                prevTargetPosition = currentTargetPosition;
                currentTargetPosition = finalTargetPosition;
            }
        }

        #endregion

        #region Pathfinding

        protected virtual void PlaceFootprintsToTargetPosition() {
            // Navigation 할 수 없거나 이미 도착한 경우는 Return
            if (!CanNavigate || isArrived) {
                ClearPath();
                return;
            }

            // 중간 도착 지점 도착 여부 확인
            if (ReachedDestination(currentTargetPosition, 0.2f)) {
                ClearPath();
                return;
            }

            // 최종 도착 지점 도착 여부 확인
            if (ReachedDestination(finalTargetPosition, arrivalThreshold)) {
                GameUIService.FloatingText(completeNavigationMessage[CurrentLoacle]);
                LogService.Debug("[CharacterQuestNavigator] : Destination reached. Terminating navigation.");
                ClearPath();
                isArrived = true;
                targetZone = null;
                return;
            }

            // Path 업데이트
            if (!isFootprintShowing || prevZone != currentZone || prevTargetPosition != currentTargetPosition ||
                currentCharacterPosition != prevCharacterPosition) {
                seeker.StartPath(character.characterModel.transform.position, currentTargetPosition, OnCompletePath);
            }
        }

        private bool ReachedDestination(Vector3 position, float threshold) {
            var moveDirection = position - character.characterModel.transform.position;
            return moveDirection.sqrMagnitude <= threshold;
        }

        private void ClearPath() {
            ClearFootprints();
            isFootprintShowing = false;
            destinationSetter.target = null;
            aiPath.enabled = false;
            destinationSetter.enabled = false;
        }

        private void OnCompletePath(Path path) {
            if (path.error) {
                LogService.Error($"Path failed: {path.errorLog}");
                return;
            }

            var abPath = path as ABPath;
            ClearFootprints();
            PlaceFootprints(abPath.vectorPath);
            isFootprintShowing = true;
        }

        private void PlaceFootprints(List<Vector3> path) {
            for (var i = 0; i < path.Count - 1; i++) {
                var start = path[i];
                var end = path[i + 1];
                var distance = start.GetDistance(end);
                var dir = (end - start).normalized;

                for (var d = 0f; d < distance; d += interval) {
                    var position = start + dir * d;
                    var sr = poolService.SpawnObject<SpriteRenderer>(footprintPrefab, footprintContent);
                    sr.gameObject.transform.position = position;
                    footPrints.Add(sr);
                }
            }
        }

        private void ClearFootprints() {
            foreach (var footprint in footPrints) {
                poolService.ReleaseObject(footprint.gameObject);
            }

            footPrints.Clear();
        }

        #endregion

        #region Getter

        protected virtual YisoNavigationZoneController FindZoneByPosition(Vector2 position) {
            return GameManager.Instance.CurrentMapController?.navigationZones?.FirstOrDefault(zone =>
                zone.Contains(position));
        }

        protected virtual Vector2 GetTargetPositionByQuestId(int questId, out bool success) {
            if (!questDestinationRef.ContainsKey(questId)) {
                success = false;
                return Vector2.zero;
            }

            success = true;
            return questDestinationRef[questId].GetTargetPosition();
        }

        #endregion

        #region Event

        public void OnEvent(YisoNavigationZoneEnterEvent e) {
            if (!AbilityAuthorized || !CanNavigate) return;
            if (currentPathRef.ContainsKey(currentZone.ZoneName)) return;
            ActivateNavigation();
        }

        public void OnEvent(YisoMapChangeEvent e) {
            if (e.isInitialMapLoad) return;
            ClearPath();
            abilityPermitted = false;
            targetZone = null;
            isArrived = false;
            questDestinationRef = new Dictionary<int, YisoQuestTargetPositionRegisterer.QuestTarget>();
        }

        public void OnEvent(YisoQuestTargetPositionRegisterEvent e) {
            questDestinationRef ??= new Dictionary<int, YisoQuestTargetPositionRegisterer.QuestTarget>();
            if (questDestinationRef.ContainsKey(e.questTarget.QuestId)) return;
            questDestinationRef[e.questTarget.QuestId] = e.questTarget;
        }

        #endregion

        #region Callback

        protected override void OnDeath() {
            base.OnDeath();
            currentZone = null;
        }

        protected override void OnRespawn() {
            base.OnRespawn();
            currentZone = FindZoneByPosition(character.characterModel.transform.position);
        }

        #endregion

        protected override void OnEnable() {
            base.OnEnable();
            this.YisoEventStartListening<YisoMapChangeEvent>();
            this.YisoEventStartListening<YisoNavigationZoneEnterEvent>();
            this.YisoEventStartListening<YisoQuestTargetPositionRegisterEvent>();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.YisoEventStopListening<YisoMapChangeEvent>();
            this.YisoEventStopListening<YisoNavigationZoneEnterEvent>();
            this.YisoEventStopListening<YisoQuestTargetPositionRegisterEvent>();
        }
    }

    public static class YisoCharacterQuestNavigatorUtils {
        public static Dictionary<string, Vector2> FindPath(YisoNavigationZoneController currentZone,
            YisoNavigationZoneController targetZone, Vector2 finalTargetPosition) {
            var priorityQueue = new SortedSet<(YisoNavigationZoneController Zone, float Distance)>(
                Comparer<(YisoNavigationZoneController, float)>.Create((a, b) =>
                    a.Item2 == b.Item2 ? a.Item1.ZoneName.CompareTo(b.Item1.ZoneName) : a.Item2.CompareTo(b.Item2)));
            var distances = new Dictionary<YisoNavigationZoneController, float>();
            var previous = new Dictionary<YisoNavigationZoneController, YisoPortal>();
            var visited = new HashSet<YisoNavigationZoneController>();

            distances[currentZone] = 0;
            priorityQueue.Add((currentZone, 0));

            while (priorityQueue.Count > 0) {
                var (current, currentDistance) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                if (current == targetZone) {
                    return ReconstructPath(previous, current, finalTargetPosition);
                }

                if (!visited.Add(current)) {
                    continue;
                }

                foreach (var portal in current.portals) {
                    var neighborZone = portal.connectedPortal.ParentZone;
                    var newDist = currentDistance + 1.0f; // 1.0f 는 다익스트라 알고리즘에서 Cost인데 고정값인 1을 사용할거라 이렇게 표현함.

                    if (!distances.ContainsKey(neighborZone) || newDist < distances[neighborZone]) {
                        distances[neighborZone] = newDist;
                        previous[neighborZone] = portal;
                        priorityQueue.Add((neighborZone, newDist));
                    }
                }
            }

            // 경로를 찾을 수 없는 경우 null 반환
            return null;
        }

        private static Dictionary<string, Vector2> ReconstructPath(
            Dictionary<YisoNavigationZoneController, YisoPortal> previous, YisoNavigationZoneController current,
            Vector2 finalTargetPosition) {
            var path = new Dictionary<string, Vector2>();

            path[current.ZoneName] = finalTargetPosition; // 도착 Zone은 포털의 위치가 아닌 target Position을 저장

            // 도착 Zone 이전에 거쳐야할 Zone들은 Portal 위치를 저장 
            while (previous.ContainsKey(current)) {
                var portal = previous[current];
                current = portal.ParentZone;
                path[current.ZoneName] = portal.transform.position;
            }

            return path;
        }
    }
}