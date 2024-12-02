using Core.Domain.Types;
using Core.Service.UI.Event;
using UI.HUD;
using UI.HUD.Interact;
using UI.HUD.Timer;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Service.UI.HUD {
    public interface IYisoHUDUIService : IYisoService {
        void SetPanelUI(YisoPlayerHudPanelUI panelUI);
        public void SetInputsUI(YisoPlayerInputsUI inputsUI);
        void RegisterOnJoystickMovement(UnityAction<Vector2> handler);
        void UnregisterOnJoystickMovement(UnityAction<Vector2> handler);
        void RegisterOnJoystickInput(UnityAction<bool> handler);
        void UnregisterOnJoystickInput(UnityAction<bool> handler);
        void RegisterAttackInput(UnityAction<YisoSelectionInputStates> handler);
        void UnregisterAttackInput(UnityAction<YisoSelectionInputStates> handler);
        public void RegisterDashInput(UnityAction<YisoSelectionInputStates> handler);
        public void UnregisterDashInput(UnityAction<YisoSelectionInputStates> handler);
        public void RegisterReviveInput(UnityAction<YisoSelectionInputStates> handler);
        public void UnregisterReviveInput(UnityAction<YisoSelectionInputStates> handler);
        public void RegisterOnQuestPathFind(UnityAction<UnityAction<int>> handler);
        public void UnregisterOnQuestPathFind(UnityAction<UnityAction<int>> handler);
        void RegisterOnVisibleHUD(UnityAction<bool> handler);
        void UnregisterOnVisibleHUD(UnityAction<bool> handler);
        public void RegisterOnTimer(UnityAction<YisoPlayerHUDTimerUI.Actions, object> handler);
        public void UnregisterOnTimer(UnityAction<YisoPlayerHUDTimerUI.Actions, object> handler);
        public void RaiseTimer(YisoPlayerHUDTimerUI.Actions action, object data = null);
        void RaiseJoystickMovement(Vector2 movement);
        void RaiseJoystickInput(bool input);
        void RaiseAttackInput(YisoSelectionInputStates state);
        void RaiseDash(YisoSelectionInputStates state);
        public void RaiseReviveInput(YisoSelectionInputStates state);
        public void ShowInteractButton(YisoHudUIInteractTypes type, UnityAction onClick);
        public void HideInteractButton(YisoHudUIInteractTypes type);
        public void SwitchToRevive(UnityAction<float, bool, bool> onRevive);
        public void SwitchToAttack();
        public void OnQuestPathFind(UnityAction<int> onClick);
    }
}