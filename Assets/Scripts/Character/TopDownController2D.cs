using Manager;
using UnityEngine;
using Utils.Beagle;

namespace Character {
    public class TopDownController2D : TopDownController {
        public override Vector3 ColliderCenter => (Vector2) transform.position + ColliderOffset;

        public override Vector3 ColliderBottom =>
            (Vector2) transform.position + ColliderOffset + Vector2.down * ColliderBounds.extents.y;

        public override Vector3 ColliderTop =>
            (Vector2) transform.position + ColliderOffset + Vector2.up * ColliderBounds.extents.y;

        protected readonly LayerMask groundLayerMask = LayerManager.GroundLayerMask;
        protected readonly LayerMask obstaclesLayerMask = LayerManager.ObstaclesLayerMask | LayerManager.MapLayerMask;

        public Vector2 ColliderSize {
            get {
                if (!boxColliderNull) return boxCollider2D.size;
                if (!capsuleColliderNull) return capsuleCollider2D.size;
                if (!circleColliderNull) return circleCollider2D.radius * Vector2.one;
                return Vector2.zero;
            }
            set {
                if (!boxColliderNull) {
                    boxCollider2D.size = value;
                    return;
                }

                if (!capsuleColliderNull) {
                    capsuleCollider2D.size = value;
                    return;
                }

                if (!circleColliderNull) {
                    circleCollider2D.radius = value.x;
                    return;
                }
            }
        }

        public Vector2 ColliderOffset {
            get {
                if (!boxColliderNull) return boxCollider2D.offset;
                if (!capsuleColliderNull) return capsuleCollider2D.offset;
                if (!circleColliderNull) return circleCollider2D.offset;
                return Vector2.zero;
            }
            set {
                if (!boxColliderNull) {
                    boxCollider2D.offset = value;
                    return;
                }

                if (!capsuleColliderNull) {
                    capsuleCollider2D.offset = value;
                    return;
                }

                if (!circleColliderNull) {
                    circleCollider2D.offset = value;
                    return;
                }
            }
        }

        public Bounds ColliderBounds {
            get {
                if (!boxColliderNull) return boxCollider2D.bounds;
                if (!capsuleColliderNull) return capsuleCollider2D.bounds;
                if (!circleColliderNull) return circleCollider2D.bounds;
                return new Bounds();
            }
        }

        public override bool AllowImpact {
            get => base.AllowImpact;
            set {
                rigidBody2D.velocity = Vector2.zero;
                base.AllowImpact = value;
            }
        }

        protected Rigidbody2D rigidBody2D;
        protected BoxCollider2D boxCollider2D;
        protected CapsuleCollider2D capsuleCollider2D;
        protected CircleCollider2D circleCollider2D;
        protected bool boxColliderNull;
        protected bool capsuleColliderNull;
        protected bool circleColliderNull;

        protected Vector2 originalColliderSize;
        protected Vector3 originalColliderCenter;
        protected Vector3 orientedMovement; // temp Movement 라 봐도 무방.

        protected RaycastHit2D raycastUp;
        protected RaycastHit2D raycastDown;
        protected RaycastHit2D raycastLeft;
        protected RaycastHit2D raycastRight;

        protected readonly float defaultImpactTime = 5f;

        protected override void Awake() {
            base.Awake();
            rigidBody2D = GetComponent<Rigidbody2D>();
            boxCollider2D = GetComponent<BoxCollider2D>();
            capsuleCollider2D = GetComponent<CapsuleCollider2D>();
            circleCollider2D = GetComponent<CircleCollider2D>();
            boxColliderNull = boxCollider2D == null;
            capsuleColliderNull = capsuleCollider2D == null;
            circleColliderNull = circleCollider2D == null;
            originalColliderSize = ColliderSize;
            originalColliderCenter = ColliderOffset;
        }

        public override void OnFixedUpdate() {
            base.OnFixedUpdate();

            if (AllowImpact) {
                ApplyImpact();
            }

            if (!freeMovement) return;

            if (friction > 1) currentMovement /= friction;
            if (friction is > 0 and < 1) currentMovement = Vector3.Lerp(speed, currentMovement, Time.deltaTime * friction);
            MovePosition(rigidBody2D.position + (Vector2) (currentMovement + addedForce) * Time.fixedDeltaTime);
        }

