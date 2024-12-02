using Character.Ability;
using Character.Core;
using Core.Behaviour;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;

namespace Character.Weapon.Aim {
    public abstract class YisoBaseAim : RunIBehaviour {
        public enum AimControls {
            Script,
            Movement,
            Mouse
        }

        public AimControls aimControl;
        public bool useSecondaryMovement = true;

        public bool aimControlActive = true;
        public float minimumAngle = -180f;
        public float maximumAngle = 180f;
        public float minimumMagnitude = 0.2f; // input 값이 minimum Magnitude가 넘어야 반영됨 (InputManager의 Movement 값)
        [ReadOnly] public Vector3 currentAim = Vector3.zero; // 최종 무기 공격 방향
        [ReadOnly] public Vector3 currentAimAbsolute = Vector3.zero;
        
        public Vector2 CurrentAim => currentAim;
        public Vector2 CurrentAimAbsolute => currentAimAbsolute;
        public float CurrentAngle => currentAngle;
        public float CurrentAngleAbsolute => currentAngleAbsolute;

        protected bool initialized = false;
        protected Quaternion initialRotation;
        
        protected float currentAngle; // "currentAim"을 Degree (Radian아님)로 변환한 값
        protected float currentAngleAbsolute;

        protected Quaternion currentRotation; // "currentAim"을 Quaternion으로 변환한 값
        // protected Vector3 direction;

        protected UnityEngine.Camera mainCamera;
        protected bool hasOrientation2D = false;
        protected Vector2 inputMovement;
        protected Vector2 lastNonNullMovement = Vector2.zero;
        protected float[] possibleAngleValues;

        protected YisoCharacter ownerCharacter;
        protected YisoCharacterOrientation2D ownerOrientation2D;

        #region Initialization

        protected override void Start() {
            Initialization();
        }

        public virtual void Initialization() {
            if (initialized) return;
            mainCamera = UnityEngine.Camera.main;

            possibleAngleValues = new float[9];
            possibleAngleValues[0] = -180f;
            possibleAngleValues[1] = -135f;
            possibleAngleValues[2] = -90f;
            possibleAngleValues[3] = -45f;
            possibleAngleValues[4] = 0f;
            possibleAngleValues[5] = 45f;
            possibleAngleValues[6] = 90f;
            possibleAngleValues[7] = 135f;
            possibleAngleValues[8] = 180f;

            initialRotation = transform.rotation;
        }

        public virtual void ApplyAim(Vector3 aim, bool forceSet) {
            Initialization();
            UpdateCurrentAim();
            if (forceSet) {
                currentAim = aim;
                currentAimAbsolute = currentAim;
            }

            DetermineWeaponRotation();
        }

        #endregion

        #region Update

        public override void OnUpdate() {
            UpdateCurrentAim(); // current Aim 계산
            DetermineWeaponRotation(); // current Angle 계산
        }

        /// <summary>
        /// 외부에서 Aim 설정하고 싶을 때
        /// 보통 "AI" 캐릭터가 이 method 사용 많이 하겠지.
        /// </summary>
        /// <param name="newAim"></param>
        public virtual void SetCurrentAim(Vector3 newAim) {
            if (!aimControlActive) return;
            currentAim = newAim;
            lastNonNullMovement.x = newAim.x;
            lastNonNullMovement.y = newAim.y;
        }

        public virtual void SyncAim() {
            currentAimAbsolute = currentAim;
        }

        /// <summary>
        /// current Aim 값 계산한 다음 넘겨줘
        /// DetermineWeaponRotation에서 current Aim 가지고 current Angle 계산함
        /// </summary>
        protected virtual void UpdateCurrentAim() {
            if (!aimControlActive) return;
            if (ownerCharacter == null) return;
            if (ownerCharacter.LinkedInputManager == null &&
                ownerCharacter.characterType == YisoCharacter.CharacterTypes.Player) return;

            switch (aimControl) {
                case AimControls.Script:
                    UpdateScriptAim();
                    break;
                case AimControls.Movement:
                    UpdateMovementAim();
                    break;
                case AimControls.Mouse:
                    UpdateMouseAim();
                    break;
            }
        }

        /// <summary>
        /// SetCurrentAim에서 직접 currentAim을 설정함
        /// AI
        /// </summary>
        protected virtual void UpdateScriptAim() {
            currentAimAbsolute = currentAim;
            // direction = currentAim - transform.position;
        }

