using Character.Core;
using Controller.Effect;
using Sirenix.OdinInspector;
using Tools.Cool;
using Tools.Feedback;
using Tools.Feedback.Core;
using Tools.Inputs;
using UnityEngine;

namespace Character.Ability {
    [AddComponentMenu("Yiso/Character/Abilities/Character Dash")]
    public class YisoCharacterDash : YisoCharacterAbility {
        [Title("Dash")] public float dashDistance = 2.1f;
        public float dashDuration = 0.2f;
        public AnimationCurve dashCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        [Title("Cooldown")] public YisoCooldown cooldown;

        [Title("Invincible")] public bool invincibleWhileDashing = false; // dash 동안 무적

        [Title("Feedback")] public YisoFeedBacks dashFeedback;

        public bool DashForbidden { get; set; }
        public bool Dashing => dashing;

        protected bool dashing = false;
        protected Vector3 dashOrigin; // Dash 출발위치
        protected Vector3 dashDestination; // Dash 도착위치
        protected Vector3 newPosition; // 각 Frame 마다 도달하기 위한 위치
        protected float dashTimer;
        protected YisoAfterImageGenerator afterImageGenerator;

        #region Initialization

        protected override void PreInitialization() {
            base.PreInitialization();
            if (!character.characterModel.TryGetComponent(out afterImageGenerator)) {
                afterImageGenerator = character.characterModel.AddComponent<YisoAfterImageGenerator>();
            }
        }

        /// <summary>
        /// Start에 실행됨
        /// </summary>
        protected override void Initialization() {
            base.Initialization();

            dashing = false;
            DashForbidden = false;
            cooldown.Initialization();
            afterImageGenerator.Initialization();
            dashFeedback?.Initialization(gameObject);
        }

        #endregion

        #region Public API

        public virtual void DashStart() {
            DashStart(Vector3.zero);
        }

        public virtual void DashStart(Vector3 destination) {
            if (DashForbidden) return;
            if (!abilityInitialized) return;
            if (!AbilityAuthorized) return;
            if (!cooldown.Ready) return;
            if (conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Normal) return;

            // Set dash params
            dashTimer = 0f;
            dashOrigin = transform.position;
            if (character.characterType == YisoCharacter.CharacterTypes.Player) {
                dashDestination = transform.position + controller.currentDirection.normalized * dashDistance;
            }
            else {
                dashDestination = destination;
            }
            
            if (inputManager != null) inputManager.ResetMovement(dashDestination);

            dashing = true;

            cooldown.Start();
            movementState.ChangeState(YisoCharacterStates.MovementStates.Dashing);
            afterImageGenerator.active = true;

            controller.freeMovement = false;

            // Start Feedback
            dashFeedback?.PlayFeedbacks(transform.position);
            PlayAbilityStartFeedbacks();

            if (invincibleWhileDashing) {
                health.DamageDisabled();
            }
        }

        public virtual void DashStop() {
            if (!dashing) return;
            dashFeedback?.StopFeedbacks(transform.position);

            StopStartFeedbacks();
            PlayAbilityStopFeedbacks();

            if (invincibleWhileDashing) {
                health.DamageEnabled();
            }

            afterImageGenerator.active = false;
            movementState.ChangeState(YisoCharacterStates.MovementStates.Idle);
            dashing = false;
            controller.freeMovement = true;

            if (inputManager != null) inputManager.ResetMovement();
        }

        public virtual float GetDashProgress() {
            if (!dashing) return 0f;

            var totalDistance = Vector3.Distance(dashOrigin, dashDestination);
            var currentDistance = Vector3.Distance(dashOrigin, transform.position);

            if (totalDistance == 0f) return 0f;

            var dashProgress = currentDistance / totalDistance;

            if (dashProgress < 0f) return 0f;
            if (dashProgress > 1f) return 1f;
            return dashProgress;
        }

        #endregion

        #region Core

        /// <summary>
        /// Update에 실행됨 (Process Ability 이전에 실행됨)
        /// </summary>
        protected override void HandleInput() {
            base.HandleInput();
            if (inputManager.DashButton.State.CurrentState is YisoInput.ButtonStates.ButtonDown) {
                DashStart();
            }
        }

        /// <summary>
        /// Update에 실행됨
        /// </summary>
        public override void ProcessAbility() {
            base.ProcessAbility();
            cooldown.Update();
            UpdateDashCooldownButton();

            if (dashing) {
                if (dashTimer < dashDuration) {
                    newPosition = Vector3.Lerp(dashOrigin, dashDestination,
                        dashCurve.Evaluate(dashTimer / dashDuration));
                    dashTimer += Time.deltaTime;
                    controller.MovePosition(newPosition);
                }
                else {
                    DashStop();
                }
            }
        }

        #endregion

        protected virtual void UpdateDashCooldownButton() {
            // TODO (Dash) : 스킬 버튼에 쿨타임이랑 원형 progress bar 표시 (다른 스킬도 마찬가지로 필요) 
        }

        protected override void OnDestroy() {
            afterImageGenerator.DestroyAfterImages();
        }
    }
}