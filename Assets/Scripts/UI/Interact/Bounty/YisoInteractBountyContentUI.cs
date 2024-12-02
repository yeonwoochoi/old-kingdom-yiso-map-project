using Core.Domain.Bounty;
using Core.Domain.Types;
using Core.Domain.Wanted;
using Core.Logger;
using Core.Service;
using Core.Service.Bounty;
using Core.Service.Scene;
using Core.Service.UI.Popup;
using UI.Interact.Base;
using UI.Interact.Bounty.Events;
using UI.Interact.Bounty.Items;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Interact.Bounty {
    public class YisoInteractBountyContentUI : YisoInteractBasePanelUI {
        [SerializeField] private YisoInteractBountyItemsUI itemsUI;
        [SerializeField] private YisoInteractBountyDescriptionUI descriptionUI;
        [SerializeField] private UnityEvent onClickCloseEvent;

        private IYisoBountyService bountyService;
        private IYisoPopupUIService popupService;
        private IYisoSceneService sceneService;

        protected override void Awake() {
            base.Awake();
            bountyService = YisoServiceProvider.Instance.Get<IYisoBountyService>();
            popupService = YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
            sceneService = YisoServiceProvider.Instance.Get<IYisoSceneService>();
        }

        protected override void Start() {
            base.Start();
            descriptionUI.Clear();
        }

        public override void OnVisible() {
            SetItems();
        }
        

        protected override void RegisterEvents() {
            itemsUI.OnBountyEvent += OnBountyUIEvent;
            descriptionUI.OnClickButtonEvent += OnClickButton;
        }

        protected override void UnregisterEvents() {
            itemsUI.OnBountyEvent -= OnBountyUIEvent;
            descriptionUI.OnClickButtonEvent -= OnClickButton;
        }
        
        private void OnBountyUIEvent(BountyUIEventArgs args) {
            switch (args) {
                case BountyUISelectedEventArgs selectArgs:
                    descriptionUI.SetItem(selectArgs.Bounty, selectArgs.Status);
                    break;
                case BountyUIUnSelectedEventArgs:
                    descriptionUI.Clear();
                    break;
            }
        }
        
        private void OnClickButton(YisoBounty bounty) {
            popupService.AlertS("현상금 사냥", $"정말 {bounty.GetTargetName(CurrentLocale)}를 잡으러 가시겠습니까?", () => {
                bountyService.ReadyBounty(bounty.Id);
                sceneService.LoadScene(YisoSceneTypes.BOUNTY, onFade: flag => {
                    if (!flag) return;
                    onClickCloseEvent?.Invoke();
                });
            }, () => { });
        }
        
        private void SetItems() {
            var idleBounties = bountyService.GetBountiesByStatus(YisoBountyStatus.IDLE);
            foreach (var bounty in idleBounties) itemsUI.SetItem(bounty, YisoBountyStatus.IDLE);
            var completeBounties = bountyService.GetBountiesByStatus(YisoBountyStatus.COMPLETE);
            foreach (var bounty in completeBounties) itemsUI.SetItem(bounty, YisoBountyStatus.COMPLETE);
        }
        
        public override void ClearPanel() {
            itemsUI.UnsetItem();
            itemsUI.Clear();
            descriptionUI.Clear();
        }

        public override YisoInteractTypes GetType() => YisoInteractTypes.BOUNTY;
    }
}