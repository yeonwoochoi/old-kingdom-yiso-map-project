using System.Collections;
using Character.Core;
using Controller.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;

namespace Character.Ability.Skill {
    [AddComponentMenu("Yiso/Character/Abilities/Character Skill Spin Attack")]
    public class YisoCharacterSkillSpinAttack : YisoCharacterSkillAttack {
        [Title("Dash")] public bool useDash = true;
        [ShowIf("useDash")] public float dashDistance = 3f;
        [ShowIf("useDash")] public AnimationCurve dashCurve = new(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        protected float skillTimer = 0f;
        protected Vector2 dashDestination;
        protected YisoAfterImageGenerator afterImageGenerator;

        protected readonly float skillDuration = 0.3f;
        protected readonly float startAngle = 270f;
        protected readonly float endAngle = -90f;

        protected const string SpinAttackAnimationParameterName = "SpinAttack";
        protected int spinAttackAnimationParameter;

        #region Override Property

        protected override Vector2 InitialAim => Vector2.down;
        protected override bool isDamageAreaAttachedToCharacter => true;
        public override bool CanMoveWhileCasting => false;
        public override bool CanAttackWhileCasting => false;
        public override bool CanDashWhileCasting => true;
        public override bool CanOverlapWithOtherSkills => false;
        public override bool FixOrientation => false;

        #endregion

        #region Initialization

        protected override void PreInitialization() {
            base.PreInitialization();

            if (useDash) {
                if (!character.characterModel.TryGetComponent(out afterImageGenerator)) {
                    afterImageGenerator = character.characterModel.AddComponent<YisoAfterImageGenerator>();
                }
            }
        }

        protected override void Initialization() {
            base.Initialization();
            if (useDash) {
                afterImageGenerator.Initialization();
            }
        }

        #endregion

        #region Core

        protected override IEnumerator PerformSkillSequence(Transform target) {
            skillTimer = 0f;
            SkillAim.ApplyAim(InitialAim, true);

            if (useDash) {
                // Set Dash Destination
                if (character.characterType == YisoCharacter.CharacterTypes.Player) {
                    dashDestination = character.characterModel.transform.position +
                                      controller.currentDirection.normalized * dashDistance;
                }
                else if (target != null) {
                    dashDestination = target.position;
                }
                else {
                    dashDestination = character.characterModel.transform.position;
                }

                afterImageGenerator.active = true;
            }

            UpdateCharacterAnimationParameter();
            damageAreaSettings.EnableDamageArea();

            while (skillTimer < skillDuration) {
                skillTimer += Time.deltaTime;
                var t = skillTimer / skillDuration;
                var currentAngle = Mathf.Lerp(startAngle, endAngle, t);
                SkillAim.SetCurrentAim(YisoMathUtils.DirectionFromAngle2D(currentAngle, 0f));
                yield return null;
            }

            if (useDash) {
                afterImageGenerator.active = false;
            }

            damageAreaSettings.DisableDamageArea();
            StopSkillCast(true);
        }

        #endregion

        #region Cycle

        public override void ProcessAbility() {
            base.ProcessAbility();
            if (SkillCasting && useDash) {
                if (skillTimer < skillDuration) {
                    var newPosition = Vector3.Lerp(character.characterModel.transform.position, dashDestination,
                        dashCurve.Evaluate(skillTimer / skillDuration));
                    controller.MovePosition(newPosition);
                }
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            afterImageGenerator.DestroyAfterImages();
        }

        #endregion

        protected override void InitializeAnimatorParameters() {
            base.InitializeAnimatorParameters();
            if (character.characterType == YisoCharacter.CharacterTypes.AI) {
                RegisterAnimatorParameter(SpinAttackAnimationParameterName, AnimatorControllerParameterType.Bool,
                    out spinAttackAnimationParameter);
            }
        }

        protected override void UpdateCharacterAnimationParameter() {
            base.UpdateCharacterAnimationParameter();
            if (character.characterType == YisoCharacter.CharacterTypes.AI) {
                YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, spinAttackAnimationParameter, SkillCasting,
                    character.AnimatorParameters);
            }
        }
    }
}