        /// <summary>
        /// Current Aim 계산 (aimControl = Movement)
        /// Player
        /// </summary>
        protected virtual void UpdateMovementAim() {
            if (!aimControlActive) return;
            if (ownerCharacter.LinkedInputManager == null) return;

            // Get Input Value (Secondary로 할건지 Primary로 할건지)
            inputMovement = useSecondaryMovement
                ? ownerCharacter.LinkedInputManager.SecondaryMovement
                : ownerCharacter.LinkedInputManager.Movement;

            // 일정 이상의 Input 들어왔을때
            if (inputMovement.magnitude <= minimumMagnitude) return;

            // last Movement zero인경우 Input manager에 저장된 값 가져옴
            if (lastNonNullMovement == Vector2.zero) {
                lastNonNullMovement = useSecondaryMovement
                    ? ownerCharacter.LinkedInputManager.LastSecondaryMovement
                    : ownerCharacter.LinkedInputManager.LastMovement;
            }

            // input movement를 Input Manager로부터 가져와서
            inputMovement = inputMovement.magnitude > 0 ? inputMovement : lastNonNullMovement;

            currentAimAbsolute = inputMovement;

            // current Aim 설정
            if (hasOrientation2D) {
                if (ownerOrientation2D.isFacingRight) {
                    currentAim = inputMovement;
                    // direction = transform.position + currentAim;
                }
                else {
                    currentAim = inputMovement;
                    // direction = currentAim - transform.position;
                }
            }
            else {
                currentAim = inputMovement;
                // direction = transform.position + currentAim;
            }

            // last Movement 저장
            lastNonNullMovement = inputMovement.magnitude > 0 ? inputMovement : lastNonNullMovement;
        }

        protected virtual void UpdateMouseAim() {
            if (!aimControlActive) return;
            if (ownerCharacter == null || ownerCharacter.LinkedInputManager == null) return;

            var inputManager = ownerCharacter.LinkedInputManager;
            inputMovement = useSecondaryMovement ? inputManager.SecondaryMovement : inputManager.Movement;

            if (lastNonNullMovement == Vector2.zero) {
                lastNonNullMovement = useSecondaryMovement ? inputManager.LastSecondaryMovement : inputManager.LastMovement;
            }

            // 기본 inputMovement가 0일 때 Main Movement를 가져오도록 함
            if (inputMovement.magnitude <= 0 && useSecondaryMovement) {
                inputMovement = inputManager.Movement;
            }

            // 조준 관련 값 업데이트
            currentAim = inputMovement;
            currentAimAbsolute = currentAim;
            lastNonNullMovement = inputMovement.magnitude > 0 ? inputMovement : lastNonNullMovement;
        }

        private bool facingRightLastFrame = false;

        /// <summary>
        /// Calculate Current Angle (by Current Aim) => Apply Angle
        /// </summary>
        protected virtual void DetermineWeaponRotation() {
            if (currentAim != Vector3.zero) {
                // if (direction == Vector3.zero) return;
                // current Aim (vector2) -> Degree (float)
                currentAngle = Mathf.Atan2(currentAim.y, currentAim.x) * Mathf.Rad2Deg;
                currentAngleAbsolute = Mathf.Atan2(currentAimAbsolute.y, currentAimAbsolute.x) * Mathf.Rad2Deg;

                // currentAngle = YisoMathUtils.RoundToClosest(currentAngle, possibleAngleValues);

                var flip = false;
                if (hasOrientation2D) {
                    currentAngle = ownerOrientation2D.isFacingRight
                        ? Mathf.Clamp(currentAngle, minimumAngle, maximumAngle)
                        : Mathf.Clamp(currentAngle, -maximumAngle, -minimumAngle);
                    flip = facingRightLastFrame != ownerOrientation2D.isFacingRight;
                    facingRightLastFrame = ownerOrientation2D.isFacingRight;
                }
                else {
                    currentAngle = Mathf.Clamp(currentAngle, minimumAngle, maximumAngle);
                }

                currentRotation = Quaternion.Euler(currentAngle * Vector3.forward);
                RotateWeapon(currentRotation, flip);
            }
            else {
                currentAngle = 0f;
                RotateWeapon(initialRotation);
            }

#if UNITY_EDITOR
            YisoDebugUtils.DebugDrawArrow(transform.position, currentAimAbsolute.normalized, Color.green);
#endif
        }

        /// <summary>
        /// Apply Rotation to Damage Area
        /// </summary>
        /// <param name="newRotation"></param>
        /// <param name="forceInstant"></param>
        protected virtual void RotateWeapon(Quaternion newRotation, bool forceInstant = false) {
            transform.rotation = newRotation;
            // transform.rotation = forceInstant 
            //     ? newRotation 
            //     : Quaternion.Slerp(transform.rotation, newRotation, 100f * Time.deltaTime);
        }

        #endregion
    }
}