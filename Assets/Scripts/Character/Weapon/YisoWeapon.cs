using System.Collections;
using System.Collections.Generic;
using Character.Ability;
using Character.Core;
using Character.Weapon.Aim;
using Core.Behaviour;
using Sirenix.OdinInspector;
using Tools.Feedback;
using Tools.Feedback.Core;
using Tools.StateMachine;
using UnityEngine;
using Utils.Beagle;

namespace Character.Weapon {
    public abstract class YisoWeapon : RunIBehaviour {
        public enum AttackType {
            None = 0,
            Shoot = 1,
            Thrust = 2,
            Slash = 3
        }

        public enum WeaponStates {
            Idle,
            Start,
            DelayBeforeUse,
            Use,
            DelayBetweenUses,
            Stop,
            Interrupt
        }

        /// <summary>
        /// SemiAuto: Input이 있을때만 Weapon Start
        /// Auto: Input이 Released되면 자동으로 다시 Weapon Start (Request) => ex. 몇 초 간격으로 계속 쏘는 대포
        /// </summary>
        public enum TriggerModes {
            SemiAuto,
            Auto
        }

        [Title("ID")] public string weaponName;

        [Title("Settings")] [ReadOnly] public bool weaponActive = false; // Active 여부
        public bool interruptable = false; // 맞았을때 공격 Interrupt 할거냐?

        [Title("Use")] public TriggerModes triggerMode = TriggerModes.Auto;
        public float attackSpeed = 1.5f;
        public float delayBeforeUse = 0.2f;
        public float delayUse = 1f; // 이 시간 동안 Weapon Use 상태로 있는 거 => delayUse 후 DelayBetweenUses로 넘어감
        public float delayBetweenUses = 1f;

        public bool
            canDelayBeforeUseInterrupted =
                false; // whether or not the delay before used can be interrupted by releasing the shoot button (if true, releasing the button will cancel the delayed shot)

        public bool canDelayBetweenUsesInterrupted = false;

        /// whether or not the time between uses can be interrupted by releasing the shoot button (if true, releasing the button will cancel the time between uses)
        [Title("Recoil")] public float recoilForce = 0f;

        [Title("Restriction")] public bool preventAllMovementWhileInUse = true; // 때릴때는 멈추게 할거냐
        public bool preventAllAimWhileInUse = false; // 때릴때는 Aim 멈출거냐 => Attack 상태일때 Aim 먹통됨. AI일때는 꺼줘야함

        [Title("Feedback")] public YisoFeedBacks weaponStartFeedbacks;
        public YisoFeedBacks weaponUsedFeedbacks;
        public YisoFeedBacks weaponStopFeedbacks;

        public YisoStateMachine<WeaponStates> weaponState;

        public YisoCharacter Owner => owner;
        public YisoCharacterHandleWeapon CharacterHandleWeapon => characterHandleWeapon;
        public YisoWeaponAim WeaponAim => weaponAim;
        public bool IsWeaponHidden  => isWeaponHidden;

        public float DelayBeforeUse => delayBeforeUse / attackSpeed;
        public float DelayUse => delayUse / attackSpeed;
        public float DelayBetweenUses => delayBetweenUses / attackSpeed;
        public Animator WeaponAnimator { get; protected set; }

        protected bool initialized = false;
        protected bool inputReleased = false; // false = 더이상 Input이 안 들어온다.
        protected bool isWeaponHidden = false;

        protected YisoWeaponAim weaponAim;
        protected YisoComboWeapon comboWeapon;
        protected TopDownController controller;
        protected YisoCharacterMovement characterMovement;
        protected YisoCharacter owner;
        protected YisoCharacterHandleWeapon characterHandleWeapon;
        protected YisoCharacterHandleSkill characterHandleSkill;
        protected YisoCharacterStat characterStat;
        protected HashSet<int> animatorParameters;

