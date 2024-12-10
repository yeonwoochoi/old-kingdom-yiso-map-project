using System;
using System.Collections;
using Character.Core;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;
using Random = UnityEngine.Random;

namespace Character.Ability.Skill {
    [AddComponentMenu("Yiso/Character/Abilities/CharacterSkillTeleportAttack")]
    public class YisoCharacterSkillTeleportAttack : YisoCharacterSkillAttack {
        [Title("Skill Effect Prefab")] public GameObject teleportEffectPrefab;
        public GameObject teleportAttackEffectPrefab;

        [Title("Teleport Settings")] public LayerMask avoidLayerMask =
            LayerManager.MapLayerMask | LayerManager.ObstaclesLayerMask | LayerManager.EnemiesLayerMask;

        public float teleportDistance = 1f;
        public int maxAttempts = 10;

        protected GameObject teleportEffectObj;
        protected GameObject teleportAttackEffectObj;
        protected Animator[] teleportEffectAnimators;
        protected Animator[] teleportAttackEffectAnimators;
        protected SpriteRenderer[] teleportAttackEffectRenderers;

        protected Vector2 targetForward;
        protected Vector2 teleportPosition;

        protected const string TeleportAttackAnimationParameterName = "TeleportAttack";
        protected const string TeleportEffectAnimationParameterName = "Teleport";
        protected const string AttackEffectAnimationParameterName = "Attack";
        protected int teleportAttackAnimationParameter;
        protected int teleportEffectAnimationParameter;
        protected int attackEffectAnimationParameter;

        protected readonly float teleportAttackDelay = 0.1f;
        protected readonly float skillDuration = 0.3f;
        protected readonly Vector3 teleportEffectOffset = new Vector3(0f, -0.35f, 0f);

        #region Override Property

        protected override Vector2 InitialAim => Vector2.down;
        protected override bool isDamageAreaAttachedToCharacter => true;
        public override bool CanMoveWhileCasting => false;
        public override bool CanAttackWhileCasting => false;
        public override bool CanDashWhileCasting => false;
        public override bool CanOverlapWithOtherSkills => false;
        public override bool FixOrientation => true;

        #endregion

        protected override void Initialization() {
            base.Initialization();

            if (teleportEffectAnimators == null) {
                teleportEffectObj = Instantiate(teleportEffectPrefab, Vector3.zero, Quaternion.identity);
                teleportEffectAnimators = teleportEffectObj.GetComponentsInChildren<Animator>();
            }

            if (teleportAttackEffectAnimators == null) {
                teleportAttackEffectObj = Instantiate(teleportAttackEffectPrefab, Vector3.zero, Quaternion.identity,
                    character.transform);
                teleportAttackEffectObj.transform.localPosition = Vector3.zero;
                teleportAttackEffectAnimators = teleportAttackEffectObj.GetComponentsInChildren<Animator>();
                teleportAttackEffectRenderers = teleportAttackEffectObj.GetComponentsInChildren<SpriteRenderer>();
            }
        }

