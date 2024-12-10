using Character;
using Character.Ability;
using Character.Core;
using Character.Health;
using Manager;
using Sirenix.OdinInspector;
using Tools.Movement.PathMovement;
using UnityEngine;
using Utils.Beagle;

namespace Tools.AI.Actions.Move {
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionMovePatrol")]
    public class YisoAIActionMovePatrol : YisoAIAction {
        [Title("Obstacle")] public bool changeDirectionOnObstacle = true;
        public LayerMask obstaclesLayerMask = LayerManager.ObstaclesLayerMask | LayerManager.MapLayerMask;
        public float obstaclesCheckFrequency = 1f;

        protected TopDownController controller;
        protected YisoCharacter character;
        protected YisoCharacterMovement characterMovement;
        protected YisoCharacterOrientation2D orientation2D;
        protected YisoHealth health;
        protected YisoPath path;

        protected Vector2 direction;

        protected float lastObstacleDetectionTimestamp = 0f;
        protected float lastPatrolPointReachedAt = 0f;

        // Yiso Path의 변수들을 가져와 로컬 변수에 저장하는 것임.
        protected int currentIndex = 0;
        protected int indexLastFrame = -1;
        protected float waitingDelay = 0f;

        #region Initialization

        protected override void Awake() {
            base.Awake();
            InitializePatrol();
        }

        protected virtual void InitializePatrol() {
            controller = gameObject.GetComponentInParent<TopDownController>();
            character = gameObject.GetComponentInParent<YisoCharacter>();
            characterMovement = character?.FindAbility<YisoCharacterMovement>();
            orientation2D = character?.FindAbility<YisoCharacterOrientation2D>();
            health = character?.characterHealth;
            path = gameObject.GetComponentInParent<YisoPath>();
            direction = orientation2D.isFacingRight ? Vector2.right : Vector2.left;
            currentIndex = 0;
            indexLastFrame = -1;
            waitingDelay = 0;
            initialized = true;
            lastPatrolPointReachedAt = Time.time;
        }

        public override void OnExitState() {
            base.OnExitState();
            characterMovement?.SetHorizontalMovement(0f);
            characterMovement?.SetVerticalMovement(0f);
        }

        #endregion

        #region Core

        public override void PerformAction() {
            Patrol();
        }

        protected virtual void Patrol() {
            if (characterMovement == null) return;

            // Delay 중이니 움직임 Stop
            if (Time.time - lastPatrolPointReachedAt < waitingDelay) {
                characterMovement.SetHorizontalMovement(0f);
                characterMovement.SetVerticalMovement(0f);
                return;
            }

            // Obstacle 감지되면 방향 바꿔
            CheckForObstacles();

            // path의 Node Index 확인해서 다르면 그 방향으로 움직여야함. 그 전에 해당 노드에 저장된 Delay 확인 
            currentIndex = path.CurrentIndex;
            if (currentIndex != indexLastFrame) {
                lastPatrolPointReachedAt = Time.time;
                DetermineDelay();
            }

            // 방향 결정
            direction = (path.CurrentPoint - transform.position).normalized;

            // 움직여
            characterMovement.SetHorizontalMovement(direction.x);
            characterMovement.SetVerticalMovement(direction.y);

            indexLastFrame = currentIndex;
        }

        /// <summary>
        /// Obstacle을 Raycast 쏴서 확인한 후 있으면 방향 바꿈
        /// </summary>
        protected virtual void CheckForObstacles() {
            if (!changeDirectionOnObstacle) return;
            if (Time.time - lastObstacleDetectionTimestamp < obstaclesCheckFrequency) return;

            var raycast = YisoDebugUtils.RayCast(controller.ColliderCenter,
                direction, 1f, obstaclesLayerMask, YisoColorUtils.Gold, true);

            if (raycast) {
                ChangeDirection();
            }

            lastObstacleDetectionTimestamp = Time.time;
        }

        /// <summary>
        /// YisoPathMovementElement에 저장된 Delay 실행
        /// </summary>
        protected virtual void DetermineDelay() {
            // 출발점일때
            if (path.Direction > 0 && currentIndex == 0
                || path.Direction < 0 && currentIndex == path.pathElements.Count - 1) {
                var previousPathIndex =
                    path.Direction > 0
                        ? path.pathElements.Count - 1
                        : 1; // TODO: 왜 direction < 0일때 previousPathIndex가 0이 아니라 1인지
                waitingDelay = path.pathElements[previousPathIndex].delay;
            }
            else {
                var previousPathIndex = path.Direction > 0 ? currentIndex - 1 : currentIndex + 1;
                waitingDelay = path.pathElements[previousPathIndex].delay;
            }
        }

        public virtual void ChangeDirection() {
            direction = -direction;
            path.ChangeDirection();
        }

        #endregion

        #region Health

        protected virtual void OnRevive() {
            if (!initialized) return;

            if (orientation2D != null) {
                direction = orientation2D.isFacingRight ? Vector2.right : Vector2.left;
            }

            InitializePatrol();
        }

        protected override void OnEnable() {
            base.OnEnable();
            if (health == null) health = gameObject.GetComponentInParent<YisoHealth>();
            if (health != null) health.onRevive += OnRevive;
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (health != null) health.onRevive -= OnRevive;
        }

        #endregion

        #region Gizmo

        protected virtual void OnDrawGizmosSelected() {
            if (path == null) return;
            Gizmos.color = YisoColorUtils.IndianRed;
            Gizmos.DrawLine(transform.position, path.CurrentPoint);
        }

        #endregion
    }
}