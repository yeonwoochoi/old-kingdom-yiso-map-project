using System.Collections.Generic;
using System.Linq;
using Character.Core;
using Character.Health;
using Character.Weapon;
using Core.Behaviour;
using Manager_Temp_;
using Sirenix.OdinInspector;
using Tools.Feedback.Core;
using Tools.StateMachine;
using UnityEngine;
using Utils.Beagle;

namespace Character.Ability {
    public class YisoCharacterAbility : RunIBehaviour {
        [Title("Sfx")] public AudioClip abilityStartSfx;
        public AudioClip abilityInProgressSfx; // null이 아니면 play된 상태 vs null이면 stop인 상태
        public AudioClip abilityStopSfx;

        [Title("Feedback")] public YisoFeedBacks abilityStartFeedbacks;
        public YisoFeedBacks abilityStopFeedbacks;

        [Title("Settings")] public bool abilityPermitted = true; // false면 작동 안함

        [Title("Blocking States")]
        public YisoCharacterStates.MovementStates[]
            blockingMovementStates; // blocking movement states에선 이 Ability 실행 안됨

        public YisoCharacterStates.CharacterConditions[]
            blockingConditionStates; // blocking condition states에선 이 Ability 실행 안됨

        public YisoWeapon.WeaponStates[] blockingWeaponStates;

        protected YisoCharacter character;
        protected TopDownController controller;
        protected TopDownController2D controller2D;
        protected GameObject model;
        protected YisoHealth health;
        protected YisoCharacterMovement characterMovement;
        protected InputManager inputManager;
        protected Animator animator = null;
        protected SpriteRenderer spriteRenderer;
        protected YisoStateMachine<YisoCharacterStates.MovementStates> movementState;
        protected YisoStateMachine<YisoCharacterStates.CharacterConditions> conditionState;
        protected List<YisoCharacterHandleWeapon> handleWeaponList;
        protected AudioSource currentAbilityInProgressSfx;
        protected float verticalInput;
        protected float horizontalInput;
        protected bool abilityInitialized = false;
        protected bool startFeedbackIsPlaying = false;

        public bool AbilityInitialized => abilityInitialized;

        // 해당 Ability 사용 가능 (Available)
        public bool AbilityAuthorized {
            get {
                if (character != null) {
                    if (blockingMovementStates != null && blockingMovementStates.Length > 0) {
                        if (blockingMovementStates.Any(m => m == character.movementState.CurrentState)) {
                            return false;
                        }
                    }

                    if (blockingConditionStates != null && blockingConditionStates.Length > 0) {
                        if (blockingConditionStates.Any(c => c == character.conditionState.CurrentState)) {
                            return false;
                        }
                    }

                    if (blockingWeaponStates != null && blockingWeaponStates.Length > 0) {
                        if (blockingWeaponStates.Any(t =>
                            handleWeaponList.Where(handleWeapon => handleWeapon.currentWeapon != null)
                                .Any(handleWeapon => handleWeapon.currentWeapon.weaponState.CurrentState == t))) {
                            return false;
                        }
                    }
                }

                return abilityPermitted;
            }
        }

        protected override void Awake() {
            PreInitialization();
        }

        protected override void Start() {
            Initialization();
        }

        protected virtual void PreInitialization() {
            character = gameObject.GetComponentInParent<YisoCharacter>();
            BindAnimator();
        }

        protected virtual void Initialization() {
            character = gameObject.GetComponentInParent<YisoCharacter>();
            BindAnimator();
            controller = gameObject.GetComponentInParent<TopDownController>();
            controller2D = gameObject.GetComponentInParent<TopDownController2D>();
            model = character.characterModel;
            characterMovement = character?.FindAbility<YisoCharacterMovement>();
            spriteRenderer = gameObject.GetComponentInParent<SpriteRenderer>();
            handleWeaponList = character?.FindAbilities<YisoCharacterHandleWeapon>();
            health = character.characterHealth;
            inputManager = character.LinkedInputManager;
            movementState = character.movementState;
            conditionState = character.conditionState;
            abilityInitialized = true;
        }

        #region Animator

        /// <summary>
        /// Binds the animator from the character and initializes the animator parameters
        /// </summary>
        protected virtual void BindAnimator() {
            if (character.Animator == null) {
                character.AssignAnimator();
            }

            animator = character.Animator;

            if (animator != null) {
                InitializeAnimatorParameters();
            }
        }

