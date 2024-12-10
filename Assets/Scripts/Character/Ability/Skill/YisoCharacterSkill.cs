using System.Collections;
using Character.Core;
using Character.Weapon;
using Core.Domain.Skill;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Cutscene.Scripts.Control.Cutscene;
using Manager;
using Sirenix.OdinInspector;
using Tools.Cutscene;
using Tools.Event;
using UnityEngine;
using UnityEngine.Events;
using Utils.Beagle;

namespace Character.Ability.Skill {
    public abstract class YisoCharacterSkill : YisoCharacterAbility, IYisoEventListener<YisoCutsceneStateChangeEvent> {
        [Title("Targeting")] public bool requiresTarget;
        [ShowIf("requiresTarget")] public float detectRadius = 8f;
        [ShowIf("requiresTarget")] public bool obstacleDetection = true;

        [ShowIf("requiresTarget")]
        public LayerMask obstacleMask = LayerManager.ObstaclesLayerMask | LayerManager.MapLayerMask;

        [ShowIf("requiresTarget")] public LayerMask targetMask = LayerManager.EnemiesLayerMask;
        [ShowIf("requiresTarget"), ReadOnly] public GameObject target;

        [Title("Invincible")] public bool invincibleWhileCasting = false; // Skill Cast 동안 무적

        [Title("Animation")] 
        public bool useWeaponAnimation = true;
        public int skillAnimatorNumber = 1;

        [Title("Data")]
        public YisoSkillSO skillSO;
        public bool preview = false;

        protected YisoCharacterHandleWeapon characterHandleWeapon;
        protected YisoCharacterHandleSkill characterHandleSkill;
        protected YisoCharacterDash characterDash;
        protected Coroutine skillSequenceCoroutine;
        protected int maxDetectTargetCount = 10;
        
        protected event UnityAction OnSkillStartEvent; 
        protected event UnityAction OnSkillEndEvent;

        public int SkillId => skillSO == null ? -1 : skillSO.id;
        public bool SkillCasting { get; protected set; } = false;
        protected GameObject WeaponObj => characterHandleWeapon?.CurrentWeaponModel;
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoCharacterSkill>();

        #region Abstract Property

        public abstract bool CanMoveWhileCasting { get; }
        public abstract bool CanAttackWhileCasting { get; }
        public abstract bool CanDashWhileCasting { get; }
        public abstract bool FixOrientation { get; }
        public abstract bool CanOverlapWithOtherSkills { get; } // 다른 스킬과 동시에 실행할 수 있는지 여부

        #endregion

        protected override void Initialization() {
            base.Initialization();

            SkillCasting = false;
            characterHandleWeapon = character.FindAbility<YisoCharacterHandleWeapon>();
            characterHandleSkill = character.FindAbility<YisoCharacterHandleSkill>();
            characterDash = character.FindAbility<YisoCharacterDash>();
            if (characterHandleWeapon == null) {
                LogService.Warn($"[YisoCharacterSkill] : Could not find character handle weapon.");
                abilityInitialized = false;
            }

            conditionState.OnStateChange += OnConditionStateChanged;
        }

        #region Public API

        public virtual void StartSkillCast(ref int skillId, UnityAction startCallback = null, UnityAction endCallback = null) {
            if (!CanCastSkill()) return;
            skillId = SkillId;
            OnSkillStartEvent = startCallback;
            OnSkillEndEvent = endCallback;
            StartSkillCast();
        }

