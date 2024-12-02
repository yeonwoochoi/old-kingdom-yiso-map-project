using System.Collections;
using Character.Ability;
using Sirenix.OdinInspector;
using Tools.Cool;
using UnityEngine;
using Utils.Beagle;

namespace Tools.AI.Actions.Attack {
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionDashAttack")]
    public class YisoAIActionDashAttack : YisoAIActionAttack {
        public struct DashDestinationInfo {
            public Vector3 position;
            public float time;

            public DashDestinationInfo(Vector3 position, float time) {
                this.position = position;
                this.time = time;
            }
        }

        [Title("Dash")] [Range(0f, 5f)]
        public float dashDestinationTimeGap = 0.5f; // Dash Destination은 Dash 하기 몇 초전 위치인지

        public bool forceDashAbilitySetting = false;
        [ShowIf("forceDashAbilitySetting")] public float dashDistance = 2.1f;
        [ShowIf("forceDashAbilitySetting")] public float dashDuration = 0.2f;

        [ShowIf("forceDashAbilitySetting")]
        public AnimationCurve dashCurve = new(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        [ShowIf("forceDashAbilitySetting")] public YisoCooldown cooldown;

        [Title("Delay")] public float delayBeforeDash = 0.5f;

        [Title("Timing")] [Range(0f, 1f), Tooltip("Dash가 몇 퍼센트 진행되었을 때 Attack 함수를 실행할건지 (ex. 80% => 0.8f)")]
        public float dashAttackTiming = 0.8f;

        protected YisoCharacterDash characterDash;
        protected bool dashing = false;
        protected DashDestinationInfo[] dashDestinationInfo;
        protected Vector3 dashDestination;
        protected readonly int dashDestinationCapacity = 10;
        protected int dashDestinationIndex = 0;

        protected const string DashAttackReadyAnimationParameterName = "DashAttackReady";
        protected const string DashAttackAnimationParameterName = "DashAttack";
        protected int dashAttackReadyAnimationParameter;
        protected int dashAttackAnimationParameter;

        protected override bool FaceTargetWhenAttacking => true;
        protected override bool AimAtTargetWhenEnterState => true;
        protected override bool AimAtTargetDuringState => false;

        public override void Initialization() {
            if (!ShouldInitialize) return;
            base.Initialization();

            dashAttackReadyAnimationParameter = Animator.StringToHash(DashAttackReadyAnimationParameterName);
            dashAttackAnimationParameter = Animator.StringToHash(DashAttackAnimationParameterName);
            characterDash = character.FindAbility<YisoCharacterDash>();
            dashDestinationInfo = new DashDestinationInfo[dashDestinationCapacity];
            dashing = false;

            if (forceDashAbilitySetting) {
                characterDash.dashDistance = dashDistance;
                characterDash.dashDuration = dashDuration;
                characterDash.dashCurve = dashCurve;
                characterDash.cooldown = cooldown;
                characterDash.cooldown.Initialization();
            }

            StartCoroutine(SaveDestination());
        }

        public override void OnEnterState() {
            numberOfShoots = 0;
            attacking = true;
            InitializeWeapon();
            CalculateDestination();
            AimAtTarget();
            SetWeaponInterrupted();
            FaceTarget(weaponAimDirection);
            YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, dashAttackAnimationParameter, false);
            YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, dashAttackReadyAnimationParameter, true);
        }

        public override void OnExitState() {
            base.OnExitState();
            YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, dashAttackAnimationParameter, false);
            YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, dashAttackReadyAnimationParameter, false);
            dashing = false;
            characterDash.DashStop();
        }

        public override void PerformAction() {
            StartCoroutine(DashCo());
        }

        protected override void AimAtTarget() {
            weaponAimDirection = dashDestination - transform.position;
        }

        protected virtual IEnumerator DashCo() {
            if (!initialized || !characterDash.cooldown.Ready || dashing) yield break;
            if (character.Animator == null) yield break;

            dashing = true;
            YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, dashAttackReadyAnimationParameter, true);
            yield return new WaitForSeconds(delayBeforeDash);
            characterDash.DashStart(dashDestination);

            while (dashing) {
                if (characterDash.GetDashProgress() > dashAttackTiming) {
                    Attack();
                    YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, dashAttackAnimationParameter, true);
                    YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, dashAttackReadyAnimationParameter, false);
                    break;
                }

                yield return null;
            }

            dashing = false;
        }

        protected virtual IEnumerator SaveDestination() {
            while (enabled) {
                if (Target != null) {
                    if (dashDestinationIndex >= dashDestinationCapacity) {
                        dashDestinationIndex = 0;
                    }

                    dashDestinationInfo[dashDestinationIndex] =
                        new DashDestinationInfo(Target.transform.position, Time.time);
                    yield return new WaitForSeconds(dashDestinationTimeGap);
                }
                else {
                    yield return null;
                }
            }
        }

        protected virtual void CalculateDestination() {
            var gap = dashDestinationTimeGap - delayBeforeDash;
            if (gap <= 0) {
                dashDestination = transform.position +
                                  (Target.transform.position - transform.position).normalized * dashDistance;
                return;
            }

            // TODO : 저장된 Destination을 꺼내는 시점이 Attack하는 시간 기준이 아니라 State Enter하는 시간을 기준으로 함. (나중에 다시 테스트해보기)
            // 그럴 수 밖에 없는게 Dash 전에 미리 도착지점을 지정해놔야 방향을 정하지
            if (dashDestinationInfo == null || dashDestinationInfo.Length < 1) {
                dashDestination = transform.position;
                return;
            }

            var closestDestination = dashDestinationInfo[0];
            var targetSaveTime = Time.time - gap;
            var timeOffset = 1000f;

            foreach (var info in dashDestinationInfo) {
                if (Mathf.Abs(info.time - targetSaveTime) < timeOffset) {
                    closestDestination = info;
                    timeOffset = Mathf.Abs(info.time - targetSaveTime);
                }
            }

            dashDestination = transform.position +
                              (closestDestination.position - transform.position).normalized * dashDistance;
        }
    }
}