using System;
using Core.Domain.Bounty;
using Core.Domain.Item;
using Core.Domain.Quest;
using Core.Domain.Types;
using Core.Service.Bounty;
using Core.Service.Data;
using Core.Service.Domain;
using UI.Popup;
using UI.Popup.Alert;
using UI.Popup.Bounty;
using UI.Popup.Cabinet;
using UI.Popup.Game.Dialogue;
using UI.Popup.Inventory;
using UI.Popup.Npc.Dialogue;
using UI.Popup.Quest;
using UnityEngine.Events;

namespace Core.Service.UI.Popup {
    public class YisoPopupUIService : IYisoPopupUIService {
        private YisoPopupCanvasUI canvasUI;

        private bool isActive = false;

        public void SetPopupCanvas(YisoPopupCanvasUI canvasUI) {
            this.canvasUI = canvasUI;
        }

        public void AlertS(string title, string content, UnityAction onClickOk, UnityAction onClickCancel = null, bool hideCancel = false) {
            var args = new YisoPopupAlertArgs {
                Title = title,
                Content = content,
                HideCancel = hideCancel
            };
            
            args.AddOkCb(() => {
                onClickOk?.Invoke();
                isActive = false;
                canvasUI.HideCanvas();
            });
            
            args.AddCancelCb(() => {
                onClickCancel?.Invoke();
                isActive = false;
                canvasUI.HideCanvas();
            });
            
            canvasUI.ShowCanvas(YisoPopupTypes.ALERT_S, args);
            isActive = true;
        }
        

        public void Alert(string title, string content, UnityAction onClickOk, UnityAction onClickCancel = null) {
            UnityAction newOnClickCancel = null;
            if (onClickCancel != null) newOnClickCancel = NewOnClickCancel;

            var dataPack = (title, content, (UnityAction)NewOnClickOk, newOnClickCancel);
            canvasUI.ShowCanvas(YisoPopupTypes.ALERT, dataPack);
            isActive = true;
            return;

            void NewOnClickOk() {
                onClickOk?.Invoke();
                isActive = false;
                canvasUI.HideCanvas();
            }

            void NewOnClickCancel() {
                onClickCancel?.Invoke();
                isActive = false;
                canvasUI.HideCanvas();
            }
        }

        public void ShowDropItemCount(YisoItem item, UnityAction<int> onClickOk, UnityAction onClickCancel = null) {
            var dataPack = (item, (UnityAction<int>)NewOnClickOk, (UnityAction)NewOnClickCancel);
            canvasUI.ShowCanvas(YisoPopupTypes.DROP_ITEM_COUNT, dataPack);
            isActive = true;
            return;

            void NewOnClickOk(int count) {
                onClickOk(count);
                isActive = false;
                canvasUI.HideCanvas();
            }

            void NewOnClickCancel() {
                onClickCancel?.Invoke();
                isActive = false;
                canvasUI.HideCanvas();
            }
        }

        public void ShowInventoryInput(YisoPopupInventoryInputArgs args) {
            args.OnClickOkList.Add(_ => {
                canvasUI.HideCanvas();
                isActive = false;
            });
            args.OnClickCloseList.Add(() => {
                canvasUI.HideCanvas();
                isActive = false;
            });
            canvasUI.ShowCanvas(YisoPopupTypes.INVENTORY_INPUT, args);
            isActive = true;
        }
        
        public void ShowQuestStartPopup(int questId, UnityAction onClickOk = null, UnityAction onClickCancel = null) {
            if (!TryGetQuest(questId, out var quest)) throw new Exception($"Quest({questId}) Not Found");
            var args = new YisoPopupQuestArgs(YisoPopupQuestContentUI.Types.START, quest);
            ShowQuestPopup(args, onClickOk, onClickCancel);
        }

        public void ShowQuestRestartPopup(int questId, UnityAction onClickOk = null, UnityAction onClickCancel = null) {
            if (!TryGetQuest(questId, out var quest)) throw new Exception($"Quest({questId}) Not Found");
            var args = new YisoPopupQuestArgs(YisoPopupQuestContentUI.Types.RE_START, quest);
            ShowQuestPopup(args, onClickOk, onClickCancel);
        }

        public void ShowQuestCompletePopup(int questId, int deathCount = 0 ,UnityAction onClickOk = null, UnityAction onClickCancel = null) {
            if (!TryGetQuest(questId, out var quest)) throw new Exception($"Quest({questId}) Not Found");
            var args = new YisoPopupQuestArgs(YisoPopupQuestContentUI.Types.COMPLETE, quest) {
                DeathCount = deathCount
            };
            ShowQuestPopup(args, onClickOk, onClickCancel);
        }

        private bool TryGetQuest(int questId, out YisoQuest quest) {
            return YisoServiceProvider.Instance.Get<IYisoDomainService>().TryGetQuest(questId, out quest);
        }

