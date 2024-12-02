using System;
using BaseCamp.UI.HUD;
using Core.Domain.Types;
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
    public interface IYisoUIService : IYisoService {
        public void SetGlobalCanvasUI(YisoGlobalCanvasUI globalCanvasUI);
        void SetHUDUI(YisoPlayerHudPanelUI hudUI);
        void SetGameUI(YisoGameCanvasUI gameUI);
        void SetMenuUI(YisoMenuCanvasUI menuUI);
        public void SetBaseCampHUDUI(YisoPlayerBaseCampHUDCanvasUI baseCampHudUI);
        public void SetInteractUI(YisoInteractCanvasUI interactUI);
        public void SetPopupCanvasUI(YisoPopupCanvasUI popupCanvasUI);
        public void SetPopup2CanvasUI(YisoPopup2CanvasUI popup2CanvasUI);
        public void ShowGameUI(YisoGameUITypes type, object data = null);
        public void ShowInteractUI(YisoInteractTypes type, object data = null);
        public void ShowMenuUI(YisoMenuTypes type, object data = null);
        public void ShowHUDUI();
        void ActiveOnlyHudUI(bool flag);
        public void ActiveOnlyGameUI(bool flag, YisoGameUITypes type, object data = null);
        public void ActiveInteractButtonOnBaseCamp(bool flag, Action onClick = null);
        public bool IsUIShowed();
        public void AddSubUIService(IYisoSubUIService subUIService);

        public void TestShowHUD();
    }
}