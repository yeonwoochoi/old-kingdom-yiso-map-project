using UnityEngine;
using Utils.Beagle;
using Sirenix.OdinInspector;

namespace Character.Weapon.Projectiles {
    [AddComponentMenu("Yiso/Weapons/Projectile/Arrow")]
    public class YisoArrow : YisoProjectile {
        [ReadOnly] public float currentSpeed = 0f;

        protected Animator animator;
        private Vector3 previousPosition = Vector3.zero;
        private const float WedgedSpeedThreshold = 0.5f; // Projectile

        protected const string WedgedAnimationParameterName = "Wedged";
        protected int wedgedAnimationParameter;

        protected override void Awake() {
            base.Awake();
            animator = GetComponent<Animator>();
            wedgedAnimationParameter = Animator.StringToHash(WedgedAnimationParameterName);
        }

        protected override void Initialization() {
            base.Initialization();
            previousPosition = transform.position;
        }

        public override void OnFixedUpdate() {
            if (canMove) {
                Move();
                CalculateVelocity();
                UpdateAnimator();
                DisableCollider();
            }
        }

        protected virtual void CalculateVelocity() {
            // 현재 프레임의 위치
            var currentPosition = transform.position;

            // 이전 프레임과의 거리 차이를 이용하여 속도 계산
            var velocity = (currentPosition - previousPosition) / Time.fixedDeltaTime;

            // 속도를 저장
            currentSpeed = velocity.magnitude;

            // 현재 위치를 다음 프레임에서 사용하기 위해 업데이트
            previousPosition = currentPosition;
        }

        protected virtual void UpdateAnimator() {
            YisoAnimatorUtils.UpdateAnimatorBool(animator, wedgedAnimationParameter,
                currentSpeed < WedgedSpeedThreshold);
        }

        protected virtual void DisableCollider() {
            if (collider2D == null) return;
            collider2D.enabled = !(currentSpeed < 0.01f);
        }
    }
}