        protected float delayBeforeUseCounter;
        protected float delayBetweenUsesCounter;
        protected float delayUseCounter;
        protected float lastAttackRequestAt = -float.MaxValue;
        protected float lastWeaponTurnOnAt = -float.MaxValue;

        protected const string XSpeedAnimationParameterName = "X";
        protected const string YSpeedAnimationParameterName = "Y";
        protected const string WalkingAnimationParameterName = "IsMoving";
        protected const string AttackAnimationParameterName = "IsAttack";
        protected const string HideAnimationParameterName = "IsHide";
        protected const string MoveSpeedAnimationParameterName = "MoveSpeed";
        protected const string AttackSpeedAnimationParameterName = "AttackSpeed";
        protected const string ComboIndexAnimationParameterName = "Combo";
        protected const string SkillCastAnimationParameterName = "IsSkillCast";
        protected const string SkillNumberAnimationParameterName = "SkillNumber";

        protected int xSpeedAnimationParameter;
        protected int ySpeedAnimationParameter;
        protected int walkingAnimationParameter;
        protected int attackAnimationParameter;
        protected int hideAnimationParameter;
        protected int moveSpeedAnimationParameter;
        protected int attackSpeedAnimationParameter;
        protected int comboIndexAnimationParameter;
        protected int skillCastAnimationParameter;
        protected int skillNumberAnimationParameter;

        #region Initialization

        public virtual void SetOwner(YisoCharacter newOwner, YisoCharacterHandleWeapon handleWeapon) {
            owner = newOwner;
            if (owner != null) {
                characterHandleWeapon = handleWeapon;
                characterHandleSkill = owner.FindAbility<YisoCharacterHandleSkill>();
                characterMovement = owner.FindAbility<YisoCharacterMovement>();
                controller = owner.GetComponent<TopDownController2D>();
                characterStat = owner.FindAbility<YisoCharacterStat>();
            }
        }

        public virtual void Initialization() {
            weaponAim = GetComponent<YisoWeaponAim>();
            comboWeapon = gameObject.GetComponent<YisoComboWeapon>();
            weaponState = new YisoStateMachine<WeaponStates>(gameObject, true);
            weaponState.ChangeState(WeaponStates.Idle);
            InitializeAnimator();
            InitializeFeedbacks();
        }

        public virtual void InitializeComboWeapons() {
            if (comboWeapon != null) {
                comboWeapon.Initialization();
            }
            else {
                weaponActive = true;
            }
        }

        protected virtual void InitializeFeedbacks() {
            weaponStartFeedbacks?.Initialization(gameObject);
            weaponUsedFeedbacks?.Initialization(gameObject);
            weaponStopFeedbacks?.Initialization(gameObject);
        }

        protected virtual void InitializeAnimator() {
            WeaponAnimator = gameObject.GetComponentInChildren<Animator>();
            if (WeaponAnimator == null) return;
            characterHandleWeapon.CurrentWeaponModel = WeaponAnimator.gameObject;
            if (comboWeapon == null) AttachWeaponToModelParent();
            animatorParameters = new HashSet<int>();
            InitializeAnimatorParameters();
        }

        public virtual void AttachWeaponToModelParent() {
            WeaponAnimator.transform.SetParent(characterHandleWeapon.weaponAttachment);
            WeaponAnimator.transform.localPosition = Vector3.zero;
            WeaponAnimator.transform.localRotation = Quaternion.identity;
        }

        protected override void OnDestroy() {
            if (WeaponAnimator != null) {
                Destroy(WeaponAnimator.gameObject);
            }
        }

        #endregion

        #region State

        public override void OnUpdate() {
            ProcessWeaponState();
        }

