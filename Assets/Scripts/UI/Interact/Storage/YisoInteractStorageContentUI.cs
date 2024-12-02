using Core.Domain.Actor.Player.Modules.Inventory;
using Core.Domain.Item;
using Core.Domain.Types;
using Core.Service;
using Core.Service.UI.Popup;
using Sirenix.OdinInspector;
using UI.Common.Inventory.Potential;
using UI.Interact.Base;
using UI.Interact.Storage.Event;
using UI.Popup.Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Interact.Storage {
    public class YisoInteractStorageContentUI : YisoInteractBasePanelUI {
        [SerializeField, Title("Tabs")] private Toggle[] tabs;
        [SerializeField, Title("Holder")] private YisoInteractStorageItemHoldersUI holdersUI;
        [SerializeField] private YisoInteractStorageDescriptionUI descriptionUI;
        
        
        private YisoItem.InventoryType currentType = YisoItem.InventoryType.EQUIP;

        protected override void Awake() {
            base.Awake();
            for (var i = 0; i < tabs.Length; i++)
                tabs[i].onValueChanged.AddListener(OnToggleTab(i));
        }
        
        protected override void RegisterEvents() {
            holdersUI.OnStorageUIEvent += OnStorageUIEvent;
            player.InventoryModule.OnInventoryEvent += OnInventoryEvent;
            player.StorageModule.OnStorageEvent += OnStorageEvent;
            descriptionUI.OnButtonActionRaised += OnButtonAction;
        }

        protected override void UnregisterEvents() {
            holdersUI.OnStorageUIEvent -= OnStorageUIEvent;
            player.InventoryModule.OnInventoryEvent -= OnInventoryEvent;
            player.StorageModule.OnStorageEvent -= OnStorageEvent;
            descriptionUI.OnButtonActionRaised -= OnButtonAction;
        }

        public override void ClearPanel() {
            tabs[0].isOn = true;
            holdersUI.Clear(YisoInteractStorageItemHoldersUI.Types.INVENTORY);
            holdersUI.Clear(YisoInteractStorageItemHoldersUI.Types.STORAGE);
        }

        public override void OnVisible() {
            SetItems();
        }

        public override YisoInteractTypes GetType() => YisoInteractTypes.STORAGE;

        private void OnButtonAction(YisoInteractStorageItemHoldersUI.Types type, YisoItem item) {
            var popupService = YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
            var args = new YisoPopupInventoryInputArgs {
                Item = item
            };
            
            switch (type) {
                case YisoInteractStorageItemHoldersUI.Types.STORAGE:
                    args.Type = YisoPopupInventoryInputContentUI.Types.STORAGE_PULL;
                    args.OnClickOkList.Add(count => {
                        player.StorageModule.PullItem(item, count);
                    });
                    break;
                case YisoInteractStorageItemHoldersUI.Types.INVENTORY:
                    args.Type = YisoPopupInventoryInputContentUI.Types.STORAGE_PUSH;
                    args.OnClickOkList.Add(count => {
                        player.InventoryModule.StoreItem(item, count);
                    });
                    break;
            }
            
            popupService.ShowInventoryInput(args);
        }
        
        private void OnStorageUIEvent(StorageUIEventArgs args) {
            var type = args.Type;
            switch (args) {
                case StorageUIItemSelectedEventArgs selectedEventArgs:
                    descriptionUI.SetItem(type, selectedEventArgs.Item);
                    break;
                case StorageUIItemUnSelectedEventArgs:
                    descriptionUI.Clear();
                    break;
            }
        }

        private void OnInventoryEvent(YisoPlayerInventoryEventArgs args) {
            switch (args) {
                case YisoPlayerInventoryAddEventArgs addArgs:
                    var position = addArgs.Position;
                    holdersUI.SetItem(YisoInteractStorageItemHoldersUI.Types.INVENTORY,
                        position,
                        player.InventoryModule.GetItem<YisoItem>(currentType, position)
                        );
                    break;
                case YisoPlayerInventoryRemoveEventArgs removeArgs:
                    holdersUI.Clear(YisoInteractStorageItemHoldersUI.Types.INVENTORY, removeArgs.Position);
                    break;
                case YisoPlayerInventoryCountEventArgs countArgs:
                    holdersUI.UpdateCount(YisoInteractStorageItemHoldersUI.Types.INVENTORY, countArgs.Position, countArgs.AfterCount);
                    break;
            }
        }

        private void OnStorageEvent(YisoPlayerInventoryEventArgs args) {
            switch (args) {
                case YisoPlayerInventoryAddEventArgs addArgs:
                    var position = addArgs.Position;
                    holdersUI.SetItem(YisoInteractStorageItemHoldersUI.Types.STORAGE,
                        position,
                        player.StorageModule.GetItem<YisoItem>(currentType, position)
                    );
                    break;
                case YisoPlayerInventoryRemoveEventArgs removeArgs:
                    holdersUI.Clear(YisoInteractStorageItemHoldersUI.Types.STORAGE, removeArgs.Position);
                    break;
                case YisoPlayerInventoryCountEventArgs countArgs:
                    holdersUI.UpdateCount(YisoInteractStorageItemHoldersUI.Types.STORAGE, countArgs.Position, countArgs.AfterCount);
                    break;
            }
        }
        
        private UnityAction<bool> OnToggleTab(int index) => flag => {
            if (!flag) {
                holdersUI.Clear(YisoInteractStorageItemHoldersUI.Types.INVENTORY);
                holdersUI.Clear(YisoInteractStorageItemHoldersUI.Types.STORAGE);
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
            foreach (var (position, item) in units.ItemDict) {
                holdersUI.SetItem(YisoInteractStorageItemHoldersUI.Types.INVENTORY, position, item);
            }

            foreach (var (position, item) in player.StorageModule.StorageUnits[currentType].ItemDict) {
                holdersUI.SetItem(YisoInteractStorageItemHoldersUI.Types.STORAGE, position, item);
            }
        }
    }
}