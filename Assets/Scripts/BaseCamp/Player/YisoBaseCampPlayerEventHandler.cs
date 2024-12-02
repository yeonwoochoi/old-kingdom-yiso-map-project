using BaseCamp.Manager;
using Core.Service;
using Core.Service.UI;
using UnityEngine;

namespace BaseCamp.Player {
    public sealed class YisoBaseCampPlayerEventHandler {
        private readonly YisoBaseCampPlayerController controller;
        private readonly YisoBaseCampPlayerInputController inputController;
        private bool preventInput = false;

        private readonly YisoBaseCampManager baseCampManager;

        internal YisoBaseCampPlayerEventHandler(YisoBaseCampPlayerController controller) {
            this.controller = controller;
            inputController = controller.GetOrAddComponent<YisoBaseCampPlayerInputController>();
            this.controller.AddOnEnable(RegisterEvents);
            this.controller.AddOnDisable(UnregisterEvents);
            baseCampManager = YisoBaseCampManager.Instance;
        }

        private void RegisterEvents() {
            inputController.OnInputEvent += OnInput;
            var uiService = YisoServiceProvider.Instance.Get<IYisoUIService>();
            // uiService.GetBaseCampJoystick().OnJoystickInputEvent += OnIsJoystickInputEvent;
            // uiService.GetBaseCampJoystick().OnJoystickMovementEvent += OnJoystickInputEvent;
        }

        private void UnregisterEvents() {
            inputController.OnInputEvent -= OnInput;
            var uiService = YisoServiceProvider.Instance.Get<IYisoUIService>();
            // uiService.GetBaseCampJoystick().OnJoystickInputEvent -= OnIsJoystickInputEvent;
            // uiService.GetBaseCampJoystick().OnJoystickMovementEvent -= OnJoystickInputEvent;
        }
        
        private void OnIsJoystickInputEvent(bool value) {
            controller.joystickInput = value;
        }

        private void OnJoystickInputEvent(Vector2 value) {
            if (preventInput) return;
            controller.animator.IsMoving = value != Vector2.zero;
            controller.movementInput = value;
        }
        
        private void OnInput(YisoBaseCampPlayerInputEventArgs args) {
            switch (args) {
                case YisoBaseCampPlayerMovementInputEventArgs movementInput:
                    if (!controller.joystickInput)
                        OnJoystickInputEvent(movementInput.Movement);
                    break;
                case YisoBaseCampPlayerInteractInputEventArgs _:
                    if (!controller.interactInfo.Active) return;
                    controller.interactInfo.OnClick?.Invoke();
                    break;
            }
        }
    }
}