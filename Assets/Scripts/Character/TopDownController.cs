using Core.Behaviour;
using Sirenix.OdinInspector;
using Tools.Environment;
using UnityEngine;

namespace Character {
    public class TopDownController : RunIBehaviour {
        [ReadOnly] public Vector3 speed; // 한 Frame당 얼마나 이동했는지

        [ReadOnly] public Vector3
            currentMovement; // friction과 speed랑 계산되는 Vector 값 (크기 방향 모두) -> Player의 경우 Input Manager로부터 들어오는 값

        [ReadOnly] public Vector3 currentDirection; // current Movement Normalized된 값
        [ReadOnly] public float friction; // 마찰력
        [ReadOnly] public Vector3 addedForce; // 캐릭터 Dash 같은거 할때
        [ReadOnly] public bool freeMovement = true; // 조작 가능한 상태

        public virtual bool AllowImpact {
            get => allowImpact;
            set {
                impact = Vector3.zero;
                addedForce = Vector3.zero;
                allowImpact = value;
            }
        }

        public virtual Vector3 ColliderCenter => Vector3.zero;
        public virtual Vector3 ColliderBottom => Vector3.zero;
        public virtual Vector3 ColliderTop => Vector3.zero;

        public GameObject DetectedObstacleLeft { get; set; }
        public GameObject DetectedObstacleRight { get; set; }
        public GameObject DetectedObstacleUp { get; set; }
        public GameObject DetectedObstacleDown { get; set; }
        public bool CollidingWithCardinalObstacle { get; set; } // 어느 한 방향에서라도 장애물 감지되면 true
        public YisoSurfaceModifier SurfaceModifierBelow { get; set; }

        protected Vector3 positionLastFrame; // 이전 프레임에서 character 위치
        protected bool allowImpact = true;
        protected Vector3 impact;

        protected override void Awake() {
            currentDirection = transform.forward;
        }

        public override void OnUpdate() {
            HandleFriction();
            DetermineDirection();
        }

        public override void OnFixedUpdate() {
        }

        public override void OnLateUpdate() {
        }

        protected virtual void ComputeSpeed() {
            if (Time.deltaTime != 0f) speed = (transform.position - positionLastFrame) / Time.deltaTime;

            speed.x = Mathf.Round(speed.x * 100f) / 100f;
            speed.y = Mathf.Round(speed.y * 100f) / 100f;
            speed.z = Mathf.Round(speed.z * 100f) / 100f;
            positionLastFrame = transform.position;
        }

        protected virtual void DetermineDirection() {
        }

        public virtual void DetectObstacles(float distance, Vector3 offset) {
        }

        public virtual void Impact(Vector3 direction, float force) {
        }

        public virtual void Impact(Vector3 movement) {
        }

        public virtual void SetMovement(Vector3 movement) {
        }

        public virtual void MovePosition(Vector3 newPosition) {
        }

        public virtual void CollisionsOn() {
        }

        public virtual void CollisionsOff() {
        }

        protected virtual void HandleFriction() {
        }

        public virtual void Reset() {
            impact = Vector3.zero;
            speed = Vector3.zero;
            currentMovement = Vector3.zero;
            currentDirection = Vector3.zero;
            addedForce = Vector3.zero;
        }
    }
}