        private void ShowQuestPopup(YisoPopupQuestArgs args, UnityAction onClickOk = null,
            UnityAction onClickCancel = null) {
            args.AddOnClickOk(() => {
                onClickOk?.Invoke();
                canvasUI.HideCanvas();
                isActive = false;
            });
            
            args.AddOnClickCancel(() => {
                onClickCancel?.Invoke();
                canvasUI.HideCanvas();
                isActive = false;
            });
            
            isActive = true;
            canvasUI.ShowCanvas(YisoPopupTypes.QUEST, args);
        }

        public void ShowCabinetPopup(int cabinetId, UnityAction beforeOpenPopup = null, string okButtonText = "",
            string cancelButtonText = "", UnityAction onClickOk = null, UnityAction onClickClose = null) {
            beforeOpenPopup?.Invoke();
            ShowCabinetPopup(cabinetId, okButtonText, cancelButtonText, onClickOk, onClickClose);
        }

        public void ShowDevBountyPopup(YisoBountySO bountySO) {
            var args = new YisoPopupBountyData(bountySO.CreateBounty());
            args.AddOnClickOK(() => {
                canvasUI.HideCanvas();
                isActive = false;
            });
            isActive = true;
            canvasUI.ShowCanvas(YisoPopupTypes.BOUNTY, args);
        }

        public void ShowBountyPopup(UnityAction onClickOk = null) {
            var currentBounty = YisoServiceProvider.Instance.Get<IYisoBountyService>().GetCurrentBounty();
            var args = new YisoPopupBountyData(currentBounty);
            args.AddOnClickOK(() => {
                onClickOk?.Invoke();
                canvasUI.HideCanvas();
                isActive = false;
            });

            isActive = true;
            canvasUI.ShowCanvas(YisoPopupTypes.BOUNTY, args);
        }

        public void ShowBountyClearPopup(UnityAction onComplete = null) {
            isActive = true;
            canvasUI.ShowCanvas(YisoPopupTypes.BOUNTY_CLEAR, (UnityAction)CompleteAction);
            return;

            void CompleteAction() {
                onComplete?.Invoke();
                canvasUI.HideCanvas();
                isActive = false;
            }
        }

        public void ShowCabinetPopup(int cabinetId, string okButtonText = "", string cancelButtonText = "", UnityAction onClickOk = null, UnityAction onClickClose = null) {
            var domainService = YisoServiceProvider.Instance.Get<IYisoDomainService>();
            var cabinet = domainService.GetCabinetByIdElseThrow(cabinetId);
            var args = new YisoPopupCabinetEventArgs(cabinet) {
                OkButtonText = okButtonText,
                CancelButtonText = cancelButtonText
            };
            
            args.AddOkCb(() => {
                onClickOk?.Invoke();
                canvasUI.HideCanvas();
                isActive = false;
            });
            
            args.AddCancelCb(() => {
                onClickClose?.Invoke();
                canvasUI.HideCanvas();
                isActive = false;
            });

            isActive = true;
            canvasUI.ShowCanvas(YisoPopupTypes.CABINET, args);
        }

        public void ShowDialogue(int dialogueId, UnityAction onOpenPopup = null,
            UnityAction onCompleteDialogue = null) {
            var dialogue = YisoServiceProvider.Instance.Get<IYisoDomainService>().GetDialogueByIdElseThrow(dialogueId);
            onOpenPopup?.Invoke();

            var args = new YisoPopupNpcDialogueArgs(dialogue);
            args.AddOnClickClose(() => {
                onCompleteDialogue?.Invoke();
                canvasUI.HideCanvas();
                isActive = false;
            });

            isActive = true;
            canvasUI.ShowCanvas(YisoPopupTypes.NPC_DIALOGUE, args);
        }

        public void ShowDirection(int directionId, UnityAction onOpenPopup = null, UnityAction onComplete = null) {
            var direction = YisoServiceProvider.Instance.Get<IYisoDomainService>()
                .GetGameDirectionByIdOrElseThrow(directionId);
            var args = new YisoPopupGameDialogueArgs(direction);
            args.AddOnClick(() => {
                onComplete?.Invoke();
                canvasUI.HideCanvas();
                isActive = false;
            });

            isActive = true;
            onOpenPopup?.Invoke();
            canvasUI.ShowCanvas(YisoPopupTypes.GAME_DIRECTION, args);
        }

        public void ShowWelcome() {
            isActive = true;
            canvasUI.ShowCanvas(YisoPopupTypes.WELCOME, (UnityAction)ClickOk);
            return;

            void ClickOk() {
                canvasUI.HideCanvas();
                isActive = false;
            }
        }

        public bool IsReady() => canvasUI != null;
        public bool IsActive() => isActive;

        private YisoPopupUIService() {
            YisoServiceProvider.Instance.Get<IYisoUIService>().AddSubUIService(this);
        }
        public void OnDestroy() { }
        internal static YisoPopupUIService CreateService() => new();
    }
}