        /// <summary>
        /// 각 Ability 마다 필요한 Animator Parameter 여기서 설정해주면 됨. Override 해서
        /// </summary>
        protected virtual void InitializeAnimatorParameters() {
        }

        /// <summary>
        /// Yiso Character의 UpdateAnimators() 에서 호출함
        /// </summary>
        public virtual void UpdateAnimator() {
        }

        /// <summary>
        /// Yiso Character에 있는 AnimatorParameters List<HashSet>에 Animator parameter 추가 
        /// </summary>
        public virtual void RegisterAnimatorParameter(string parameterName,
            AnimatorControllerParameterType parameterType, out int parameter) {
            parameter = Animator.StringToHash(parameterName);

            if (animator == null) return;
            if (animator.HasParameterOfType(parameterName, parameterType)) {
                if (character != null) {
                    character.AnimatorParameters.Add(parameter);
                }
            }
        }

        #endregion

        #region Input

        public virtual void SetInputManager(InputManager newInputManager) {
            inputManager = newInputManager;
        }

        protected virtual void InternalHandleInput() {
            if (inputManager == null) return;
            horizontalInput = inputManager.Movement.x;
            verticalInput = inputManager.Movement.y;
            HandleInput();
        }

        protected virtual void HandleInput() {
        }

        public virtual void ResetInput() {
            verticalInput = 0f;
            horizontalInput = 0f;
            if (inputManager != null) inputManager.ResetAllMovement();
        }

        #endregion

        #region Ability

        public virtual void PreProcessAbility() {
            InternalHandleInput();
        }

        public virtual void ProcessAbility() {
        }

        public virtual void PostProcessAbility() {
        }

        public virtual void PermitAbility(bool permitted) {
            abilityPermitted = permitted;
        }

        public virtual void ResetAbility() {
        }

        #endregion

        #region Sfx

        /// <summary>
        /// Play ability start sound (말 그대로 시작할때만)
        /// </summary>
        public virtual void PlayAbilityStartSfx() {
            // TODO
        }

        /// <summary>
        /// Play ability using sound (말 그대로 사용되고 있는 동안 내내~)
        /// </summary>
        public virtual void PlayAbilityUsedSfx() {
            // TODO
        }

        /// <summary>
        /// Stop ability using sound (말 그대로 사용되고 있는 동안 내내 재생되는 Sound를 Stop)
        /// </summary>
        public virtual void StopAbilityUsedSfx() {
            // TODO
        }

        /// <summary>
        /// Play ability stop sound (말 그대로 사용 멈췄을 때)
        /// </summary>
        public virtual void PlayAbilityStopSfx() {
            // TODO
        }

        #endregion

        #region Feedback

        /// <summary>
        /// Play ability Start Feedbacks
        /// </summary>
        public virtual void PlayAbilityStartFeedbacks() {
            abilityStartFeedbacks?.PlayFeedbacks(transform.position);
            startFeedbackIsPlaying = true;
        }

        /// <summary>
        /// Stop ability Start Feedbacks
        /// </summary>
        public virtual void StopStartFeedbacks() {
            abilityStartFeedbacks?.StopFeedbacks();
            startFeedbackIsPlaying = false;
        }

        /// <summary>
        /// Play ability Stop feedbacks
        /// </summary>
        public virtual void PlayAbilityStopFeedbacks() {
            abilityStopFeedbacks?.PlayFeedbacks();
        }

        #endregion

        #region Health

        protected virtual void OnRespawn() {
        }

        protected virtual void OnDeath() {
            StopAbilityUsedSfx();
            StopStartFeedbacks();
        }

        protected virtual void OnHit(GameObject attacker) {
        }

        protected override void OnEnable() {
            base.OnEnable();
            if (health == null) health = gameObject.GetComponentInParent<YisoCharacter>().characterHealth;
            if (health == null) health = gameObject.GetComponentInParent<YisoHealth>();
            if (health != null) {
                health.onRevive += OnRespawn;
                health.onDeath += OnDeath;
                health.onHit += OnHit;
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (health != null) {
                health.onRevive -= OnRespawn;
                health.onDeath -= OnDeath;
                health.onHit -= OnHit;
            }
        }

        #endregion
    }
}