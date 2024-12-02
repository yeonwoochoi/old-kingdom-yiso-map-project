using Core.Domain.Types;
using Core.Domain.Wanted;
using Core.Service;
using Core.Service.Data;
using Core.Service.Domain;
using Core.Service.UI.Popup;
using UI.Interact.Base;
using UI.Interact.Wanted.Event;
using UI.Interact.Wanted.Item;
using UnityEngine;

namespace UI.Interact.Wanted {
    public class YisoInteractWantedContentUI : YisoInteractBasePanelUI {
        [SerializeField] private YisoInteractWantedItemsUI itemsUI;
        [SerializeField] private YisoInteractWantedDescriptionUI descriptionUI;

        protected override void Start() {
            base.Start();
            SetItems();
            descriptionUI.Clear();
        }
        protected override void RegisterEvents() {
            itemsUI.OnWantedUIEvent += OnWantedUIEvent;
            descriptionUI.OnClickButtonEvent += OnClickButton;
        }

        protected override void UnregisterEvents() {
            itemsUI.OnWantedUIEvent -= OnWantedUIEvent;
            descriptionUI.OnClickButtonEvent -= OnClickButton;
        }

        private void OnWantedUIEvent(WantedUIEventArgs args) {
            switch (args) {
                case WantedUISelectedEventArgs selectArgs:
                    descriptionUI.SetItem(selectArgs.Wanted);
                    break;
                case WantedUIUnSelectedEventArgs:
                    descriptionUI.Clear();
                    break;
            }
        }

        private void OnClickButton(YisoWanted wanted) {
            var popupService = YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
            popupService.Alert("현상금 사냥", $"정말 {wanted.GetTargetName(CurrentLocale)}를 잡으러 가시겠습니까?", () => {
                popupService.Alert("알림", "현상금 사냥은 아직 개발중에 있습니다.", () => { });
            }, () => { });
        }

        private void SetItems() {
            var domainService = YisoServiceProvider.Instance.Get<IYisoDomainService>();
            var wantedList = domainService.GetWantedList();
            foreach (var wanted in wantedList) itemsUI.SetItem(wanted);
        }


        public override void ClearPanel() {
            itemsUI.UnsetItem();
            descriptionUI.Clear();
        }

        public override YisoInteractTypes GetType() => YisoInteractTypes.BOUNTY;
    }
}