        public override void OnLateUpdate() {
            base.OnLateUpdate();
            ComputeSpeed();
        }

        // 얼음 바닥 같이 마찰력이 다른 Ground를 지나갈시
        protected override void HandleFriction() {
            if (SurfaceModifierBelow == null) {
                friction = 0f;
                addedForce = Vector3.zero;
            }
            else {
                friction = SurfaceModifierBelow.friction;
                if (addedForce.y != 0f) Impact(addedForce); // 이 부분 이해 안됨.
                addedForce.y = 0f;
                addedForce = SurfaceModifierBelow.addedForce;
            }
        }

        /// <summary>
        /// 특정 방향으로 Impact 주고 싶으면 이거 쓰면 됨 (KnockBack)
        /// </summary>
        /// <param name="movement"></param>
        public override void Impact(Vector3 movement) {
            Impact(movement.normalized, movement.magnitude);
        }

        public override void Impact(Vector3 direction, float force) {
            if (!AllowImpact) return;
            direction = direction.normalized;
            impact += direction.normalized * force;
        }

        protected virtual void ApplyImpact() {
            if (impact.magnitude > 0.2f) rigidBody2D.AddForce(impact);
            impact = Vector3.Lerp(impact, Vector3.zero, defaultImpactTime * Time.deltaTime);
        }

        // x, z 값으로 들어옴 -> x,y 로 변환해서 적용
        public override void SetMovement(Vector3 movement) {
            orientedMovement = movement;
            orientedMovement.y = orientedMovement.z;
            orientedMovement.z = 0f;
            currentMovement = orientedMovement;
        }

        public override void MovePosition(Vector3 newPosition) {
            if (rigidBody2D.bodyType != RigidbodyType2D.Static) rigidBody2D.MovePosition(newPosition);
        }

        protected override void DetermineDirection() {
            if (currentMovement != Vector3.zero) {
                currentDirection = currentMovement.normalized;
            }
        }

        public override void CollisionsOn() {
            if (!boxColliderNull) boxCollider2D.enabled = true;
            if (!capsuleColliderNull) capsuleCollider2D.enabled = true;
            if (!circleColliderNull) circleCollider2D.enabled = true;
        }

        public override void CollisionsOff() {
            if (!boxColliderNull) boxCollider2D.enabled = false;
            if (!capsuleColliderNull) capsuleCollider2D.enabled = false;
            if (!circleColliderNull) circleCollider2D.enabled = false;
        }

        public override void DetectObstacles(float distance, Vector3 offset) {
            raycastRight = YisoPhysicsUtils.RayCast(transform.position + offset, Vector3.right, distance,
                obstaclesLayerMask, Color.yellow, true);
            DetectedObstacleRight = raycastRight.collider != null ? raycastRight.collider.gameObject : null;

            raycastLeft = YisoPhysicsUtils.RayCast(transform.position + offset, Vector3.left, distance,
                obstaclesLayerMask, Color.yellow, true);
            DetectedObstacleLeft = raycastLeft.collider != null ? raycastLeft.collider.gameObject : null;

            raycastUp = YisoPhysicsUtils.RayCast(transform.position + offset, Vector3.up, distance, obstaclesLayerMask,
                Color.yellow, true);
            DetectedObstacleUp = raycastUp.collider != null ? raycastUp.collider.gameObject : null;

            raycastDown = YisoPhysicsUtils.RayCast(transform.position + offset, Vector3.down, distance,
                obstaclesLayerMask, Color.yellow, true);
            DetectedObstacleDown = raycastDown.collider != null ? raycastDown.collider.gameObject : null;
        }

        public override void Reset() {
            base.Reset();
            if (rigidBody2D != null && rigidBody2D.bodyType != RigidbodyType2D.Static) {
                rigidBody2D.velocity = Vector2.zero;
            }
        }
    }
}