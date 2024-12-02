using System.Collections;
using Character.Core;
using Core.Service;
using Core.Service.Character;
using UnityEngine;
using Utils.Beagle;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Character.Ability {
    [AddComponentMenu("Yiso/Character/Abilities/Character Movement")]
    public class YisoCharacterMovement : YisoCharacterAbility {
        public bool inputAuthorized = true; // false면 input 제한됨

        public bool analogInput = false; // true: analog (joystick 같이 -1~1 사이 값이 있는 device) / false: digital (WASD 입력 -1, 1) 
        
        public float baseMovementSpeed = 3f;
        public float idleThreshold = 0.05f;
        public float acceleration = 10f;
        public float deceleration = 10f;
        public bool interpolateMovementSpeed = false; // 말 그대로 속도를 보간할건지 말지 (보간하면 input을 안줘도 천천히 부드럽게 멈춤)
        public ParticleSystem[] walkParticles;

        public bool MovementForbidden { get; set; } // True 이면 못 움직임

        #region Speed

        private float movementSpeed = 1f; // Player의 경우 Core에서 받아오는 값 / Enemy는 1f
        private float movementMaxSpeed = 8f;
        
        public float BaseMovementSpeed { get; set; }
        
        public float MovementSpeed {
            get => Mathf.Min(movementSpeed * BaseMovementSpeed, movementMaxSpeed);
            private set => movementSpeed = value;
        }

        private float movementSpeedMultiplier = 1f;
        private float movementMaxSpeedMultiplier = 3f;

        public float MovementSpeedMultiplier {
            get => Mathf.Min(movementSpeedMultiplier, movementMaxSpeedMultiplier);
            private set => movementSpeedMultiplier = value;
        }

        #endregion

        protected float horizontalMovement; // Player : Input Manager
        protected float verticalMovement; // AI : Public API (SetMovement, SetHorizontalMovement, SetVerticalMovement)
        protected bool walkParticlesPlaying = false;
        protected Coroutine applyMovementMultiplierCoroutine;

        protected const string MoveSpeedAnimationParameterName = "MoveSpeed";
        protected const string IdleAnimationParameterName = "IsIdle";
        protected const string WalkingAnimationParameterName = "IsMoving";

        protected int moveSpeedAnimationParameter;
        protected int idleAnimationParameter;
        protected int walkingAnimationParameter;

        #region Main

        protected override void Initialization() {
            base.Initialization();
            ResetAbility();
        }

        public override void ResetAbility() {
            base.ResetAbility();
            ResetSpeed(1f, false);
            movementState?.ChangeState(YisoCharacterStates.MovementStates.Idle);
            MovementForbidden = false;

            foreach (var system in walkParticles) {
                if (system != null) system.Stop();
            }
        }

        // PreProcessAbility()에서 호출됨 (Update)
        protected override void HandleInput() {
            if (inputAuthorized) {
                horizontalMovement = horizontalInput;
                verticalMovement = verticalInput;
            }
            else {
                horizontalMovement = 0f;
                verticalMovement = 0f;
            }
        }

        /// <summary>
        /// ProcessAbility()에서 호출됨 (Update)
        /// </summary>
        public override void ProcessAbility() {
            base.ProcessAbility();
            HandleFrozen();
            HandleMovement();
            HandleFeedbacks();
        }

        protected virtual void HandleFrozen() {
            if (!AbilityAuthorized) return;
            if (conditionState.CurrentState == YisoCharacterStates.CharacterConditions.Frozen) {
                horizontalMovement = 0f;
                verticalMovement = 0f;
                SetMovement();
            }
        }

        /// <summary>
        /// 각 Case 별로 상태 처리
        /// </summary>
        protected virtual void HandleMovement() {
            // 안 걷고 있는 경우 Start Feedback 꺼
            if (movementState.CurrentState != YisoCharacterStates.MovementStates.Walking && startFeedbackIsPlaying) {
                StopStartFeedbacks();
            }

            // 안 걷고 있는 경우 Walking sound 켜져있는 경우 꺼
            if (movementState.CurrentState != YisoCharacterStates.MovementStates.Walking &&
                abilityInProgressSfx != null) {
                StopAbilityUsedSfx();
            }

            // 걷는 중인데 Walking Sound 꺼져있는 경우 켜
            if (movementState.CurrentState == YisoCharacterStates.MovementStates.Walking &&
                abilityInProgressSfx == null) {
                PlayAbilityStartSfx();
            }

            // movement가 막혀있거나, Frozen, Dead 등의 움직일 수 없는 상태일때
            if (!AbilityAuthorized || conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Normal) {
                return;
            }

            if (MovementForbidden) {
                horizontalMovement = 0f;
                verticalMovement = 0f;
            }

            // Idle 상태인데 Input값이 Idle경계값 넘어갈때 Walking State로 변경
            if (controller.currentMovement.magnitude > idleThreshold &&
                movementState.CurrentState == YisoCharacterStates.MovementStates.Idle) {
                movementState.ChangeState(YisoCharacterStates.MovementStates.Walking);
                PlayAbilityStartSfx();
                PlayAbilityUsedSfx();
                PlayAbilityStartFeedbacks();
            }

            // 반대로 Walking 상태인데 Input값이 Idle 경계값 아래일때 Idle State로 변경
            if (controller.currentMovement.magnitude <= idleThreshold &&
                movementState.CurrentState == YisoCharacterStates.MovementStates.Walking) {
                movementState.ChangeState(YisoCharacterStates.MovementStates.Idle);
                PlayAbilityStopSfx();
                PlayAbilityStopFeedbacks();
            }

            SetMovement();
        }

        /// <summary>
        /// Particle 처리
        /// </summary>
        protected virtual void HandleFeedbacks() {
            if (controller.currentMovement.magnitude > idleThreshold) {
                foreach (var system in walkParticles) {
                    if (!walkParticlesPlaying && system != null) {
                        system.Play();
                    }

                    walkParticlesPlaying = true;
                }
            }
            else {
                foreach (var system in walkParticles) {
                    if (walkParticlesPlaying && system != null) {
                        system.Stop();
                        walkParticlesPlaying = false;
                    }
                }
            }
        }

        #endregion

        #region Setter

        /// <summary>
        /// AI Character가 사용 (Input값이 없으니)
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetMovement(Vector2 value) {
            horizontalMovement = value.x;
            verticalMovement = value.y;
        }

        public virtual void SetHorizontalMovement(float value) {
            horizontalMovement = value;
        }

        public virtual void SetVerticalMovement(float value) {
            verticalMovement = value;
        }

        public virtual void ResetSpeed(float speed, bool resetMultiplier) {
            BaseMovementSpeed = baseMovementSpeed;
            MovementSpeed = character.characterType == YisoCharacter.CharacterTypes.Player
                ? YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().StatModule.MoveSpeed
                : speed;

            if (resetMultiplier) {
                if (applyMovementMultiplierCoroutine != null) StopCoroutine(applyMovementMultiplierCoroutine);
                MovementSpeedMultiplier = speed;
            }
        }

        public virtual void ResetSpeed() {
            ResetSpeed(1f, true);
        }

        protected float _movementSpeed; // 최종 Speed (movement Vector에 곱해짐)
        protected Vector3 _movementVector; // 최종적으로 이 값으로 rigidbody.MovePosition() 되는 거

        // 이 값으로 직접 Input 값이 들어오는 것 아님. 그냥 Vector2 (horizontalMovement, verticalMovement) 합친 Vector2
        protected Vector2 _currentInput = Vector2.zero;

        protected Vector2 _normalizedInput; // currentInput normalized (wasd로 움직일때 사용)
        protected Vector2 _lerpedInput = Vector2.zero; // 최종 Input (가속도, 감속도 계산한 input 값)
        protected float _acceleration = 0f;

        /// <summary>
        /// 해당 Controller에서 사용 (Update)
        /// </summary>
        protected virtual void SetMovement() {
            _movementVector = Vector3.zero;
            _currentInput = Vector2.zero;

            _currentInput.x = horizontalMovement;
            _currentInput.y = verticalMovement;

            _normalizedInput = _currentInput.normalized;

            var interpolationSpeed = 1f;

            // 가속도, 감속도가 없는 경우
            if (acceleration == 0 || deceleration == 0) {
                _lerpedInput = analogInput ? _currentInput : _normalizedInput; // wasd (analog input이 아닌 경우) 입력은 -1~1값만.
            }
            else {
                // Input 값이 없을때 (=외부 힘 X)
                if (_normalizedInput.magnitude == 0) {
                    _acceleration = Mathf.Lerp(_acceleration, 0f, deceleration * Time.deltaTime);
                    _lerpedInput = Vector2.Lerp(_lerpedInput, _lerpedInput * _acceleration,
                        deceleration * Time.deltaTime);
                    interpolationSpeed = deceleration;
                }
                // Input 값이 있을때 (=외부 힘 O)
                else {
                    _acceleration = Mathf.Lerp(_acceleration, 1f, acceleration * Time.deltaTime);
                    _lerpedInput = analogInput
                        ? Vector2.ClampMagnitude(_currentInput, _acceleration)
                        : Vector2.ClampMagnitude(_normalizedInput, _acceleration);
                    interpolationSpeed = acceleration;
                }
            }

            _movementVector.x = _lerpedInput.x;
            _movementVector.y = 0f;
            _movementVector.z = _lerpedInput.y;

            // Speed 계산 (보간하는 경우 부드럽게 출발하고 멈추도록)
            if (interpolateMovementSpeed) {
                _movementSpeed = Mathf.Lerp(_movementSpeed, MovementSpeed * MovementSpeedMultiplier,
                    interpolationSpeed * Time.deltaTime);
            }
            else {
                _movementSpeed = MovementSpeed * MovementSpeedMultiplier;
            }

            _movementVector *= _movementSpeed;

            // 현재 속도가 설정 속도 넘어가지 못하게 제한
            if (_movementVector.magnitude > MovementSpeed * MovementSpeedMultiplier) {
                _movementVector = Vector3.ClampMagnitude(_movementVector, MovementSpeed);
            }

            // currentMovement(마찰력까지 계산된 최종 movement) 크기랑 currentInput 크기가 Idle 경계값보다 작은 경우
            if (_currentInput.magnitude <= idleThreshold && controller.currentMovement.magnitude < idleThreshold) {
                _movementVector = Vector3.zero;
            }

            // YisoCharacter 에 넘겨줘.
            controller.SetMovement(_movementVector);
        }

        #endregion

        #region Movement Multiplier

        protected bool isBoost = false;

        /// <summary>
        /// duration 동안 movementMultiplier 배 만큼 속도 변화
        /// </summary>
        /// <param name="movementMultiplier"></param>
        /// <param name="duration"></param>
        public virtual void ApplyMovementMultiplier(float movementMultiplier, float duration) {
            if (isBoost) return;
            if (applyMovementMultiplierCoroutine != null) {
                StopCoroutine(applyMovementMultiplierCoroutine);
            }

            applyMovementMultiplierCoroutine = StartCoroutine(ApplyMovementMultiplierCo(movementMultiplier, duration));
        }

        protected virtual IEnumerator ApplyMovementMultiplierCo(float movementMultiplier, float duration) {
            if (isBoost) yield break;
            isBoost = true;
            MovementSpeedMultiplier = movementMultiplier;
            yield return new WaitForSeconds(duration);
            MovementSpeedMultiplier = 1f;
            isBoost = false;
        }

        #endregion

        #region Animator

        protected override void InitializeAnimatorParameters() {
            RegisterAnimatorParameter(MoveSpeedAnimationParameterName, AnimatorControllerParameterType.Float,
                out moveSpeedAnimationParameter);
            RegisterAnimatorParameter(WalkingAnimationParameterName, AnimatorControllerParameterType.Bool,
                out walkingAnimationParameter);
            RegisterAnimatorParameter(IdleAnimationParameterName, AnimatorControllerParameterType.Bool,
                out idleAnimationParameter);
        }

        public override void UpdateAnimator() {
            if (conditionState.CurrentState is YisoCharacterStates.CharacterConditions.Frozen) {
                YisoAnimatorUtils.UpdateAnimatorBool(animator, idleAnimationParameter, true,
                    character.AnimatorParameters, character.runAnimatorSanityChecks);
                YisoAnimatorUtils.UpdateAnimatorBool(animator, walkingAnimationParameter, false,
                    character.AnimatorParameters, character.runAnimatorSanityChecks);
                return;
            }

            YisoAnimatorUtils.UpdateAnimatorFloat(animator, moveSpeedAnimationParameter,
                Mathf.Abs(MovementSpeedMultiplier), character.AnimatorParameters, character.runAnimatorSanityChecks);
            YisoAnimatorUtils.UpdateAnimatorBool(animator, walkingAnimationParameter,
                movementState.CurrentState == YisoCharacterStates.MovementStates.Walking, character.AnimatorParameters,
                character.runAnimatorSanityChecks);
            YisoAnimatorUtils.UpdateAnimatorBool(animator, idleAnimationParameter,
                movementState.CurrentState == YisoCharacterStates.MovementStates.Idle, character.AnimatorParameters,
                character.runAnimatorSanityChecks);
        }

        #endregion

        #region Health

        protected override void OnRespawn() {
            base.OnRespawn();
            ResetSpeed(1f, true);
            MovementForbidden = false;
        }

        protected override void OnDeath() {
            base.OnDeath();
            SetMovement(Vector2.zero);
            DisableWalkParticles();
        }

        #endregion

        protected virtual void DisableWalkParticles() {
            if (walkParticles.Length > 0) {
                foreach (var system in walkParticles) {
                    if (system != null) {
                        system.Stop();
                    }
                }
            }
        }

        protected virtual void OnPlayerMoveSpeedChanged(float speed) {
            if (character.characterType != YisoCharacter.CharacterTypes.Player) return;
            ResetSpeed(speed, false);
        }

        protected override void OnEnable() {
            base.OnEnable();
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().StatModule.OnMoveSpeedChangedEvent += OnPlayerMoveSpeedChanged;
        }

        protected override void OnDisable() {
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().StatModule.OnMoveSpeedChangedEvent -= OnPlayerMoveSpeedChanged;
            base.OnDisable();
            DisableWalkParticles();
            PlayAbilityStopSfx();
            PlayAbilityStopFeedbacks();
            StopAbilityUsedSfx();
        }
    }
}