using Character.Ability;
using Character.Core;
using Manager;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.AI.Actions.Move {
    [AddComponentMenu("AI/Character/AI/Actions/AIActionMoveTowardsTarget")]
    public class YisoAIActionMoveTowardsTarget : YisoAIAction {
        public YisoAIBrain.AITargetType aiTargetType = YisoAIBrain.AITargetType.Main;
        public float minimumDistance = 2f; // 이 거리만큼 가까워지면 더이상 다가가지 않음
        [ReadOnly] public Vector2 currentDirection;


        protected YisoCharacter character;
        protected YisoCharacterMovement characterMovement;
        protected BoxCollider2D characterBoxCollider2D;

        private Vector2 slidingDirection;
        private Vector2 alternativeSlidingDirection;
        private float slidingTimer;

        private Vector2 prevPosition;
        private float stuckCheckTimer;
        private bool isStuckState = false;

        protected Vector2 colliderSize;
        protected Vector2 colliderOffset;
        protected Vector2 currentPosition;

        protected Seeker seeker;
        protected AIPath aiPath;
        protected AIDestinationSetter destinationSetter;

        private readonly float stuckCheckCycleTime = 1f; // 같은 위치에 계속 머무는 상태를 체크하는 주기
        private readonly float slidingLockTime = 0.5f; // 슬라이딩 방향을 고정할 시간
        private readonly float obstacleCheckDistance = 0.5f;
        private readonly LayerMask obstacleLayer = LayerManager.ObstaclesLayerMask | LayerManager.MapLayerMask;

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
            destinationSetter.target = Target;
            seeker.StartPath(brain.owner.transform.position, Target.position);
        }

        protected override void Awake() {
            base.Awake();
            character = gameObject.GetComponentInParent<YisoCharacter>();
            characterMovement = character?.FindAbility<YisoCharacterMovement>();
        }

        public override void Initialization() {
            base.Initialization();
            characterBoxCollider2D = brain.owner.gameObject.GetComponent<BoxCollider2D>();
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
            aiPath.endReachedDistance = minimumDistance;
            aiPath.maxSpeed = characterMovement != null ? characterMovement.MovementSpeed : 5f;
            aiPath.enableRotation = false;
        }

        protected virtual void MoveCharacter(Vector3 direction) {
            // Update sliding timer
            slidingTimer -= Time.deltaTime;

            currentPosition = (Vector2) brain.owner.transform.position + colliderOffset;
            Vector2 moveDirection = direction.normalized;

            // Check Obstacle
            var hit = Physics2D.BoxCast(currentPosition, colliderSize, 0f, moveDirection, obstacleCheckDistance,
                obstacleLayer);

            if (hit.collider != null) {
                if (slidingTimer <= 0f) {
                    // Calculate the potential sliding direction
                    var slidingDirection1 = Vector3.ProjectOnPlane(direction, hit.normal);
                    var slidingDirection2 = -slidingDirection1; // The opposite direction

                    // Choose the sliding direction that is closest to the original direction
                    (slidingDirection, alternativeSlidingDirection) =
                        GetSlidingDirection(direction, slidingDirection1, slidingDirection2);

                    var currentSlidingDirection = isStuckState ? alternativeSlidingDirection : slidingDirection;
                    if (IsValidSlidingDirection(currentSlidingDirection, hit.normal)) {
                        characterMovement.SetMovement(currentSlidingDirection);
                        currentDirection = currentSlidingDirection;
                    }
                    else {
                        // If the chosen direction is not valid, fallback to the original direction
                        characterMovement.SetMovement(direction);
                        currentDirection = direction;
                    }

                    slidingTimer = Random.Range(slidingLockTime, slidingLockTime * 2);
                }
            }
            else {
                // No obstacle detected, move in the original direction
                slidingTimer = 0f; // Reset timer
                currentDirection = direction;
                characterMovement.SetMovement(currentDirection);
            }

            HandleStuckState();

            (Vector2, Vector2) GetSlidingDirection(Vector2 originalDirection, Vector2 dir1, Vector2 dir2) {
                // Calculate the angle between the original direction and the sliding directions
                var angleToDir1 = Vector2.Angle(originalDirection, dir1);
                var angleToDir2 = Vector2.Angle(originalDirection, dir2);

                // Choose the sliding direction with the smallest angle to the original direction
                return angleToDir1 < angleToDir2 ? (dir1, dir2) : (dir2, dir1);
            }

            bool IsValidSlidingDirection(Vector3 dir, Vector3 hitNormal) {
                // 벡터 내적 (각에서 큰 차이가 나지 않으면 무시)
                return Vector3.Dot(dir, hitNormal) < 0.5f; // Allow a small tolerance
            }

            void HandleStuckState() {
                stuckCheckTimer -= Time.fixedDeltaTime;
                if (stuckCheckTimer <= 0) {
                    isStuckState = Vector2.Distance(currentPosition, prevPosition) < 0.1f;
                    prevPosition = currentPosition;
                    stuckCheckTimer = stuckCheckCycleTime;
                }
            }
        }

        public override void OnEnterState() {
            base.OnEnterState();
            if (brain.owner != null && Target != null) {
                seeker.StartPath(brain.owner.transform.position, Target.position);
            }

            if (!aiPath.enabled) aiPath.enabled = true;
            if (!destinationSetter.enabled) destinationSetter.enabled = true;

            colliderSize = characterBoxCollider2D.size;
            colliderOffset = characterBoxCollider2D.offset;
            isStuckState = false;
            slidingTimer = slidingLockTime;
            stuckCheckTimer = stuckCheckCycleTime;
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
    }
}