        /// <summary>
        /// Start Skill Cast
        /// </summary>
        /// <param name="targetObj"></param>
        public virtual void StartSkillCast(Transform targetObj = null) {
            if (!CanCastSkill()) return;

            SkillCasting = true;
            movementState.ChangeState(YisoCharacterStates.MovementStates.SkillCasting);

            if (invincibleWhileCasting) health.DamageDisabled();
            if (!CanMoveWhileCasting) {
                characterMovement.MovementForbidden = true;
                controller.AllowImpact = false;
                controller.freeMovement = false;
            }

            if (!CanAttackWhileCasting) characterHandleWeapon.AttackForbidden = true;
            if (!CanDashWhileCasting && characterDash != null) characterDash.DashForbidden = true;
            if (FixOrientation) character.Orientation2D.OrientationForbidden = true;
            if (!useWeaponAnimation && WeaponObj != null) WeaponObj.SetActive(false);

            var finalTargetObj = targetObj;
            if (targetObj == null && requiresTarget) {
                finalTargetObj = FindNearestTarget();
            }

            target = finalTargetObj == null ? null : finalTargetObj.gameObject;
            PlayAbilityStartFeedbacks();
            OnSkillStartEvent?.Invoke();
            skillSequenceCoroutine = StartCoroutine(PerformSkillSequence(finalTargetObj));
        }

        /// <summary>
        /// Stop Skill Cast
        /// </summary>
        /// <param name="isAutoSkillSequenceTermination">skill sequence가 끝난 후 코루틴 내에서 호출되는지 여부</param>
        public virtual void StopSkillCast(bool isAutoSkillSequenceTermination = false) {
            PlayAbilityStopFeedbacks();
            OnSkillEndEvent?.Invoke();

            if (invincibleWhileCasting) health.DamageEnabled();
            if (!CanMoveWhileCasting) {
                characterMovement.MovementForbidden = false;
                controller.AllowImpact = true;
                controller.freeMovement = true;
            }

            if (!CanAttackWhileCasting) characterHandleWeapon.AttackForbidden = false;
            if (!CanDashWhileCasting && characterDash != null) characterDash.DashForbidden = false;
            if (FixOrientation) character.Orientation2D.OrientationForbidden = false;
            if (!useWeaponAnimation && WeaponObj != null) WeaponObj.SetActive(true);

            if (!isAutoSkillSequenceTermination && skillSequenceCoroutine != null)
                StopCoroutine(skillSequenceCoroutine);

            SkillCasting = false;
            UpdateCharacterAnimationParameter();
            movementState.ChangeState(YisoCharacterStates.MovementStates.Idle);
        }

        #endregion

        #region Core

        protected abstract IEnumerator PerformSkillSequence(Transform target);

        #endregion

        #region Target

        protected virtual Transform FindNearestTarget() {
            return YisoPhysicsUtils.FindNearestCharacterTarget(character.transform, detectRadius,
                controller2D.ColliderBounds, targetMask, obstacleMask, obstacleDetection, maxDetectTargetCount);
        }

        #endregion

        #region Check

        public virtual bool CanCastSkill() {
            if (!abilityInitialized) return false;
            if (!AbilityAuthorized) return false;
            if (character.characterType == YisoCharacter.CharacterTypes.Player) {
                if (!preview && skillSO.attackType != YisoWeapon.AttackType.None) {
                    if (characterHandleWeapon.currentWeapon == null) return false;
                    if (characterHandleWeapon.currentWeapon.GetAttackType() != skillSO.attackType) return false;
                }
            }

            if (character.conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Normal) return false;
            if (!CanOverlapWithOtherSkills && characterHandleSkill.IsSkillCasting) return false;
            return !SkillCasting;
        }

        #endregion

        #region Animator

        protected virtual void UpdateCharacterAnimationParameter() {
        }

        #endregion

        protected virtual void OnConditionStateChanged() {
            if (conditionState.CurrentState == YisoCharacterStates.CharacterConditions.Normal) return;
            StopSkillCast();
        }

        public void OnEvent(YisoCutsceneStateChangeEvent e) {
            if (e.cutsceneState == YisoCutsceneTrigger.CutsceneState.Play) StopSkillCast();
        }

        protected override void OnEnable() {
            base.OnEnable();
            this.YisoEventStartListening();
        }

        protected override void OnDisable() {
            base.OnDisable();
            conditionState.OnStateChange -= OnConditionStateChanged;
            this.YisoEventStopListening();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            StopAllCoroutines();
        }
    }
}