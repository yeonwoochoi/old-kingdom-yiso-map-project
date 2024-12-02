using System.Collections;
using System.Collections.Generic;
using Character.Core;
using Character.Health.Damage;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;

namespace Character.Ability.Skill {
    [AddComponentMenu("Yiso/Character/Abilities/CharacterSkillDivineStrike")]
    public class YisoCharacterSkillDivineStrike: YisoCharacterSkillAttack {
        public struct DivineStrikeEffectComponent {
            public GameObject skillEffectObj;
            public Animator[] skillEffectAnimators;
            public YisoDamageOnTouch damageOnTouch;
            public Collider2D damageCollider;

            private Transform damageAreaParent;

            public void Initialization(GameObject owner, GameObject effectObject, YisoDamageOnTouch.DamageCalculateDelegate damageCalculateDelegate) {
                if (effectObject == null) return;
                skillEffectObj = effectObject;
                skillEffectAnimators = skillEffectObj.GetComponentsInChildren<Animator>();
                InitializeDamageArea(owner, damageCalculateDelegate);
            }

            private void InitializeDamageArea(GameObject owner, YisoDamageOnTouch.DamageCalculateDelegate damageCalculateDelegate) {
                damageOnTouch = skillEffectObj.GetComponent<YisoDamageOnTouch>();
                if (damageOnTouch == null) return;
                damageCollider = damageOnTouch.gameObject.GetComponent<Collider2D>();
                damageOnTouch.damageCalculate = damageCalculateDelegate;
                damageOnTouch.owner = owner;
                damageOnTouch.StartIgnoreObject(owner);
                DisableDamageArea();
            }

            public void PlayAnimations(int attackEffectAnimationParameter, int xAnimationParameter, int yAnimationParameter, Vector2 direction) {
                if (skillEffectAnimators == null || skillEffectAnimators.Length == 0) return;
                foreach (var skillEffectAnimator in skillEffectAnimators) {
                    YisoAnimatorUtils.UpdateAnimatorTrigger(skillEffectAnimator, attackEffectAnimationParameter);
                    YisoAnimatorUtils.UpdateAnimatorFloat(skillEffectAnimator, xAnimationParameter, direction.x);
                    YisoAnimatorUtils.UpdateAnimatorFloat(skillEffectAnimator, yAnimationParameter, direction.y);
                }
            }

            public void EnableDamageArea() {
                if (damageCollider == null) return;
                damageCollider.enabled = true;
            }

            public void DisableDamageArea() {
                if (damageCollider == null) return;
                damageCollider.enabled = false;
            }
        }
        
        [Title("Skill Effect Prefab")] public GameObject strikeEffectPrefab;

        [Title("Settings")]
        public int spawnCount = 3;
        public float delayBeforeSpawnEffect = 0.5f;
        public float distanceBetweenEffect = 1.5f;
        public float skillEffectDuration = 0.5f;
        public float intervalBetweenEffect = 0.25f;

        protected Vector2 spawnEffectDirection;
        protected GameObject skillEffectParent;
        protected List<DivineStrikeEffectComponent> effectComponents;

        protected const string DivineStrikeAnimationParameterName = "DivineStrike";
        protected const string AttackAnimationParameterName = "Attack";
        protected const string XSpeedAnimationParameterName = "X";
        protected const string YSpeedAnimationParameterName = "Y";
        protected int divineStrikeAnimationParameter;
        protected int attackAnimationParameter;
        protected int xSpeedAnimationParameter;
        protected int ySpeedAnimationParameter;
        
        #region Override Property

        protected override Vector2 InitialAim => Vector2.right;
        protected override bool isDamageAreaAttachedToCharacter => false;
        public override bool CanMoveWhileCasting => false;
        public override bool CanAttackWhileCasting => false;
        public override bool CanDashWhileCasting => false;
        public override bool CanOverlapWithOtherSkills => false;
        public override bool FixOrientation => true;

        #endregion