        protected virtual void ProcessWeaponState() {
            if (weaponState == null) return;

            UpdateAnimator();

            switch (weaponState.CurrentState) {
                case WeaponStates.Idle:
                    CaseWeaponIdle();
                    break;
                case WeaponStates.Start:
                    CaseWeaponStart();
                    break;
                case WeaponStates.DelayBeforeUse:
                    CaseWeaponDelayBeforeUse();
                    break;
                case WeaponStates.Use:
                    CaseWeaponUsed();
                    break;
                case WeaponStates.DelayBetweenUses:
                    CaseWeaponDelayBetweenUses();
                    break;
                case WeaponStates.Stop:
                    CaseWeaponStop();
                    break;
                case WeaponStates.Interrupt:
                    CaseWeaponInterrupted();
                    break;
            }
        }

        protected virtual void CaseWeaponIdle() {
        }

        protected virtual void CaseWeaponStart() {
            if (DelayBeforeUse > 0) {
                delayBeforeUseCounter = DelayBeforeUse;
                weaponState.ChangeState(WeaponStates.DelayBeforeUse);
            }
            else {
                AttackRequest();
            }
        }

        protected virtual void CaseWeaponDelayBeforeUse() {
            delayBeforeUseCounter -= Time.deltaTime;
            if (delayBeforeUseCounter <= 0f) {
                AttackRequest();
            }
        }

        protected virtual void CaseWeaponUsed() {
            // 바로 다음 State로 넘기지 말고 일정 시간동안 Use State를 유지하게끔 하기 (Animation 과 동기화를 위해)
            delayUseCounter -= Time.deltaTime;
            StartCoroutine(AttackUseCo());
        }

        protected virtual void CaseWeaponDelayBetweenUses() {
            if (inputReleased && canDelayBetweenUsesInterrupted) {
                AttackStop();
                return;
            }

            delayBetweenUsesCounter -= Time.deltaTime;
            if (delayBetweenUsesCounter <= 0) {
                if (triggerMode == TriggerModes.Auto && !inputReleased) {
                    AttackRequest();
                }
                else {
                    AttackStop();
                }
            }
        }

        protected virtual void CaseWeaponStop() {
            weaponState.ChangeState(WeaponStates.Idle);
        }

        protected virtual void CaseWeaponInterrupted() {
            AttackStop();
            weaponState.ChangeState(WeaponStates.Idle);
        }

        #endregion

        #region Core

        public virtual void AttackStart() {
            if (weaponState.CurrentState != WeaponStates.Idle) return;
            inputReleased = false;

            // 쿨타임 체크
            if (Time.time - lastWeaponTurnOnAt < DelayBetweenUses) return;
            lastWeaponTurnOnAt = Time.time;

            // Feedback Trigger
            TriggerWeaponStartFeedback();

            // Change Movement State
            owner.movementState.ChangeState(YisoCharacterStates.MovementStates.Attacking);

            // Weapon State Cycle Start
            weaponState.ChangeState(WeaponStates.Start);

            // Combo Start
            if (comboWeapon != null) comboWeapon.WeaponStarted();

            // Movement Stop
            if (preventAllMovementWhileInUse && characterMovement != null && controller != null) {
                characterMovement.SetMovement(Vector2.zero);
                characterMovement.MovementForbidden = true;
            }

            // Aim Stop
            if (preventAllAimWhileInUse && weaponAim != null) {
                weaponAim.SyncAim();
                weaponAim.aimControlActive = false;
            }
        }

        public virtual void AttackStop() {
            if (weaponState.CurrentState == WeaponStates.Idle || weaponState.CurrentState == WeaponStates.Stop) {
                return;
            }

            inputReleased = true;

            // Stop Feedback Trigger
            TriggerWeaponStopFeedback();

            // Change Movement State
            owner.movementState.ChangeState(YisoCharacterStates.MovementStates.Idle);

            // Weapon State Cycle Stop
            weaponState.ChangeState(WeaponStates.Stop);

            // Combo Stop
            if (comboWeapon != null) comboWeapon.WeaponStopped();

            // Movement Allow
            if (preventAllMovementWhileInUse && (characterMovement != null)) {
                characterMovement.MovementForbidden = false;
            }

            // Aim Start
            if (preventAllAimWhileInUse && weaponAim != null) {
                weaponAim.aimControlActive = true;
            }
        }

