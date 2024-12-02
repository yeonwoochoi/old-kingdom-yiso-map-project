using Core.Domain.Actor.Player.Modules.Inventory;
using Core.Domain.Actor.Player.Modules.Inventory.Reinforce;
using Core.Domain.Item;
using Core.Domain.Types;
using TMPro;
using UI.Interact.Base;
using UI.Interact.Blacksmith.Description;
using UI.Interact.Blacksmith.Event;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Blacksmith {
    public class YisoInteractBlacksmithContentUI : YisoInteractBasePanelUI {
        [SerializeField] private Toggle[] tabs;
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private YisoInteractBlacksmithItemHoldersUI itemHoldersUI;
        [SerializeField] private YisoInteractBlacksmithDescriptionUI descriptionUI;

        private YisoInteractBlacksmithBaseDescriptionUI.Types currentType =
            YisoInteractBlacksmithBaseDescriptionUI.Types.NORMAL;

        private int currentTabIndex = 0;

        protected override void Start() {
            base.Start();
            for (var i = 0; i < tabs.Length; i++) 
                tabs[i].onValueChanged.AddListener(OnClickTab(i));
            descriptionUI.Clear();
        }

        public override void OnVisible() {
            OnMoneyChanged(player.InventoryModule.Money);
            SetItems();
        }

        public override void ClearPanel() {
            tabs[0].isOn = true;
            itemHoldersUI.Clear();
        }

        protected override void OnEnable() {
            base.OnEnable();
            itemHoldersUI.OnEventRaised += OnBlacksmithUIEvent;
            descriptionUI.OnReinforceResultEvent += OnReinforceResult;
            player.InventoryModule.OnMoneyChanged += OnMoneyChanged;
        }

        protected override void OnDisable() {
            base.OnDisable();
            itemHoldersUI.OnEventRaised -= OnBlacksmithUIEvent;
            descriptionUI.OnReinforceResultEvent -= OnReinforceResult;
            player.InventoryModule.OnMoneyChanged -= OnMoneyChanged;
        }

        private void OnBlacksmithUIEvent(BlacksmithUIEventArgs args) {
            switch (args) {
                case BlacksmithUIItemSelectedEventArgs itemSelectedArgs:
                    descriptionUI.SetItem(itemSelectedArgs.Item);
                    break;
                case BlacksmithUIItemUnSelectedEventArgs:
                    descriptionUI.Clear();
                    break;
            }
        }

        private void OnMoneyChanged(double money) {
            moneyText.SetText(money.ToCommaString());
        }

        private void OnReinforceResult(YisoEquipItem item, YisoPlayerInventoryReinforceResult result) {
            player.InventoryModule.Reinforce(result, item);
            descriptionUI.SetItem(item);
            itemHoldersUI.UpdateItem(item);
        }

        public override YisoInteractTypes GetType() => YisoInteractTypes.BLACKSMITH;

        private void SetItems() {
            var units = player.InventoryModule.InventoryUnits[YisoItem.InventoryType.EQUIP];
            foreach (var (position, item) in units.ItemDict) {
                itemHoldersUI.SetItem(position, item);
            }
        }

        private UnityAction<bool> OnClickTab(int index) => flag => {
            if (!flag || index == currentTabIndex) return;
            currentType = index.ToEnum<YisoInteractBlacksmithBaseDescriptionUI.Types>();
            itemHoldersUI.UnSelect();
            descriptionUI.SetDescription(currentType);
            currentTabIndex = index;
        };
    }
}