using System.Collections;
using Character.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;

namespace Character.Ability.Skill {
    [AddComponentMenu("Yiso/Character/Abilities/CharacterSkillDaggerTempest")]
    public class YisoCharacterSkillDaggerTempest : YisoCharacterSkillAttack {
        [Title("Skill Effect Prefab")] public GameObject daggerTempestPrefab;

        #region Override Property

        protected override Vector2 InitialAim => Vector2.down;
        protected override bool isDamageAreaAttachedToCharacter => true;
        public override bool CanMoveWhileCasting => false;
        public override bool CanAttackWhileCasting => false;
        public override bool CanDashWhileCasting => false;
        public override bool CanOverlapWithOtherSkills => false;
        public override bool FixOrientation => false;

        #endregion

        protected GameObject daggerTempestObj;
        protected Animator[] daggerTempestAnimators;
        protected float skillTimer = 0f;
        protected readonly float skillDuration = 0.8f;

        protected const string DaggerTempestAnimationParameterName = "DaggerTempest";
        protected const string DaggerTempestEffectAnimationParameterName = "Attack";
        protected int daggerTempestAnimationParameter;
        protected int daggerTempestEffectAnimationParameter;

        #region Initialization

        protected override void PreInitialization() {
            base.PreInitialization();
            invincibleWhileCasting = true;
        }

        protected override void Initialization() {
            base.Initialization();
            if (daggerTempestAnimators == null) {
                daggerTempestObj = Instantiate(daggerTempestPrefab, Vector3.zero, Quaternion.identity,
                    character.transform);
                daggerTempestObj.transform.localPosition = Vector3.zero;
                daggerTempestAnimators = daggerTempestObj.GetComponentsInChildren<Animator>();
            }
        }

        #endregion

        #region Core

        protected override IEnumerator PerformSkillSequence(Transform target) {
            skillTimer = 0f;
            SkillAim.ApplyAim(InitialAim, true);

            UpdateCharacterAnimationParameter();
            UpdateSkillEffectAnimationParameter();
            damageAreaSettings.EnableDamageArea();

            while (skillTimer < skillDuration) {
                skillTimer += Time.deltaTime;
                yield return null;
            }

            damageAreaSettings.DisableDamageArea();
            StopSkillCast(true);
        }

        #endregion

        protected override void InitializeAnimatorParameters() {
            base.InitializeAnimatorParameters();
            RegisterAnimatorParameter(DaggerTempestEffectAnimationParameterName,
                AnimatorControllerParameterType.Trigger, out daggerTempestEffectAnimationParameter);
            if (character.characterType == YisoCharacter.CharacterTypes.AI) {
                RegisterAnimatorParameter(DaggerTempestAnimationParameterName, AnimatorControllerParameterType.Bool,
                    out daggerTempestAnimationParameter);
            }
        }

        protected override void UpdateCharacterAnimationParameter() {
            base.UpdateCharacterAnimationParameter();
            if (character.characterType == YisoCharacter.CharacterTypes.AI) {
                YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, daggerTempestAnimationParameter, SkillCasting,
                    character.AnimatorParameters);
            }
        }

        protected virtual void UpdateSkillEffectAnimationParameter() {
            if (daggerTempestAnimators == null) return;
            foreach (var chargeEffectAnimator in daggerTempestAnimators) {
                YisoAnimatorUtils.UpdateAnimatorTrigger(chargeEffectAnimator, daggerTempestEffectAnimationParameter);
            }
        }
    }
}