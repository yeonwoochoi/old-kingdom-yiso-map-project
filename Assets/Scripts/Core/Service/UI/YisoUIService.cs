using System;
using System.Collections.Generic;
using System.Linq;
using BaseCamp.UI.HUD;
using Core.Domain.Types;
using Core.Service.Scene;
using Core.Service.UI.Event;
using UI.Game;
using UI.Global;
using UI.HUD;
using UI.Interact;
using UI.Menu;
using UI.Popup;
using UI.Popup2;
using UnityEngine.Events;

namespace Core.Service.UI {
    public class YisoUIService : IYisoUIService {
        private YisoPlayerHudPanelUI hudUI;
        private YisoPlayerBaseCampHUDCanvasUI baseCampHudUI;
        private YisoGameCanvasUI gameUI;
        private YisoMenuCanvasUI menuUI;
        private YisoInteractCanvasUI interactUI;
        private YisoGlobalCanvasUI globalCanvasUI;
        private YisoPopupCanvasUI popupCanvasUI;
        private YisoPopup2CanvasUI popup2CanvasUI;

        private readonly List<IYisoSubUIService> subUIServices = new();

        private readonly IYisoSceneService sceneService = YisoServiceProvider.Instance.Get<IYisoSceneService>();

        private ActiveUITypes currentActiveUI;

        public bool IsReady() => hudUI != null &&
                                 // baseCampHudUI != null &&
                                 gameUI != null &&
                                 menuUI != null &&
                                 interactUI != null &&
                                 globalCanvasUI != null &&
                                 popupCanvasUI != null &&
                                 popup2CanvasUI != null;

        public void AddSubUIService(IYisoSubUIService subUIService) {
            subUIServices.Add(subUIService);
        }
        
        private void OnSceneChanged(YisoSceneTypes beforeSceneType, YisoSceneTypes afterSceneType) {
            currentActiveUI = afterSceneType.IsCombatScene() ? ActiveUITypes.HUD_GAME : ActiveUITypes.HUD_BASE_CAMP;
            if (hudUI != null) hudUI.GameMode(true);
            // if (baseCampHudUI != null) baseCampHudUI.BaseCampMode(afterSceneType == YisoSceneTypes.BASE_CAMP);
        }

        public void SetPopupCanvasUI(YisoPopupCanvasUI popupCanvasUI) {
            this.popupCanvasUI = popupCanvasUI;
        }

        public void SetPopup2CanvasUI(YisoPopup2CanvasUI popup2CanvasUI) {
            this.popup2CanvasUI = popup2CanvasUI;
        }

        public void SetGlobalCanvasUI(YisoGlobalCanvasUI globalCanvasUI) {
            this.globalCanvasUI = globalCanvasUI;
        }
        
        public void SetBaseCampHUDUI(YisoPlayerBaseCampHUDCanvasUI baseCampHudUI) {
            // this.baseCampHudUI = baseCampHudUI;
        }

        public void SetInteractUI(YisoInteractCanvasUI interactUI) {
            this.interactUI = interactUI;
        }

        public void SetHUDUI(YisoPlayerHudPanelUI hudUI) {
            this.hudUI = hudUI;
        }

        public void SetGameUI(YisoGameCanvasUI gameUI) {
            this.gameUI = gameUI;
        }

        public void SetMenuUI(YisoMenuCanvasUI menuUI) {
            this.menuUI = menuUI;
        }

        public bool IsUIShowed() {
            return gameUI.IsUIActive ||
                   menuUI.IsUIActive ||
                   interactUI.IsUIActive ||
                   globalCanvasUI.IsUIActive ||
                   popupCanvasUI.IsUIActive ||
                   popup2CanvasUI.IsUIActive;
        }

        public void TestShowHUD() {
            // baseCampHudUI.Visible(false);
            hudUI.GameMode(true);
        }

        private void InnerVisibleHudUI(bool flag) {
            hudUI.GameMode(flag);
            /*if (sceneService.GetCurrentScene().IsCombatScene()) hudUI.Visible(flag);
            else baseCampHudUI.Visible(flag);*/
        }

        public void ShowGameUI(YisoGameUITypes type, object data = null) {
            currentActiveUI = ActiveUITypes.GAME;
            InnerVisibleHudUI(false);
            gameUI.ShowCanvas(type, data);
        }

        public void ActiveOnlyGameUI(bool flag, YisoGameUITypes type, object data = null) {
            if (flag && type != YisoGameUITypes.FLOATING_TEXT)
                currentActiveUI = ActiveUITypes.GAME;
            if (flag)
                gameUI.ShowCanvas(type, data);
            else
                gameUI.HideCanvas();
        }

        public void ShowMenuUI(YisoMenuTypes type, object data = null) {
            currentActiveUI = ActiveUITypes.MENU;
            InnerVisibleHudUI(false);
            menuUI.ShowCanvas(type, data);
        }

        public void ShowInteractUI(YisoInteractTypes type, object data = null) {
            currentActiveUI = ActiveUITypes.INTERACT;
            InnerVisibleHudUI(false);
            interactUI.ShowCanvas(type, data);
        }

        public void ShowHUDUI() {
            HideActiveUI(currentActiveUI);
            InnerVisibleHudUI(true);
            currentActiveUI = ActiveUITypes.HUD_GAME;
        }

        public void ActiveOnlyHudUI(bool flag) {
            hudUI.Visible(flag);
        }

        public void ActiveInteractButtonOnBaseCamp(bool flag, Action onClick = null) {
            if (sceneService.GetCurrentScene().IsCombatScene()) return;
            baseCampHudUI.ActiveInteract(flag, onClick);
        }

        private void HideActiveUI(ActiveUITypes type) {
            switch (type) {
                case ActiveUITypes.MENU:
                    menuUI.HideCanvas();
                    break;
                case ActiveUITypes.INTERACT:
                    interactUI.HideCanvas();
                    break;
                case ActiveUITypes.GAME:
                    gameUI.HideCanvas();
                    break;
            }
        }
        
        private YisoUIService() {
            sceneService.RegisterOnSceneChanged(OnSceneChanged);
        }
        
        internal static YisoUIService CreateService() => new();
        public void OnDestroy() { }
        public enum ActiveUITypes {
            HUD_GAME, HUD_BASE_CAMP, MENU, INTERACT, GAME
        }
    }
}