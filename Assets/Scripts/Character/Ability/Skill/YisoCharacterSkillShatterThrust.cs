using System.Collections;
using Character.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;

namespace Character.Ability.Skill {
    [AddComponentMenu("Yiso/Character/Abilities/CharacterSkillShatterThrust")]
    public class YisoCharacterSkillShatterThrust : YisoCharacterSkillAttack {
        [Title("Skill Effect Prefab")] public GameObject skillChargeEffectPrefab;
        public GameObject skillThrustEffectPrefab;

        [Title("Thrust Settings")] public float dashDistance = 3f;
        public AnimationCurve dashCurve = new(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        protected GameObject skillChargeEffectObj;
        protected GameObject skillThrustEffectObj;
        protected Animator[] skillChargeEffectAnimators;
        protected Animator[] skillThrustEffectAnimators;

        protected Vector2 dashDestination;
        protected float skillTimer = 0f;
        protected readonly float chargeDuration = 0.3f;
        protected readonly float skillDuration = 0.3f;

        protected const string ShatterThrustAnimationParameterName = "ShatterThrust";
        protected const string ChargeEffectAnimationParameterName = "Charge";
        protected const string ThrustEffectAnimationParameterName = "Thrust";
        protected int shatterThrustAnimationParameter;
        protected int chargeEffectAnimationParameter;
        protected int thrustEffectAnimationParameter;

        #region Override Property

        protected override Vector2 InitialAim => Vector2.down;
        protected override bool isDamageAreaAttachedToCharacter => true;
        public override bool CanMoveWhileCasting => false;
        public override bool CanAttackWhileCasting => false;
        public override bool CanDashWhileCasting => false;
        public override bool CanOverlapWithOtherSkills => false;
        public override bool FixOrientation => false;

        #endregion

        protected override void Initialization() {
            base.Initialization();
            if (skillChargeEffectAnimators == null) {
                skillChargeEffectObj = Instantiate(skillChargeEffectPrefab, Vector3.zero, Quaternion.identity,
                    character.transform);
                skillChargeEffectObj.transform.localPosition = Vector3.zero;
                skillChargeEffectAnimators = skillChargeEffectObj.GetComponentsInChildren<Animator>();
            }

            if (skillThrustEffectAnimators == null) {
                skillThrustEffectObj = Instantiate(skillThrustEffectPrefab, Vector3.zero, Quaternion.identity,
                    character.transform);
                skillThrustEffectObj.transform.localPosition = Vector3.zero;
                skillThrustEffectAnimators = skillThrustEffectObj.GetComponentsInChildren<Animator>();
            }
        }

        protected override IEnumerator PerformSkillSequence(Transform target) {
            // Set Skill Aim
            skillTimer = 0f;

            // Set Orientation
            var currentDirection = GetCurrentDirection(target);
            var currentOrientation = YisoPhysicsUtils.GetDirectionFromVector(currentDirection);
            SkillAim.ApplyAim(currentDirection, true);
            character.Orientation2D.Face(currentOrientation);

            skillThrustEffectObj.transform.localRotation = GetThrustEffectObjRotationByDirection(currentOrientation);
            skillThrustEffectObj.transform.localPosition = GetThrustEffectObjPositionByDirection(currentOrientation);

            // Charge Animation
            UpdateCharacterAnimationParameter();
            UpdateChargeEffectAnimationParameter();

            yield return new WaitForSeconds(chargeDuration);

            // Dash
            if (character.characterType == YisoCharacter.CharacterTypes.Player || target == null) {
                dashDestination = character.characterModel.transform.position +
                                  controller.currentDirection.normalized * dashDistance;
            }
            else {
                dashDestination = target.transform.position;
            }

            UpdateThrustEffectAnimationParameter();
            damageAreaSettings.EnableDamageArea();

            while (skillTimer < skillDuration) {
                skillTimer += Time.deltaTime;
                yield return null;
            }

            damageAreaSettings.DisableDamageArea();
            StopSkillCast(true);
        }

        public override void ProcessAbility() {
            base.ProcessAbility();
            if (SkillCasting) {
                if (skillTimer < skillDuration) {
                    var newPosition = Vector3.Lerp(character.characterModel.transform.position, dashDestination,
                        dashCurve.Evaluate(skillTimer / skillDuration));
                    controller.MovePosition(newPosition);
                }
            }
        }

        #region Getter

        private Vector2 GetThrustEffectObjPositionByDirection(YisoCharacter.FacingDirections direction) {
            return direction switch {
                YisoCharacter.FacingDirections.West => new Vector2(-0.5f, 0.3f),
                YisoCharacter.FacingDirections.East => new Vector2(0.5f, 0f),
                YisoCharacter.FacingDirections.North => new Vector2(0.35f, 0.8f),
                YisoCharacter.FacingDirections.South => new Vector2(-0.17f, 0f),
                _ => Vector2.zero
            };
        }

        private Quaternion GetThrustEffectObjRotationByDirection(YisoCharacter.FacingDirections direction) {
            return direction switch {
                YisoCharacter.FacingDirections.West => Quaternion.Euler(0, 0, 0f),
                YisoCharacter.FacingDirections.East => Quaternion.Euler(0, 0, 180f),
                YisoCharacter.FacingDirections.North => Quaternion.Euler(0, 0, 270f),
                YisoCharacter.FacingDirections.South => Quaternion.Euler(0, 0, 90f),
                _ => Quaternion.identity
            };
        }

        #endregion

        #region Animator

        protected override void InitializeAnimatorParameters() {
            base.InitializeAnimatorParameters();
            RegisterAnimatorParameter(ChargeEffectAnimationParameterName, AnimatorControllerParameterType.Trigger,
                out chargeEffectAnimationParameter);
            RegisterAnimatorParameter(ThrustEffectAnimationParameterName, AnimatorControllerParameterType.Trigger,
                out thrustEffectAnimationParameter);
            if (character.characterType == YisoCharacter.CharacterTypes.AI) {
                RegisterAnimatorParameter(ShatterThrustAnimationParameterName, AnimatorControllerParameterType.Bool,
                    out shatterThrustAnimationParameter);
            }
        }

        protected override void UpdateCharacterAnimationParameter() {
            base.UpdateCharacterAnimationParameter();
            if (character.characterType == YisoCharacter.CharacterTypes.AI) {
                YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, shatterThrustAnimationParameter, SkillCasting,
                    character.AnimatorParameters);
            }
        }

        protected virtual void UpdateChargeEffectAnimationParameter() {
            if (skillChargeEffectAnimators == null) return;
            foreach (var chargeEffectAnimator in skillChargeEffectAnimators) {
                YisoAnimatorUtils.UpdateAnimatorTrigger(chargeEffectAnimator, chargeEffectAnimationParameter);
            }
        }

        protected virtual void UpdateThrustEffectAnimationParameter() {
            if (skillThrustEffectAnimators == null) return;
            foreach (var thrustEffectAnimator in skillThrustEffectAnimators) {
                YisoAnimatorUtils.UpdateAnimatorTrigger(thrustEffectAnimator, thrustEffectAnimationParameter);
            }
        }

        #endregion
    }
}