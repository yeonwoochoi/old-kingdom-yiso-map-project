using System.Collections;
using Character.Core;
using Controller.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;

namespace Character.Ability.Skill {
    [AddComponentMenu("Yiso/Character/Abilities/CharacterSkillEvasiveAssault")]
    public class YisoCharacterSkillEvasiveAssault : YisoCharacterSkillAttack {
        [Title("Skill Effect")] public GameObject evasiveAssaultPrefab;
        public bool useAfterImage = true;

        [Title("Duration")] public float dodgeDuration = 0.25f;
        public float delayAfterDodge = 0.25f;
        public float attackForwardDuration = 0.15f;
        public float delayAfterSkillCast = 0.15f;

        [Title("Distance")] public float dodgeBackDistance = 3f;
        public float attackForwardDistance = 4f;

        #region Override Property

        protected override Vector2 InitialAim => Vector2.down;
        protected override bool isDamageAreaAttachedToCharacter => true;
        public override bool CanMoveWhileCasting => false;
        public override bool CanAttackWhileCasting => false;
        public override bool CanDashWhileCasting => false;
        public override bool CanOverlapWithOtherSkills => false;
        public override bool FixOrientation => true;

        #endregion

        protected GameObject evasiveAssaultObj;
        protected Animator[] evasiveAssaultAnimators;
        protected float dodgeTimer = 0f;
        protected float attackForwardTimer = 0f;
        protected YisoAfterImageGenerator afterImageGenerator;

        protected const string EvasiveAssaultAnimationParameterName = "EvasiveAssault";
        protected const string EvasiveAssaultEffectAnimationParameterName = "Attack";
        protected const string EvasiveAssaultHorizontalAnimationParameterName = "X";
        protected const string EvasiveAssaultVerticalAnimationParameterName = "Y";
        protected int evasiveAssaultAnimationParameter;
        protected int evasiveAssaultEffectAnimationParameter;
        protected int evasiveAssaultHorizontalAnimationParameter;
        protected int evasiveAssaultVerticalAnimationParameter;

        #region Initialization

        protected override void PreInitialization() {
            base.PreInitialization();
            if (useAfterImage) {
                if (!character.characterModel.TryGetComponent(out afterImageGenerator)) {
                    afterImageGenerator = character.characterModel.AddComponent<YisoAfterImageGenerator>();
                }
            }
        }

        protected override void Initialization() {
            base.Initialization();
            if (evasiveAssaultPrefab == null) return;
            if (evasiveAssaultAnimators == null) {
                evasiveAssaultObj = Instantiate(evasiveAssaultPrefab, Vector3.zero, Quaternion.identity,
                    character.transform);
                evasiveAssaultObj.transform.localPosition = Vector3.zero;
                evasiveAssaultAnimators = evasiveAssaultObj.GetComponentsInChildren<Animator>();
            }

            if (useAfterImage) {
                afterImageGenerator.Initialization();
            }
        }

        #endregion

        #region Core

        protected override IEnumerator PerformSkillSequence(Transform target) {
            attackForwardTimer = 0f;
            dodgeTimer = 0f;

            // Set Orientation
            var currentDirection = GetCurrentDirection(target);
            var currentOrientation = YisoPhysicsUtils.GetDirectionFromVector(currentDirection);
            SkillAim.ApplyAim(currentDirection, true);
            character.Orientation2D.Face(currentOrientation);

            UpdateCharacterAnimationParameter();

            if (useAfterImage) afterImageGenerator.active = true;

            // Jump
            var dodgeStartPosition = character.characterModel.transform.position;
            var dodgeDestinationPosition = (Vector2) dodgeStartPosition - currentDirection * dodgeBackDistance;
            while (dodgeTimer < dodgeDuration) {
                controller.MovePosition(Vector2.Lerp(dodgeStartPosition, dodgeDestinationPosition,
                    dodgeTimer / dodgeDuration));
                dodgeTimer += Time.deltaTime;
                yield return null;
            }

            controller.MovePosition(dodgeDestinationPosition);

            if (useAfterImage) afterImageGenerator.active = false;

            yield return new WaitForSeconds(delayAfterDodge);

            // Attack Forward
            UpdateSkillEffectAnimationParameter();
            damageAreaSettings.EnableDamageArea();

            var attackDestinationPosition = dodgeDestinationPosition + currentDirection * attackForwardDistance;
            while (attackForwardTimer < attackForwardDuration) {
                controller.MovePosition(Vector2.Lerp(dodgeDestinationPosition, attackDestinationPosition,
                    attackForwardTimer / attackForwardDuration));
                attackForwardTimer += Time.deltaTime;
                yield return null;
            }

            controller.MovePosition(attackDestinationPosition);
            damageAreaSettings.DisableDamageArea();

            yield return new WaitForSeconds(delayAfterSkillCast);

            StopSkillCast(true);
        }

        #endregion

        protected override void InitializeAnimatorParameters() {
            base.InitializeAnimatorParameters();
            RegisterAnimatorParameter(EvasiveAssaultEffectAnimationParameterName,
                AnimatorControllerParameterType.Trigger, out evasiveAssaultEffectAnimationParameter);
            RegisterAnimatorParameter(EvasiveAssaultHorizontalAnimationParameterName,
                AnimatorControllerParameterType.Float, out evasiveAssaultHorizontalAnimationParameter);
            RegisterAnimatorParameter(EvasiveAssaultVerticalAnimationParameterName,
                AnimatorControllerParameterType.Float, out evasiveAssaultVerticalAnimationParameter);
            if (character.characterType == YisoCharacter.CharacterTypes.AI) {
                RegisterAnimatorParameter(EvasiveAssaultAnimationParameterName, AnimatorControllerParameterType.Bool,
                    out evasiveAssaultAnimationParameter);
            }
        }

        protected override void UpdateCharacterAnimationParameter() {
            base.UpdateCharacterAnimationParameter();
            if (character.characterType == YisoCharacter.CharacterTypes.AI) {
                YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, evasiveAssaultAnimationParameter, SkillCasting,
                    character.AnimatorParameters);
            }
        }

        protected virtual void UpdateSkillEffectAnimationParameter() {
            if (evasiveAssaultAnimators == null) return;
            foreach (var effectAnimator in evasiveAssaultAnimators) {
                YisoAnimatorUtils.UpdateAnimatorFloat(effectAnimator, evasiveAssaultHorizontalAnimationParameter,
                    character.Orientation2D.DirectionFineAdjustmentValue.x);
                YisoAnimatorUtils.UpdateAnimatorFloat(effectAnimator, evasiveAssaultVerticalAnimationParameter,
                    character.Orientation2D.DirectionFineAdjustmentValue.y);
                YisoAnimatorUtils.UpdateAnimatorTrigger(effectAnimator, evasiveAssaultEffectAnimationParameter);
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            if (useAfterImage) afterImageGenerator.DestroyAfterImages();
        }
    }
}