using Character.Core;
using Sirenix.OdinInspector;
using Tools.Inputs;
using UnityEngine;
using Utils.Beagle;

namespace Character.Ability {
    [AddComponentMenu("Yiso/Character/Abilities/Character Run")]
    public class YisoCharacterRun : YisoCharacterAbility {
        [Title("Speed")] public float runSpeed = 5f;

        public bool AutoRun => inputManager != null && inputManager.IsMobile;
        protected bool runningStarted = false;

        protected readonly float autoRunThreshold = 0.99f;
        protected const string RunningAnimationParameterName = "IsRunning";
        protected int runningAnimationParameter;

        protected override void HandleInput() {
            base.HandleInput();
            if (AutoRun) {
                if (inputManager.Movement.magnitude > autoRunThreshold) {
                    inputManager.RunButton.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
                }
            }

            if (inputManager.RunButton.State.CurrentState is YisoInput.ButtonStates.ButtonDown or YisoInput.ButtonStates
                .ButtonPressed) {
                RunStart();
            }

            if (inputManager.RunButton.State.CurrentState is YisoInput.ButtonStates.ButtonUp) {
                RunStop();
            }
            else {
                if (AutoRun) {
                    if (inputManager.Movement.magnitude <= autoRunThreshold) {
                        inputManager.RunButton.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
                        RunStop();
                    }
                }
            }
        }

        public override void ProcessAbility() {
            base.ProcessAbility();
            HandleRunningExit();
        }

        public virtual void RunStart() {
            if (!CanRun()) return;
            if (characterMovement != null) {
                characterMovement.BaseMovementSpeed = runSpeed;
            }

            if (movementState.CurrentState != YisoCharacterStates.MovementStates.Running) {
                PlayAbilityStartSfx();
                PlayAbilityUsedSfx();
                PlayAbilityStartFeedbacks();
                runningStarted = true;
            }

            movementState.ChangeState(YisoCharacterStates.MovementStates.Running);
        }

        public virtual void RunStop() {
            if (!runningStarted) return;
            if (characterMovement != null) {
                characterMovement.ResetSpeed();
                movementState.ChangeState(YisoCharacterStates.MovementStates.Idle);
            }
            
            StopFeedbacks();
            StopSfx();
            runningStarted = false;
        }

        protected virtual bool CanRun() {
            return AbilityAuthorized && conditionState.CurrentState == YisoCharacterStates.CharacterConditions.Normal &&
                   movementState.CurrentState == YisoCharacterStates.MovementStates.Walking;
        }

        protected virtual void HandleRunningExit() {
            if (conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Normal) {
                StopAbilityUsedSfx();
            }

            if (movementState.CurrentState is YisoCharacterStates.MovementStates.Running &&
                abilityInProgressSfx != null && currentAbilityInProgressSfx == null) {
                PlayAbilityStartSfx();
            }

            if (Mathf.Abs(controller.currentMovement.magnitude) < (runSpeed / 10) && movementState.CurrentState is YisoCharacterStates.MovementStates.Running) {
                movementState.ChangeState(YisoCharacterStates.MovementStates.Idle);
                StopFeedbacks();
                StopSfx();
            }
        }

        /// <summary>
        /// Stops all run feedbacks
        /// </summary>
        protected virtual void StopFeedbacks() {
            if (!startFeedbackIsPlaying) return;
            StopStartFeedbacks();
            PlayAbilityStopFeedbacks();
        }

        /// <summary>
        /// Stops all run sounds
        /// </summary>
        protected virtual void StopSfx() {
            StopAbilityUsedSfx();
            PlayAbilityStopSfx();
        }

        protected override void InitializeAnimatorParameters() {
            RegisterAnimatorParameter(RunningAnimationParameterName, AnimatorControllerParameterType.Bool, out runningAnimationParameter);
        }

        public override void UpdateAnimator() {
            YisoAnimatorUtils.UpdateAnimatorBool(animator, runningAnimationParameter, movementState.CurrentState is YisoCharacterStates.MovementStates.Running, character.AnimatorParameters, character.runAnimatorSanityChecks);
        }
    }
}