        protected virtual void AttackRequest() {
            if (Time.time - lastAttackRequestAt < DelayBetweenUses) return;

            delayUseCounter = DelayUse;
            weaponState.ChangeState(WeaponStates.Use);
            lastAttackRequestAt = Time.time;
        }

        protected bool attackUseCoInProgress = false; // Use 상태인지

        protected virtual IEnumerator AttackUseCo() {
            if (attackUseCoInProgress) yield break;

            attackUseCoInProgress = true;

            WeaponUse();

            while (delayUseCounter > 0f) {
                if (weaponState.CurrentState != WeaponStates.Use) {
                    attackUseCoInProgress = false;
                    yield break;
                }

                yield return null;
            }

            if (weaponState.CurrentState == WeaponStates.Use) {
                delayBetweenUsesCounter = DelayBetweenUses;
                weaponState.ChangeState(WeaponStates.DelayBetweenUses);
            }

            attackUseCoInProgress = false;
        }

        /// <summary>
        /// 이걸 Override 해서 각 Weapon마다 맞는 로직 추가해서 실행
        /// </summary>
        protected virtual void WeaponUse() {
            ApplyRecoil();
            TriggerWeaponUsedFeedback();
            StartCoroutine(WeaponUseCo());
        }

        protected bool weaponUseCoInProgress = false;

        /// <summary>
        /// 추가적인 Weapon Use 로직이 있다면 이걸 Override 해서 쓰기
        /// Weapon State 랑 독립적으로 진행됨. (Weapon State가 갑자기 Stop으로 바뀌어도 이 코루틴은 실행됨)
        /// ex. Damage On Touch를 Enable / Disable 시킨다던지
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator WeaponUseCo() {
            yield return null;
        }

        #endregion

        #region Interrupt

        public virtual void Interrupt() {
            if (interruptable) {
                weaponState.ChangeState(WeaponStates.Interrupt);
            }
        }

        #endregion

        #region Recoil (반동)

        protected virtual void ApplyRecoil() {
            if (recoilForce == 0f) return;
            if (owner == null) return;

            // TODO
            // controller.Impact(-owner.LinkedInputManager.LastMovement, recoilForce);
            controller.Impact(-transform.right, recoilForce);
        }

        #endregion

        #region Feedbacks

        protected virtual void TriggerWeaponStartFeedback() {
            weaponStartFeedbacks?.PlayFeedbacks(transform.position);
        }

        protected virtual void TriggerWeaponUsedFeedback() {
            weaponUsedFeedbacks?.PlayFeedbacks(transform.position);
        }

        protected virtual void TriggerWeaponStopFeedback() {
            weaponStopFeedbacks?.PlayFeedbacks(transform.position);
        }

        #endregion

        #region WeaponType

        public virtual AttackType GetAttackType() {
            return AttackType.None;
        }

        #endregion

        #region Visible

        public void ShowWeapon() {
            isWeaponHidden = false;
            UpdateAnimator();
        }

        public void HideWeapon() {
            isWeaponHidden = true;
            UpdateAnimator();
        }

        #endregion

        #region Animator

