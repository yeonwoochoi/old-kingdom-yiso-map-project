using Character.Ability;
using Character.Core;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Manager_Temp_;
using Pathfinding;
using UnityEngine;

namespace Tools.AI.Actions.Move {
    [AddComponentMenu("AI/Character/AI/Actions/AIActionMoveAwayFromTarget")]
    public class YisoAIActionMoveAwayFromTarget : YisoAIAction {
        public YisoAIBrain.AITargetType aiTargetType = YisoAIBrain.AITargetType.Main;
        public float maximumDistance = 8f;
        public float minimumDistance = 4f;

        protected YisoCharacter character;
        protected YisoCharacterMovement characterMovement;

        protected Seeker seeker;
        protected AIPath aiPath;
        protected AIDestinationSetter destinationSetter;

        protected Vector2 destination;
        protected readonly int maxAttempts = 30;
        protected readonly float endReachedDistance = 1f;
        protected readonly float checkRadius = 0.5f;
        protected LayerMask ObstacleLayerMask => LayerManager.ObstaclesLayerMask | LayerManager.MapLayerMask;
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoAIActionMoveAwayFromTarget>();

        private Transform Target {
            get {
                return aiTargetType switch {
                    YisoAIBrain.AITargetType.Main => brain.mainTarget,
                    YisoAIBrain.AITargetType.Sub => brain.subTarget,
                    YisoAIBrain.AITargetType.SpawnPosition => brain.spawnPositionTarget,
                    _ => null
                };
            }
        }

        public override void PerformAction() {
            if (Target == null || brain.owner == null) return;
            var currentDirection = Target.position - brain.owner.transform.position;
            if (!IsOppositeDirection(brain.owner.transform.position, destination, currentDirection) ||
                Vector2.Distance(brain.owner.transform.position, destination) <= endReachedDistance) {
                destination = GetValidRandomPosition();
                seeker.StartPath(brain.owner.transform.position, destination);
            }
        }

        protected override void Awake() {
            base.Awake();
            character = gameObject.GetComponentInParent<YisoCharacter>();
            characterMovement = character?.FindAbility<YisoCharacterMovement>();
        }

        public override void Initialization() {
            base.Initialization();
            InitializePathfinding();
        }

        protected virtual void InitializePathfinding() {
            if (brain.owner == null) return;

            aiPath = brain.owner.GetOrAddComponent<AIPath>();
            destinationSetter = brain.owner.GetOrAddComponent<AIDestinationSetter>();
            seeker = brain.owner.GetOrAddComponent<Seeker>();

            aiPath.enabled = false;
            destinationSetter.enabled = false;

            aiPath.movementType = AIBase.MovementType.Custom;
            aiPath.moveAction = MoveCharacter;
            aiPath.orientation = OrientationMode.YAxisForward;
            aiPath.radius = 0.5f;
            aiPath.gravity = Vector3.zero;
            aiPath.endReachedDistance = endReachedDistance;
            aiPath.maxSpeed = characterMovement != null ? characterMovement.MovementSpeed : 5f;
            aiPath.enableRotation = false;
        }

        protected virtual void MoveCharacter(Vector3 direction) {
            characterMovement.SetMovement(direction);
        }

        public override void OnEnterState() {
            base.OnEnterState();
            if (brain.owner != null && Target != null) {
                destination = GetValidRandomPosition();
                seeker.StartPath(brain.owner.transform.position, destination);
            }

            if (!aiPath.enabled) aiPath.enabled = true;
            if (!destinationSetter.enabled) destinationSetter.enabled = true;
        }

        public override void OnExitState() {
            base.OnExitState();
            seeker.CancelCurrentPathRequest();
            destinationSetter.target = null;
            aiPath.enabled = false;
            destinationSetter.enabled = false;

            characterMovement?.SetHorizontalMovement(0f);
            characterMovement?.SetVerticalMovement(0f);
        }

        protected virtual Vector2 GetValidRandomPosition() {
            var direction = (brain.owner.transform.position - Target.position).normalized; // 반대로 도망가니까
            Vector2 randomPosition;
            var attempts = 0;

            do {
                randomPosition = GetRandomPositionDirection(brain.owner.transform.position, direction);
                attempts++;
            } while (IsColliderAtPosition(randomPosition, ObstacleLayerMask) && attempts < maxAttempts);

            if (attempts >= maxAttempts) {
                LogService.Warn($"[YisoAIActionMoveAwayFromTarget] Could not find a valid position after multiple attempts ({attempts}).");
            }

            return randomPosition;
        }

        /// <summary>
        /// 특정 방향의 반대 방향인지 확인하는 함수
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="referenceDirection"></param>
        /// <param name="thresholdAngle"></param>
        /// <returns></returns>
        public bool IsOppositeDirection(Vector2 origin, Vector2 destination, Vector2 referenceDirection,
            float thresholdAngle = 80f) {
            var directionToDestination = (destination - origin).normalized;

            // 기준 방향 벡터의 반대 방향 계산
            var oppositeReferenceDirection = -referenceDirection.normalized;

            // 두 벡터 간의 내적 계산
            var dotProduct = Vector2.Dot(directionToDestination, oppositeReferenceDirection);

            // 내적으로부터 각도 계산
            var angleBetween = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

            // 두 벡터 간의 각도가 thresholdAngle 이상이면 반대 방향으로 간주
            return angleBetween <= thresholdAngle;
        }

        protected virtual bool IsColliderAtPosition(Vector2 position, LayerMask layerMask) {
            return Physics2D.OverlapCircle(position, checkRadius, layerMask) != null;
        }

        protected virtual Vector2 GetRandomPositionDirection(Vector2 origin, Vector2 direction) {
            // 반대 방향 벡터 계산
            var normalizedDirection = direction.normalized;

            // 랜덤한 각도와 거리 생성
            var randomAngle = Random.Range(0, 360) * Mathf.Deg2Rad;
            var randomDistance = Random.Range(minimumDistance, maximumDistance);

            // 반대 방향으로 랜덤한 위치 계산
            var randomOffset = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomDistance;
            var randomPosition = origin + normalizedDirection * randomDistance + randomOffset;

            return randomPosition;
        }
    }
}