        protected override IEnumerator PerformSkillSequence(Transform target) {
            // Target이 바라보는 방향 Get
            if (target == null) {
                targetForward = character.Orientation2D.DirectionFineValue.normalized;
                teleportPosition = character.characterModel.transform.position;
            }
            else {
                var targetOrientation2D =
                    target.GetComponent<YisoCharacter>()?.FindAbility<YisoCharacterOrientation2D>();
                if (targetOrientation2D == null) {
                    targetForward = (character.characterModel.transform.position - target.position).normalized;
                }
                else {
                    targetForward = targetOrientation2D.DirectionFineValue.normalized;
                }

                // Teleport 위치 Get (teleport 위치 못 찾으면 제자리 이동)
                yield return StartCoroutine(FindSafePositionBehindTarget(target.position, targetForward,
                    (position, success) => {
                        teleportPosition = success ? position : character.characterModel.transform.position;
                    }));
            }

            yield return null;

            // Teleport Animation + Fade in
            teleportEffectObj.transform.position = character.characterModel.transform.position + teleportEffectOffset;
            UpdateTeleportEffectAnimationParameter();
            // character.SetCharacterVisible(false);

            yield return null;

            // Teleport
            character.transform.position = teleportPosition;

            yield return new WaitForSeconds(teleportAttackDelay);

            // Apply Aim
            Vector3 currentAim;
            if (target == null) {
                currentAim = targetForward;
            }
            else {
                currentAim = target.position - character.characterModel.transform.position;
            }

            SkillAim.ApplyAim(currentAim, true);
            character.Orientation2D.Face(YisoPhysicsUtils.GetDirectionFromVector(currentAim));

            // Fade out && Attack animation
            // character.SetCharacterVisible(true);
            foreach (var teleportAttackEffectRenderer in teleportAttackEffectRenderers) {
                teleportAttackEffectRenderer.flipX = character.Orientation2D.DirectionFineAdjustmentValue.x >= 0f;
            }

            UpdateCharacterAnimationParameter();
            UpdateTeleportAttackEffectAnimationParameter();

            // Set Damage Area
            damageAreaSettings.EnableDamageArea();
            yield return new WaitForSeconds(skillDuration);
            damageAreaSettings.DisableDamageArea();

            StopSkillCast(true);
        }

        private IEnumerator FindSafePositionBehindTarget(Vector3 targetPosition, Vector3 targetForward,
            Action<Vector3, bool> onResult) {
            var directionBehindTarget = -targetForward.normalized;
            var success = false;
            var teleportPosition = targetPosition;

            // 여러 시도를 통해 적절한 위치를 찾음
            for (var attempt = 0; attempt < maxAttempts; attempt++) {
                // 시도할 위치 계산
                teleportPosition = targetPosition + directionBehindTarget * teleportDistance;

                // 충돌이 있는지 확인
                var hitCollider = Physics2D.OverlapCircle(teleportPosition, 0.5f, avoidLayerMask);

                if (hitCollider == null) {
                    // 충돌이 없다면 안전한 위치이므로 해당 위치 반환
                    success = true;
                    break;
                }
                else {
                    // 충돌이 있다면 약간 위치를 수정해서 다시 시도 (랜덤한 오프셋 추가)
                    directionBehindTarget = Quaternion.Euler(0, 0, Random.Range(-45f, 45f)) * directionBehindTarget;
                    yield return null;
                }
            }

            onResult?.Invoke(teleportPosition, success);
        }

        #region Animator

        protected override void InitializeAnimatorParameters() {
            base.InitializeAnimatorParameters();
            RegisterAnimatorParameter(TeleportEffectAnimationParameterName, AnimatorControllerParameterType.Trigger,
                out teleportEffectAnimationParameter);
            RegisterAnimatorParameter(AttackEffectAnimationParameterName, AnimatorControllerParameterType.Trigger,
                out attackEffectAnimationParameter);
            if (character.characterType == YisoCharacter.CharacterTypes.AI) {
                RegisterAnimatorParameter(TeleportAttackAnimationParameterName, AnimatorControllerParameterType.Bool,
                    out teleportAttackAnimationParameter);
            }
        }

        protected override void UpdateCharacterAnimationParameter() {
            base.UpdateCharacterAnimationParameter();
            if (character.characterType == YisoCharacter.CharacterTypes.AI) {
                YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, teleportAttackAnimationParameter, SkillCasting,
                    character.AnimatorParameters);
            }
        }

        protected virtual void UpdateTeleportEffectAnimationParameter() {
            if (teleportEffectAnimators == null) return;
            foreach (var teleportEffectAnimator in teleportEffectAnimators) {
                YisoAnimatorUtils.UpdateAnimatorTrigger(teleportEffectAnimator, teleportEffectAnimationParameter);
            }
        }

        protected virtual void UpdateTeleportAttackEffectAnimationParameter() {
            if (teleportAttackEffectAnimators == null) return;
            foreach (var teleportAttackEffectAnimator in teleportAttackEffectAnimators) {
                YisoAnimatorUtils.UpdateAnimatorTrigger(teleportAttackEffectAnimator, attackEffectAnimationParameter);
            }
        }

        #endregion
    }
}