        protected virtual void InitializeAnimatorParameters() {
            if (WeaponAnimator == null) return;
            YisoAnimatorUtils.AddAnimatorParameterIfExists(WeaponAnimator, XSpeedAnimationParameterName, out xSpeedAnimationParameter, AnimatorControllerParameterType.Float, animatorParameters);
            YisoAnimatorUtils.AddAnimatorParameterIfExists(WeaponAnimator, YSpeedAnimationParameterName, out ySpeedAnimationParameter, AnimatorControllerParameterType.Float, animatorParameters);
            YisoAnimatorUtils.AddAnimatorParameterIfExists(WeaponAnimator, HideAnimationParameterName, out hideAnimationParameter, AnimatorControllerParameterType.Bool, animatorParameters);
            YisoAnimatorUtils.AddAnimatorParameterIfExists(WeaponAnimator, WalkingAnimationParameterName, out walkingAnimationParameter, AnimatorControllerParameterType.Bool, animatorParameters);
            YisoAnimatorUtils.AddAnimatorParameterIfExists(WeaponAnimator, AttackAnimationParameterName, out attackAnimationParameter, AnimatorControllerParameterType.Bool, animatorParameters);
            YisoAnimatorUtils.AddAnimatorParameterIfExists(WeaponAnimator, MoveSpeedAnimationParameterName, out moveSpeedAnimationParameter, AnimatorControllerParameterType.Float, animatorParameters);
            YisoAnimatorUtils.AddAnimatorParameterIfExists(WeaponAnimator, AttackSpeedAnimationParameterName, out attackSpeedAnimationParameter, AnimatorControllerParameterType.Float, animatorParameters);
            YisoAnimatorUtils.AddAnimatorParameterIfExists(WeaponAnimator, SkillCastAnimationParameterName, out skillCastAnimationParameter, AnimatorControllerParameterType.Bool, animatorParameters);
            YisoAnimatorUtils.AddAnimatorParameterIfExists(WeaponAnimator, SkillNumberAnimationParameterName, out skillNumberAnimationParameter, AnimatorControllerParameterType.Int, animatorParameters);
            if (comboWeapon != null) {
                YisoAnimatorUtils.AddAnimatorParameterIfExists(WeaponAnimator, ComboIndexAnimationParameterName, out comboIndexAnimationParameter, AnimatorControllerParameterType.Int, animatorParameters);
            }
        }

        protected virtual void UpdateAnimator() {
            if (WeaponAnimator == null) return;
            YisoAnimatorUtils.UpdateAnimatorFloat(WeaponAnimator, xSpeedAnimationParameter, owner.Orientation2D.DirectionFineAdjustmentValue.x, animatorParameters);
            YisoAnimatorUtils.UpdateAnimatorFloat(WeaponAnimator, ySpeedAnimationParameter, owner.Orientation2D.DirectionFineAdjustmentValue.y, animatorParameters);
            YisoAnimatorUtils.UpdateAnimatorBool(WeaponAnimator, hideAnimationParameter, isWeaponHidden, animatorParameters);
            YisoAnimatorUtils.UpdateAnimatorBool(WeaponAnimator, walkingAnimationParameter, owner.movementState.CurrentState == YisoCharacterStates.MovementStates.Walking, animatorParameters);
            YisoAnimatorUtils.UpdateAnimatorBool(WeaponAnimator, attackAnimationParameter, characterHandleWeapon.currentWeapon.weaponState.CurrentState == WeaponStates.Use, animatorParameters);
            YisoAnimatorUtils.UpdateAnimatorFloat(WeaponAnimator, moveSpeedAnimationParameter, characterMovement.MovementSpeedMultiplier, animatorParameters);
            YisoAnimatorUtils.UpdateAnimatorBool(WeaponAnimator, skillCastAnimationParameter, characterHandleSkill.IsSkillCasting, animatorParameters);
            YisoAnimatorUtils.UpdateAnimatorInteger(WeaponAnimator, skillNumberAnimationParameter, characterHandleSkill.CurrentSkillAnimatorId, animatorParameters);
            YisoAnimatorUtils.UpdateAnimatorFloat(WeaponAnimator, attackSpeedAnimationParameter, characterHandleWeapon.currentWeapon.attackSpeed, animatorParameters);
            if (comboWeapon != null) {
                YisoAnimatorUtils.UpdateAnimatorInteger(WeaponAnimator, comboIndexAnimationParameter, comboWeapon.currentComboIndex, animatorParameters);
            }
        }

        #endregion
    }
}