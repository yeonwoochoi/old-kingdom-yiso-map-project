using System;
using Character.Core;
using Character.Weapon.Aim;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Character.Ability {
    /// <summary>
    /// 이 스크립트가 외부에 영향을 미치는 경우는 Face() Method 사용하는 경우 밖에 없음.
    /// </summary>
    [AddComponentMenu("Yiso/Character/Abilities/Character Orientation 2D")]
    public class YisoCharacterOrientation2D : YisoCharacterAbility {
        public YisoCharacter.FacingDirections initialFacingDirection = YisoCharacter.FacingDirections.East;

        // Inspector 창에서 현재 Direction 확인하면 됨.
        // public이지만 외부에선 이 값 안쓰고 TopdownController에서 currentDirection을 씀
        [ReadOnly] public YisoCharacter.FacingDirections currentFacingDirection = YisoCharacter.FacingDirections.East;

        public float absoluteThresholdMovement = 0.05f;

        public Vector2 DirectionFineValue {
            get {
                var tunedValue = currentFacingDirection switch {
                    YisoCharacter.FacingDirections.West => Vector2.left,
                    YisoCharacter.FacingDirections.East => Vector2.right,
                    YisoCharacter.FacingDirections.North => Vector2.up,
                    YisoCharacter.FacingDirections.South => Vector2.down,
                    _ => Vector2.down
                };
                return tunedValue * 0.01f;
            }
        }

        public Vector2 DirectionFineAdjustmentValue => new(horizontalDirection + DirectionFineValue.x,
            verticalDirection + DirectionFineValue.y);

        [ReadOnly] public bool isFacingRight = true;

        protected bool initialized = false;
        protected YisoCharacterHandleWeapon characterHandleWeapon;
        protected YisoCharacterHandleSkill characterHandleSkill;

        // currentFacingDirection 값 결정하려고 있는 변수.
        protected float horizontalDirection; // 최종 계산된 X Direction (Animation Param으로 쓰임)
        protected float verticalDirection; // 최종 계산된 Y Direction (Animation Param으로 쓰임)
        protected float lastDirectionX; // 이전 Frame의 horizontalDirection값
        protected float lastDirectionY; // 이전 Frame의 verticalDirection값
        protected int direction; // is facing right 값의 int 버전 (1 = right, -1 = left)
        protected int directionLastFrame = 0;

        protected const string XSpeedAnimationParameterName = "X";
        protected const string YSpeedAnimationParameterName = "Y";
        protected int xSpeedAnimationParameter;
        protected int ySpeedAnimationParameter;

        public bool OrientationForbidden { get; set; } = false;

        #region Initialization

        protected override void Awake() {
            base.Awake();
            characterHandleWeapon = character.FindAbility<YisoCharacterHandleWeapon>();
            characterHandleSkill = character.FindAbility<YisoCharacterHandleSkill>();
        }

        protected override void Initialization() {
            base.Initialization();
            if (controller == null) controller = gameObject.GetComponentInParent<TopDownController>();
            controller.currentDirection = Vector3.zero;
            initialized = true;

            if (initialFacingDirection == YisoCharacter.FacingDirections.West) {
                isFacingRight = false;
                direction = -1;
            }
            else {
                isFacingRight = true;
                direction = 1;
            }

            Face(initialFacingDirection);
            directionLastFrame = 0;
            currentFacingDirection = initialFacingDirection;
            switch (initialFacingDirection) {
                case YisoCharacter.FacingDirections.West:
                    lastDirectionX = -1f;
                    lastDirectionY = 0f;
                    break;
                case YisoCharacter.FacingDirections.East:
                    lastDirectionX = 1f;
                    lastDirectionY = 0f;
                    break;
                case YisoCharacter.FacingDirections.North:
                    lastDirectionX = 0f;
                    lastDirectionY = 1f;
                    break;
                case YisoCharacter.FacingDirections.South:
                    lastDirectionX = 0f;
                    lastDirectionY = -1f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Update

        protected float lastNonNullXMovement;

        public override void ProcessAbility() {
            base.ProcessAbility();

            if (conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Normal) return;
            if (!AbilityAuthorized) return;
            if (OrientationForbidden) return;

            DetermineFacingDirection();
            CheckMovementDirectionIsRight();
            CheckWeaponDirectionIsRight();

            directionLastFrame = direction;
            lastNonNullXMovement = (Mathf.Abs(controller.currentDirection.x) > 0)
                ? controller.currentDirection.x
                : lastNonNullXMovement;
        }

        /// <summary>
        /// controller의 currentMovement 값과
        /// WeaponAim의 currentAimAbsolute 값을
        /// 바탕으로 horizontalDirection, verticalDirection 을 결정함 (=> 이를 바탕으로 currentFacingDirection 값도)
        /// </summary>
        protected virtual void DetermineFacingDirection() {
            if (controller.currentDirection == Vector3.zero) {
                ApplyCurrentDirection();
            }

            // Weapon Aim
            var isWeaponAimAvailable = characterHandleWeapon?.CurrentWeaponAim != null;
            var currentWeaponAimAbsolute = isWeaponAimAvailable ? characterHandleWeapon.CurrentWeaponAim.CurrentAimAbsolute : Vector2.zero;
            var isCharacterAttacking = character.movementState.CurrentState is YisoCharacterStates.MovementStates.Attacking;
            
            // Skill Aim
            var isSkillAimAvailable = characterHandleSkill?.CurrentAttackSkillAim != null;
            var currentSkillAimAbsolute = isSkillAimAvailable ? characterHandleSkill.CurrentAttackSkillAim.CurrentAimAbsolute : Vector2.zero;
            var isCharacterSkillCasting = character.movementState.CurrentState is YisoCharacterStates.MovementStates.SkillCasting;
            
            // Set State Variables
            var isMoving = controller.currentMovement.normalized.magnitude >= absoluteThresholdMovement;
            var isAttacking = isWeaponAimAvailable && isCharacterAttacking;
            var isAttackSkillCasting = isSkillAimAvailable && isCharacterSkillCasting;
            
            Vector2 currentDirection;
            var shouldApplyWeaponAimSync = false;
            
            // Set Current Directions
            // 1. 스킬 공격하는 경우 (skillAim)
            if (isAttackSkillCasting) {
                currentDirection = currentSkillAimAbsolute.normalized;
                shouldApplyWeaponAimSync = true;
            }
            // 2. 공격하는 경우 (weaponAim)
            else if (isAttacking) {
                currentDirection = currentWeaponAimAbsolute.normalized;
                shouldApplyWeaponAimSync = true;
            }
            // 3. 움직이기만 하는 경우 (controller.currentDirection)
            else if (isMoving) {
                currentDirection = controller.currentDirection;
                currentDirection.x = Mathf.Abs(currentDirection.x) >= absoluteThresholdMovement
                    ? currentDirection.x
                    : 0f;
                currentDirection.y = Mathf.Abs(currentDirection.y) >= absoluteThresholdMovement
                    ? currentDirection.y
                    : 0f;
            }
            // 4. 움직이지도 공격하지도 않는 경우
            else {
                currentDirection = new Vector2(lastDirectionX, lastDirectionY);
            }

            horizontalDirection = currentDirection.x;
            verticalDirection = currentDirection.y;

            currentFacingDirection = YisoPhysicsUtils.GetDirectionFromVector(currentDirection);
            if (shouldApplyWeaponAimSync) ApplyCurrentDirection();
            lastDirectionX = horizontalDirection;
            lastDirectionY = verticalDirection;
        }

        protected virtual void CheckMovementDirectionIsRight() {
            if (controller.currentDirection.normalized.magnitude >= absoluteThresholdMovement) {
                var checkedDirection = (Mathf.Abs(controller.currentDirection.normalized.x) > 0)
                    ? controller.currentDirection.normalized.x
                    : lastDirectionX;

                if (checkedDirection >= 0) {
                    FaceDirection(1);
                }
                else {
                    FaceDirection(-1);
                }
            }
        }

        protected virtual void CheckWeaponDirectionIsRight() {
            if (characterHandleWeapon == null) return;
            if (characterHandleWeapon.CurrentWeaponAim != null) {
                if (characterHandleWeapon.CurrentWeaponAim.aimControl == YisoBaseAim.AimControls.Movement
                    && characterHandleWeapon.CurrentWeaponAim.useSecondaryMovement
                    && character.movementState.CurrentState != YisoCharacterStates.MovementStates.Attacking) {
                    return;
                }

                var weaponAngle = characterHandleWeapon.CurrentWeaponAim.CurrentAngleAbsolute;
                if (weaponAngle > 90 || weaponAngle < -90) {
                    FaceDirection(-1);
                }
                else {
                    FaceDirection(1);
                }
            }
        }

        #endregion

        #region Core

        /// <summary>
        /// 이 ability는 자체적으로 TopdownController의 currentDirection값을 보고 currentFacingDirection을 계산함
        /// 이 method는 currentFacingDirection을 강제한 후 TopdownController의 currentDirection값에 적용시키는 것임.
        /// </summary>
        /// <param name="newDirection"></param>
        public virtual void Face(YisoCharacter.FacingDirections newDirection) {
            if (OrientationForbidden) return;
            currentFacingDirection = newDirection;
            ApplyCurrentDirection();
            if (newDirection == YisoCharacter.FacingDirections.West) {
                FaceDirection(-1);
            }

            if (newDirection == YisoCharacter.FacingDirections.East) {
                FaceDirection(1);
            }
        }

        public virtual void FaceDirection(int newDirection) {
            direction = newDirection;
            isFacingRight = direction == 1;
        }

        /// <summary>
        /// TopdownController에 있는 currentDirection값에 이 Ability에서 계산된 값 적용
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected virtual void ApplyCurrentDirection() {
            if (!initialized) Initialization();

            switch (currentFacingDirection) {
                case YisoCharacter.FacingDirections.West:
                    controller.currentDirection = Vector3.left;
                    break;
                case YisoCharacter.FacingDirections.East:
                    controller.currentDirection = Vector3.right;
                    break;
                case YisoCharacter.FacingDirections.North:
                    controller.currentDirection = Vector3.up;
                    break;
                case YisoCharacter.FacingDirections.South:
                    controller.currentDirection = Vector3.down;
                    break;
            }
        }

        #endregion

        #region Animator

        protected override void InitializeAnimatorParameters() {
            RegisterAnimatorParameter(XSpeedAnimationParameterName, AnimatorControllerParameterType.Float,
                out xSpeedAnimationParameter);
            RegisterAnimatorParameter(YSpeedAnimationParameterName, AnimatorControllerParameterType.Float,
                out ySpeedAnimationParameter);
        }

        public override void UpdateAnimator() {
            YisoAnimatorUtils.UpdateAnimatorFloat(animator, xSpeedAnimationParameter, DirectionFineAdjustmentValue.x,
                character.AnimatorParameters, character.runAnimatorSanityChecks);
            YisoAnimatorUtils.UpdateAnimatorFloat(animator, ySpeedAnimationParameter, DirectionFineAdjustmentValue.y,
                character.AnimatorParameters, character.runAnimatorSanityChecks);
        }

        #endregion
    }
}