using System;
using Core.Behaviour;
using Core.Domain.Actor.Player.Modules.Inventory;
using Core.Domain.Item;
using Core.Domain.Types;
using Core.Service;
using Core.Service.UI.Popup;
using Core.Service.UI.Popup2;
using Sirenix.OdinInspector;
using UI.Menu.Base;
using UI.Menu.Inventory.Event;
using UI.Menu.Inventory.Holder;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Inventory {
    public class YisoMenuInventoryContentUI : YisoMenuBasePanelUI {
        [SerializeField, Title("Tabs")] private Toggle[] tabs;
        [SerializeField, Title("Holder")] private YisoMenuInventoryItemHoldersUI holderUI;
        [SerializeField] private YisoMenuInventoryDescriptionUI descriptionUI;

        private YisoItem.InventoryType currentType = YisoItem.InventoryType.EQUIP;

        public event UnityAction<YisoItem> OnQuickSlotOverlayRaised; 

        protected override void Awake() {
            base.Awake();
            for (var i = 0; i < tabs.Length; i++) {
                tabs[i].onValueChanged.AddListener(OnToggleTab(i));
            }
        }

        protected override void RegisterEvents() {
            base.RegisterEvents();
            player.InventoryModule.OnInventoryEvent += OnInventoryEvent;
            holderUI.OnEventRaised += OnInventoryUIEvent;
            descriptionUI.OnButtonActionRaised += OnButtonAction;
        }

        protected override void UnregisterEvents() {
            base.UnregisterEvents();
            player.InventoryModule.OnInventoryEvent -= OnInventoryEvent;
            holderUI.OnEventRaised -= OnInventoryUIEvent;
            descriptionUI.OnButtonActionRaised -= OnButtonAction;
        }
        
        private void OnInventoryUIEvent(InventoryUIEventArgs args) {
            switch (args) {
                case InventoryUIItemSelectedEventArgs itemSelectedArgs:
                    descriptionUI.SetItem(itemSelectedArgs.Item);
                    break;
                case InventoryUIItemUnSelectedEventArgs itemUnSelectedArgs:
                    descriptionUI.Clear();
                    break;
            }
        }

        private void OnButtonAction(YisoMenuInventoryButtonUI.Types type, YisoItem item) {
            var popupService = YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
            switch (type) {
                case YisoMenuInventoryButtonUI.Types.DROP:
                    if (item is YisoEquipItem) {
                        player.InventoryModule.DropItem(item);
                    } else {
                        popupService.ShowDropItemCount(item, value => {
                            player.InventoryModule.DropItem(item, value);
                        });
                    }
                    break;
                case YisoMenuInventoryButtonUI.Types.USE:
                    /*if (!player.UIModule.TryGetSlotPosition(item.Id, out var position)) {
                        position = -1;
                    }
                    player.InventoryModule.UseItem(item as YisoUseItem, position);*/
                    break;
                case YisoMenuInventoryButtonUI.Types.EQUIP:
                    player.InventoryModule.EquipItem(item as YisoEquipItem);
                    break;
                case YisoMenuInventoryButtonUI.Types.UN_EQUIP:
                    player.InventoryModule.UnEquipItem(item as YisoEquipItem);
                    break;
                case YisoMenuInventoryButtonUI.Types.QUICK:
                    OnQuickSlotOverlayRaised?.Invoke(item);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void OnInventoryEvent(YisoPlayerInventoryEventArgs args) {
            /*YisoEquipItem unEquipItem = null;
            switch (args) {
                case YisoPlayerInventoryAddEventArgs addArgs:
                    var addIndex = addArgs.Position;
                    var item = player.InventoryModule.InventoryUnits[currentType][addIndex];
                    holderUI.SetItem(addIndex, item);
                    break;
                case YisoPlayerInventoryRemoveEventArgs removeArgs:
                    holderUI.Clear(removeArgs.Position);
                    break;
                case YisoPlayerInventoryCountEventArgs countArgs:
                    var countIndex = countArgs.Position;
                    holderUI.UpdateCount(countIndex, countArgs.AfterCount);
                    break;
                case YisoPlayerInventoryEquipEventArgs equipArgs:
                    var equipItem =
                        player.InventoryModule.GetItem<YisoEquipItem>(YisoItem.InventoryType.EQUIP, equipArgs.Position);
                    EquipUnEquipItem(equipItem, true);
                    if (!equipArgs.UnEquipped) return;
                    unEquipItem = player.InventoryModule.GetItem<YisoEquipItem>(YisoItem.InventoryType.EQUIP, equipArgs.UnEquipPosition);
                    EquipUnEquipItem(unEquipItem, false);
                    break;
                case YisoPlayerInventoryUnEquipEventArgs unEquipArgs:
                    unEquipItem =
                        player.InventoryModule.GetItem<YisoEquipItem>(YisoItem.InventoryType.EQUIP,
                            unEquipArgs.Position);
                    EquipUnEquipItem(unEquipItem, false);
                    break;
            }

            unEquipItem = null;*/
        }

        private void EquipUnEquipItem(YisoEquipItem item, bool equip) {
            holderUI.UpdateEquip(item.Position, equip);
            descriptionUI.UpdateItem(item);
        }

        private UnityAction<bool> OnToggleTab(int index) => flag => {
            if (!flag) {
                holderUI.Clear();
                descriptionUI.Clear();
                return;
            }
            var type = ToInvType(index);
            currentType = type;
            SetItems();
        };
        
        private YisoItem.InventoryType ToInvType(int index) => index switch {
            0 => YisoItem.InventoryType.EQUIP,
            1 => YisoItem.InventoryType.USE,
            _ => YisoItem.InventoryType.ETC
        };

        private void SetItems() {
            var units = player.InventoryModule.InventoryUnits[currentType];
            foreach (var pair in units.ItemDict) {
                var slot = pair.Key;
                var item = pair.Value;
                holderUI.SetItem(slot, item); 
            }
        }

        public override void Init() {
            tabs[1].isOn = true;
            tabs[0].isOn = true;
        }

        public override YisoMenuTypes GetMenuType() => YisoMenuTypes.INVENTORY;

        public override void ClearPanel() {
            tabs[0].isOn = true;
        }
    }
}