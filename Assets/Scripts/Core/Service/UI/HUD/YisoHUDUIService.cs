using Core.Domain.Types;
using Core.Service.UI.Event;
using UI.HUD;
using UI.HUD.Interact;
using UI.HUD.Timer;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Service.UI.HUD {
    public class YisoHUDUIService : IYisoHUDUIService {
        private event UnityAction<Vector2> OnJoystickMovementEvent;
        private event UnityAction<bool> OnJoystickInputEvent;
        private event UnityAction<YisoSelectionInputStates> OnAttackInputEvent;

        private event UnityAction<YisoSelectionInputStates> OnReviveInputEvent; 
        private event UnityAction<YisoHUDEventArgs> OnHUDEvent;
        private event UnityAction<bool> OnVisibleHUDEvent;
        private event UnityAction<UnityAction<int>> OnQuestPathFindEvent;

        private event UnityAction<YisoPlayerHUDTimerUI.Actions, object> OnTimerEvent; 
        public event UnityAction<YisoSelectionInputStates> OnDashInputEvent;

        private YisoPlayerHudPanelUI panelUI;

        private YisoPlayerInputsUI inputsUI;

        public void SetPanelUI(YisoPlayerHudPanelUI panelUI) {
            this.panelUI = panelUI;
        }

        public void SetInputsUI(YisoPlayerInputsUI inputsUI) {
            this.inputsUI = inputsUI;
        }

        public void RegisterOnJoystickMovement(UnityAction<Vector2> handler) {
            OnJoystickMovementEvent += handler;
        }

        public void UnregisterOnJoystickMovement(UnityAction<Vector2> handler) {
            OnJoystickMovementEvent -= handler;
        }

        public void RegisterOnJoystickInput(UnityAction<bool> handler) {
            OnJoystickInputEvent += handler;
        }

        public void UnregisterOnJoystickInput(UnityAction<bool> handler) {
            OnJoystickInputEvent -= handler;
        }

        public void RegisterAttackInput(UnityAction<YisoSelectionInputStates> handler) {
            OnAttackInputEvent += handler;
        }

        public void UnregisterAttackInput(UnityAction<YisoSelectionInputStates> handler) {
            OnAttackInputEvent -= handler;
        }

        public void RegisterDashInput(UnityAction<YisoSelectionInputStates> handler) {
            OnDashInputEvent += handler;
        }

        public void UnregisterDashInput(UnityAction<YisoSelectionInputStates> handler) {
            OnDashInputEvent -= handler;
        }
        
        
        public void RegisterReviveInput(UnityAction<YisoSelectionInputStates> handler) {
            OnReviveInputEvent += handler;
        }

        public void UnregisterReviveInput(UnityAction<YisoSelectionInputStates> handler) {
            OnReviveInputEvent -= handler;
        }

        public void RegisterOnVisibleHUD(UnityAction<bool> handler) {
            OnVisibleHUDEvent += handler;
        }

        public void UnregisterOnVisibleHUD(UnityAction<bool> handler) {
            OnVisibleHUDEvent -= handler;
        }

        public void RegisterOnQuestPathFind(UnityAction<UnityAction<int>> handler) {
            OnQuestPathFindEvent += handler;
        }

        public void UnregisterOnQuestPathFind(UnityAction<UnityAction<int>> handler) {
            OnQuestPathFindEvent -= handler;
        }

        public void RegisterOnTimer(UnityAction<YisoPlayerHUDTimerUI.Actions, object> handler) {
            OnTimerEvent += handler;
        }

        public void UnregisterOnTimer(UnityAction<YisoPlayerHUDTimerUI.Actions, object> handler) {
            OnTimerEvent -= handler;
        }

        public void RaiseTimer(YisoPlayerHUDTimerUI.Actions action, object data = null) {
            OnTimerEvent?.Invoke(action, data);
        }
        
        public void RaiseJoystickMovement(Vector2 movement) {
            OnJoystickMovementEvent?.Invoke(movement);
        }

        public void RaiseJoystickInput(bool input) {
            OnJoystickInputEvent?.Invoke(input);
        }

        public void RaiseAttackInput(YisoSelectionInputStates state) {
            OnAttackInputEvent?.Invoke(state);
        }

        public void RaiseReviveInput(YisoSelectionInputStates state) {
            OnReviveInputEvent?.Invoke(state);
        }

        public void SwitchToRevive(UnityAction<float, bool, bool> onRevive) {
            inputsUI.SwitchToRevive(onRevive);
        }

        public void SwitchToAttack() {
            inputsUI.SwitchToAttack();
        }

        public void RaiseDash(YisoSelectionInputStates state) {
            OnDashInputEvent?.Invoke(state);
        }

        public void ShowInteractButton(YisoHudUIInteractTypes type, UnityAction onClick) {
            panelUI.ShowInteractButton(type, onClick);
        }

        public void HideInteractButton(YisoHudUIInteractTypes type) {
            panelUI.HideInteractButton(type);
        }

        public void OnQuestPathFind(UnityAction<int> onClick) {
            OnQuestPathFindEvent?.Invoke(onClick);
        }

        public bool IsReady() => panelUI != null && inputsUI != null;
        public void OnDestroy() { }
        private YisoHUDUIService() { }
        
        internal static YisoHUDUIService CreateService() => new();
    }
}