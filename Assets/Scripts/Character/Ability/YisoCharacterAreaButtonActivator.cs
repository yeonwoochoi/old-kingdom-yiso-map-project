using Character.Core;
using Core.Service;
using Core.Service.UI.HUD;
using Sirenix.OdinInspector;
using Tools.Environment;
using Tools.Inputs;
using UI.HUD.Interact;
using UnityEngine;

namespace Character.Ability {
    /// <summary>
    /// 캐릭터가 특정 영역에 들어갔을 때 버튼을 활성화하는 기능을 담당
    /// </summary>
    [AddComponentMenu("Yiso/Character/Abilities/Character Area Button Activator")]
    public class YisoCharacterAreaButtonActivator : YisoCharacterAbility {
        public bool IsInButtonActivatedArea { get; set; } // 캐릭터가 Area에 있는지 여부
        public bool IsAutoActivatedArea { get; set; } // Area가 버튼을 누르지 않고도 바로 Action Trigger되는지 (Auto)
        public YisoHudUIInteractTypes InteractType { get; set; } // Button Action이 어떤 타입인지
        [ReadOnly] public YisoActionTriggerZone buttonActivatedArea;

        public IYisoHUDUIService HUDUIService => YisoServiceProvider.Instance.Get<IYisoHUDUIService>();

        protected override void Initialization() {
            base.Initialization();
            IsInButtonActivatedArea = false;
            IsAutoActivatedArea = false;
            buttonActivatedArea = null;
        }

        protected override void HandleInput() {
            if (!AbilityAuthorized) return;
            if (IsInButtonActivatedArea && buttonActivatedArea != null) {
                if (inputManager.IsMobile) {
                    // TODO: HUD Interaction Button을 활성화
                    if (CheckTriggerAction()) {
                        if (!HUDUIService.IsReady()) return;
                        HUDUIService.ShowInteractButton(InteractType, TriggerAction);
                    }
                }
                else {
                    if (inputManager.InteractButton.State.CurrentState is YisoInput.ButtonStates.ButtonDown or YisoInput
                        .ButtonStates.ButtonPressed) {
                        TriggerAction();
                    }
                }
            }
        }

        /// <summary>
        /// Normal, Frozen 상태일때만 Activate됨
        /// Dashing 중에는 Activate 안 됨
        /// Auto Activation은 알아서 Character가 Area로 들어오면 Trigger되니 여기서도 Trigger하면 중복됨
        /// </summary>
        protected virtual void TriggerAction() {
            if (!CheckTriggerAction()) return;
            buttonActivatedArea.TriggerButtonAction();
            PlayAbilityStartFeedbacks();
        }

        protected override void OnDeath() {
            base.OnDeath();
            IsInButtonActivatedArea = false;
            IsAutoActivatedArea = false;
            buttonActivatedArea = null;
        }

        protected virtual bool CheckTriggerAction() {
            if (buttonActivatedArea == null) return false;
            if (!IsInButtonActivatedArea) return false;
            if (conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Normal &&
                conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Frozen) return false;
            if (movementState.CurrentState == YisoCharacterStates.MovementStates.Dashing) return false;
            if (IsAutoActivatedArea) return false;
            return true;
        }
    }
}