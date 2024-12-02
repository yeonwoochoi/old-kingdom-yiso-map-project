using Character.Ability;
using Character.Core;
using Manager_Temp_;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.AI.Actions.Move {
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionMoveRandomly")]
    public class YisoAIActionMoveRandomly : YisoAIAction {
        [Title("Duration")] public float maximumDurationInADirection = 2f; // 안 멈추고 정해진 방향으로 최대 몇 초동안 갈 수 있게끔 할거냐

        [Title("Obstacles")]
        public LayerMask obstacleLayerMask = LayerManager.ObstaclesLayerMask | LayerManager.MapLayerMask;

        public float obstacleCheckFrequency = 0f; // Obstacle이 있는지 체크하는 빈도 (이전 체크 ~ 다음 체크 까지 몇초?)
        public Vector2 minimumRandomDirection = new(-1f, -1f);
        public Vector2 maximumRandomDirection = new(1f, 1f);

        protected YisoCharacterMovement characterMovement;
        protected Vector2 direction;
        protected Collider2D collider;
        protected float lastObstacleDetectionTimestamp = 0f;
        protected float lastDirectionChangeTimestamp = 0f;

        public override void PerformAction() {
            CheckForObstacles();

            if (Time.time - lastDirectionChangeTimestamp > maximumDurationInADirection) {
                PickRandomDirection();
            }

            Move();
        }

        public override void Initialization() {
            if (!ShouldInitialize) return;
            base.Initialization();
            characterMovement = gameObject.GetComponentInParent<YisoCharacter>()?.FindAbility<YisoCharacterMovement>();
            collider = gameObject.GetComponentInParent<Collider2D>();
            PickRandomDirection();
        }

        public override void OnExitState() {
            base.OnExitState();
            characterMovement?.SetHorizontalMovement(0f);
            characterMovement?.SetVerticalMovement(0f);
        }

        protected virtual void CheckForObstacles() {
            if (Time.time - lastObstacleDetectionTimestamp > obstacleCheckFrequency) {
                // Box를 쏴서 부딪히는 collider 확인
                var hit = Physics2D.BoxCast(collider.bounds.center, collider.bounds.size,
                    0f, direction.normalized, direction.magnitude, obstacleLayerMask);
                if (hit) {
                    PickRandomDirection();
                }

                lastObstacleDetectionTimestamp = Time.time;
            }
        }

        protected virtual void PickRandomDirection() {
            direction.x = Random.Range(minimumRandomDirection.x, maximumRandomDirection.x);
            direction.y = Random.Range(minimumRandomDirection.y, maximumRandomDirection.y);
            lastDirectionChangeTimestamp = Time.time;
        }

        protected virtual void Move() {
            characterMovement.SetMovement(direction);
        }
    }
}