        protected override void Initialization() {
            base.Initialization();
            
            // Set skill effect parent
            var skillParentObjName = $"[{character.playerID}] Skills";
            var parent = GameObject.Find(skillParentObjName);
            skillEffectParent = parent == null ? new GameObject(skillParentObjName) : parent;

            // Pooling skill effect
            effectComponents = new List<DivineStrikeEffectComponent>();
            for (var i = 0; i < spawnCount; i++) {
                var skillEffectObj = Instantiate(strikeEffectPrefab, Vector3.zero, Quaternion.identity, skillEffectParent.transform);
                skillEffectObj.transform.localPosition = Vector3.zero;
                var effectComponent = new DivineStrikeEffectComponent();
                effectComponent.Initialization(character.gameObject, skillEffectObj, CalculateDamage);
                effectComponents.Add(effectComponent);
            }
        }

        protected override IEnumerator PerformSkillSequence(Transform target) {
            if (target == null) {
                spawnEffectDirection = character.Orientation2D.DirectionFineAdjustmentValue.normalized;
            }
            else {
                spawnEffectDirection = (target.position - character.characterModel.transform.position).normalized;
            }
            
            character.Orientation2D.Face(YisoPhysicsUtils.GetDirectionFromVector(spawnEffectDirection));
            
            UpdateCharacterAnimationParameter();

            yield return new WaitForSeconds(delayBeforeSpawnEffect);

            for (var i = 0; i < spawnCount; i++) {
                var currentEffectComponent = effectComponents[i];
                SetEffectPosition(currentEffectComponent.skillEffectObj, i);
                SetEffectScale(currentEffectComponent.skillEffectObj, i);
                UpdateSkillEffectAnimationParameter(currentEffectComponent);
                StartCoroutine(EnableDamageAreaOverTime(currentEffectComponent, skillEffectDuration));
                yield return new WaitForSeconds(intervalBetweenEffect);
            }

            StopSkillCast(true);
        }

        protected IEnumerator EnableDamageAreaOverTime(DivineStrikeEffectComponent component, float time) {
            component.EnableDamageArea();
            yield return new WaitForSeconds(time);
            component.DisableDamageArea();
        }

        protected virtual void SetEffectPosition(GameObject effectObj, int index) {
            var origin = character.transform.position;
            var distance = distanceBetweenEffect * (index + 1);
            effectObj.transform.position = (Vector2) origin + distance * spawnEffectDirection;
        }

        protected virtual void SetEffectScale(GameObject effectObj, int index) {
            var scaleUnit = 1f / spawnCount;
            var localScale = (index + 1) * scaleUnit;
            effectObj.transform.localScale = Vector3.one * localScale;
        }
        
        #region Animator

        protected override void InitializeAnimatorParameters() {
            base.InitializeAnimatorParameters();
            RegisterAnimatorParameter(AttackAnimationParameterName, AnimatorControllerParameterType.Trigger,
                out attackAnimationParameter);
            RegisterAnimatorParameter(XSpeedAnimationParameterName, AnimatorControllerParameterType.Float,
                out xSpeedAnimationParameter);
            RegisterAnimatorParameter(YSpeedAnimationParameterName, AnimatorControllerParameterType.Float,
                out ySpeedAnimationParameter);
            if (character.characterType == YisoCharacter.CharacterTypes.AI) {
                RegisterAnimatorParameter(DivineStrikeAnimationParameterName, AnimatorControllerParameterType.Bool,
                    out divineStrikeAnimationParameter);
            }
        }

        protected override void UpdateCharacterAnimationParameter() {
            base.UpdateCharacterAnimationParameter();
            if (character.characterType == YisoCharacter.CharacterTypes.AI) {
                YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, divineStrikeAnimationParameter, SkillCasting,
                    character.AnimatorParameters);
            }
        }

        protected virtual void UpdateSkillEffectAnimationParameter(DivineStrikeEffectComponent component) {
            base.UpdateCharacterAnimationParameter();
            component.PlayAnimations(attackAnimationParameter, xSpeedAnimationParameter, ySpeedAnimationParameter, spawnEffectDirection);
        }

        #endregion
    }
}