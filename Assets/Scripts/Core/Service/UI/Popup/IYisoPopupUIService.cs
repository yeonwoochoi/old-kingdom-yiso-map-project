using Core.Domain.Bounty;
using Core.Domain.Item;
using Core.Domain.Quest;
using UI.Popup;
using UI.Popup.Inventory;
using UnityEngine.Events;

namespace Core.Service.UI.Popup {
    public interface IYisoPopupUIService : IYisoSubUIService {
        public void SetPopupCanvas(YisoPopupCanvasUI canvasUI);
        public void AlertS(string title, string content, UnityAction onClickOk, UnityAction onClickCancel = null, bool hideCancel = false);
        public void Alert(string title, string content, UnityAction onClickOk, UnityAction onClickCancel = null);
        public void ShowDropItemCount(YisoItem item, UnityAction<int> onClickOk, UnityAction onClickCancel = null);
        public void ShowInventoryInput(YisoPopupInventoryInputArgs args);
        public void ShowQuestStartPopup(int questId, UnityAction onClickOk = null,
            UnityAction onClickCancel = null);
        public void ShowQuestRestartPopup(int questId, UnityAction onClickOk = null,
            UnityAction onClickCancel = null);
        public void ShowQuestCompletePopup(int questId, int deathCount = 0, UnityAction onClickOk = null,
            UnityAction onClickCancel = null);

        public void ShowCabinetPopup(int cabinetId, string okButtonText = "", string cancelButtonText = "",
            UnityAction onClickOk = null, UnityAction onClickClose = null);

        public void ShowCabinetPopup(int cabinetId, UnityAction beforeOpenPopup = null, string okButtonText = "",
            string cancelButtonText = "", UnityAction onClickOk = null, UnityAction onClickClose = null);

        public void ShowDirection(int directionId, UnityAction onOpenPopup = null, UnityAction onComplete = null);
        
        public void ShowDialogue(int dialogueId, UnityAction onOpenPopup = null,
            UnityAction onCompleteDialogue = null);

        public void ShowDevBountyPopup(YisoBountySO bountySO);

        public void ShowBountyPopup(UnityAction onClickOk = null);
        public void ShowBountyClearPopup(UnityAction onComplete = null);

        public void ShowWelcome();
    }
}