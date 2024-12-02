using Character.Core;
using Controller.Effect;
using Sirenix.OdinInspector;
using Tools.Cool;
using Tools.Feedback.Core;
using Tools.Inputs;
using UnityEngine;
using Utils.Beagle;

namespace Character.Ability {
    [AddComponentMenu("Yiso/Character/Abilities/Character Dodge")]
    public class YisoCharacterDodge: YisoCharacterAbility {
        [Title("Dodge")]
        public float dodgeDistance = 3f;
        public float dodgeDuration = 0.4f;
        public AnimationCurve dodgeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        
        [Title("Cooldown")]
        public YisoCooldown cooldown;

        [Title("Invincible")]
        public bool invincibleWhileDodge = false; // dodge 동안 무적

        [Title("Feedback")]
        public bool useAfterImage = false;
        public YisoFeedBacks dodgeFeedback;

        public bool DodgeForbidden { get; set; }
        public bool Dodging => dodging;

        protected bool dodging = false;
        protected Vector3 dodgeOrigin; // Dodge 출발위치
        protected Vector3 dodgeDestination; // Dodge 도착위치
        protected Vector3 dodgeDirection; // Dodge 도착위치
        protected Vector3 newPosition; // 각 Frame 마다 도달하기 위한 위치
        protected float dodgeTimer;
        protected YisoAfterImageGenerator afterImageGenerator;
        protected YisoCharacterOrientation2D orientation2D;
        
        protected const string DodgeAnimationParameterName = "IsDodging";
        protected int dodgeAnimationParameter;

        #region Initialization

        protected override void PreInitialization() {
            base.PreInitialization();
            if (useAfterImage) {
                if (!character.characterModel.TryGetComponent(out afterImageGenerator)) {
                    afterImageGenerator = character.characterModel.AddComponent<YisoAfterImageGenerator>();
                }
            }
        }

        protected override void Initialization() {
            base.Initialization();

            dodging = false;
            DodgeForbidden = false;
            cooldown.Initialization();
            if (useAfterImage) afterImageGenerator.Initialization();
            dodgeFeedback?.Initialization(gameObject);
            orientation2D = character.Orientation2D;
        }

        #endregion

        #region Public API

        public virtual void DodgeStart() {
            DodgeStart(Vector3.zero);
        }

        public virtual void DodgeStart(Vector3 destination) {
            if (!CanDodge()) return;

            dodgeTimer = 0f;
            dodgeOrigin = transform.position;
            if (character.characterType == YisoCharacter.CharacterTypes.Player) {
                dodgeDestination = transform.position + controller.currentDirection.normalized * dodgeDistance * -1f;
            }
            else {
                dodgeDestination = destination;
            }
            
            dodgeDirection = dodgeOrigin - dodgeDestination;
            if (inputManager != null) inputManager.ResetMovement(dodgeDestination);
            if (orientation2D != null) {
                orientation2D.Face(YisoPhysicsUtils.GetDirectionFromVector(dodgeDirection));
                orientation2D.OrientationForbidden = true;
            }
            
            dodging = true;

            cooldown.Start();
            movementState.ChangeState(YisoCharacterStates.MovementStates.Dodging);
            if (useAfterImage) afterImageGenerator.active = true;

            controller.freeMovement = false;

            // Start Feedback
            dodgeFeedback?.PlayFeedbacks(transform.position);
            PlayAbilityStartFeedbacks();

            if (invincibleWhileDodge) {
                health.DamageDisabled();
            }
        }

        public virtual void DodgeStop() {
            if (!dodging) return;
            dodgeFeedback?.StopFeedbacks(transform.position);
            
            StopStartFeedbacks();
            PlayAbilityStartFeedbacks();

            if (invincibleWhileDodge) {
                health.DamageEnabled();
            }

            if (useAfterImage) afterImageGenerator.active = false;
            movementState.ChangeState(YisoCharacterStates.MovementStates.Idle);
            dodging = false;
            controller.freeMovement = true;

            if (inputManager != null) inputManager.ResetMovement();
            if (orientation2D != null) orientation2D.OrientationForbidden = false;
        }

        public virtual float GetDodgeProgress() {
            if (!dodging) return 0f;

            var totalDistance = Vector3.Distance(dodgeOrigin, dodgeDestination);
            var currentDistance = Vector3.Distance(dodgeOrigin, transform.position);

            if (totalDistance == 0f) return 0f;

            var dashProgress = currentDistance / totalDistance;

            if (dashProgress < 0f) return 0f;
            if (dashProgress > 1f) return 1f;
            return dashProgress;
        }

        protected virtual bool CanDodge() {
            if (DodgeForbidden) return false;
            if (!abilityInitialized) return false;
            if (!AbilityAuthorized) return false;
            if (!cooldown.Ready) return false;
            if (conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Normal) return false;
            return true;
        }

        #endregion

        #region Core

        protected override void HandleInput() {
            base.HandleInput();
            if (inputManager.DodgeButton.State.CurrentState is YisoInput.ButtonStates.ButtonDown) {
                DodgeStart();
            }
        }

        public override void ProcessAbility() {
            base.ProcessAbility();
            cooldown.Update();
            UpdateDodgeCooldownButton();

            if (dodging) {
                if (dodgeTimer < dodgeDuration) {
                    newPosition = Vector3.Lerp(dodgeOrigin, dodgeDestination,
                        dodgeCurve.Evaluate(dodgeTimer / dodgeDuration));
                    dodgeTimer += Time.deltaTime;
                    controller.MovePosition(newPosition);
                }
                else {
                    DodgeStop();
                }
            }
        }

        #endregion
        
        protected virtual void UpdateDodgeCooldownButton() {
            // TODO (Dodge) : 스킬 버튼에 쿨타임이랑 원형 progress bar 표시 (다른 스킬도 마찬가지로 필요) 
        }
        
        protected override void OnDestroy() {
            if (useAfterImage) afterImageGenerator.DestroyAfterImages();
        }

        #region Animator

        protected override void InitializeAnimatorParameters() {
            RegisterAnimatorParameter(DodgeAnimationParameterName, AnimatorControllerParameterType.Bool, out dodgeAnimationParameter);
        }

        public override void UpdateAnimator() {
            YisoAnimatorUtils.UpdateAnimatorBool(animator, dodgeAnimationParameter, movementState.CurrentState is YisoCharacterStates.MovementStates.Dodging, character.AnimatorParameters, character.runAnimatorSanityChecks);
        }

        #endregion
    }
}