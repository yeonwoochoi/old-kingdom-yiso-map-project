using System;
using System.Collections.Generic;
using Core.Domain.Actor.Player.Modules.Inventory;
using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Domain.Store;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Data;
using Core.Service.Data.Item;
using Core.Service.UI.Game;
using Core.Service.UI.Popup;
using Sirenix.OdinInspector;
using TMPro;
using UI.Interact.Base;
using UI.Interact.Store.Description;
using UI.Interact.Store.Event;
using UI.Interact.Store.Holder;
using UI.Interact.Store.Selection;
using UI.Interact.Store.V2.Description;
using UI.Popup.Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Store {
    public class YisoInteractStoreContentUI : YisoInteractBasePanelUI {
        [SerializeField, Title("Tabs")] private Toggle[] tabs;
        [SerializeField, Title("Holders")] private YisoInteractStoreItemHoldersV2UI holdersUI;
        [SerializeField] private YisoInteractStoreV2DescriptionUI descriptionUI;
        [SerializeField] private YisoInteractStoreSelectionUI selectionUI;
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private Button selectionButton;
        [SerializeField] private TextMeshProUGUI selectionButtonText;
        [SerializeField] private CanvasGroup storeBlockerCanvas;

        private YisoItem.InventoryType currentType = YisoItem.InventoryType.EQUIP;
        private YisoStore currentStore;
        private bool selectionMode = false;
        private bool clearMode = false;

        protected override void Awake() {
            base.Awake();
            for (var i = 0; i < tabs.Length; i++) 
                tabs[i].onValueChanged.AddListener(OnToggleTab(i));
        }

        protected override void Start() {
            base.Start();
            selectionButton.onClick.AddListener(() => {
                ActiveSelectionMode(!selectionMode);
            });
        }

        public override void ClearPanel() {
            clearMode = true;
            holdersUI.Clear(Types.INVENTORY);
            holdersUI.Clear(Types.STORE);
            descriptionUI.Clear();
            selectionUI.Clear();

            if (selectionMode) ActiveSelectionMode(false);
            clearMode = false;
        }

        protected override void RegisterEvents() {
            holdersUI.OnStoreUIEvent += OnStoreUIEvent;
            player.InventoryModule.OnInventoryEvent += OnInventoryEvent;
            player.InventoryModule.OnMoneyChanged += OnMoneyChanged;
            descriptionUI.OnButtonActionEvent += OnButtonAction;
            selectionUI.OnSellEvent += OnClickSells;
        }

        protected override void UnregisterEvents() {
            holdersUI.OnStoreUIEvent -= OnStoreUIEvent;
            player.InventoryModule.OnInventoryEvent -= OnInventoryEvent;
            player.InventoryModule.OnMoneyChanged -= OnMoneyChanged;
            descriptionUI.OnButtonActionEvent -= OnButtonAction;
            selectionUI.OnSellEvent -= OnClickSells;
        }
        
        private void ActiveSelectionMode(bool flag) {
            selectionMode = flag;
            selectionUI.Visible(selectionMode);
            storeBlockerCanvas.Visible(selectionMode);
            descriptionUI.Visible(!selectionMode);
            
            holdersUI.ActiveSelectionMode(selectionMode);

            if (selectionMode) {
                descriptionUI.Clear();
                holdersUI.UnSetItem();
            } else {
                selectionUI.Clear();
            }

            string buttonText;
            if (CurrentLocale == YisoLocale.Locale.KR) {
                buttonText = selectionMode ? "선택 종료" : "전체 선택";
            } else {
                buttonText = selectionMode ? "Exit Selection" : "Selection All";
            }
            
            selectionButtonText.SetText(buttonText);
        }

        private void OnMoneyChanged(double moeny) {
            SetMoney();
        }

        private void OnStoreUIEvent(StoreUIEventArgs args) {
            if (clearMode) return;
            switch (args) {
                case StoreUIItemSelectedEventArgs selectArgs:
                    descriptionUI.SetItem(args.Type, selectArgs.Item, selectArgs.Price);
                    break;
                case StoreUIItemUnSelectedEventArgs:
                    descriptionUI.Clear();
                    break;
                case StoreUIInventoryItemSelectedEventArgs inventorySelectArgs:
                    if (selectionMode)
                        selectionUI.OnSelected(inventorySelectArgs);
                    break;
            }
        }

        private void OnInventoryEvent(YisoPlayerInventoryEventArgs args) {
            switch (args) {
                case YisoPlayerInventoryAddEventArgs addArgs:
                    var position = addArgs.Position;
                    holdersUI.SetItem(Types.INVENTORY,
                        position,
                        player.InventoryModule.GetItem<YisoItem>(currentType, position)
                    );
                    break;
                case YisoPlayerInventoryRemoveEventArgs removeArgs:
                    holdersUI.Clear(Types.INVENTORY,
                        removeArgs.Position);
                    break;
                case YisoPlayerInventoryCountEventArgs countArgs:
                    holdersUI.UpdateCount(countArgs.Position, countArgs.AfterCount);
                    break;
            }
        }

        private void OnClickSells(List<string> objectIds, double price) {
            var popupService = YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
            popupService.AlertS("일괄 판매", $"판매금액: <sprite=0> {price.ToCommaString()}\n정말 판매하시겠습니까?", () => {
                player.InventoryModule.SellItems(currentType, objectIds, price);
                ActiveSelectionMode(false);
            });
        }

        private void OnButtonAction(Types type, YisoItem item, double price) {
            var popupService = YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
            var gameUIService = YisoServiceProvider.Instance.Get<IYisoGameUIService>();
            var args = new YisoPopupInventoryInputArgs {
                Item = item,
                Price = price
            };
            switch (type) {
                case Types.STORE:
                    args.Type = YisoPopupInventoryInputContentUI.Types.PURCHASE;
                    args.OnClickOkList.Add(count => {
                        var totalPrice = price * count;
                        if (!player.InventoryModule.CanBuy(item, totalPrice, out var reason)) {
                            gameUIService.FloatingText(reason.ToString(CurrentLocale));
                            return;
                        }
                        player.InventoryModule.PurchaseItem(item, count, totalPrice);
                        holdersUI.UnSetItem();
                        descriptionUI.Clear();
                    });
                    break;
                case Types.INVENTORY:
                    args.Type = YisoPopupInventoryInputContentUI.Types.SELL;
                    args.OnClickOkList.Add(count => {
                        player.InventoryModule.SellItem(item, count, price);
                        holdersUI.UnSetItem();
                        descriptionUI.Clear();
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            args.OnClickOkList.Add(_ => holdersUI.UnSetItem());
            popupService.ShowInventoryInput(args);
        }

        public override YisoInteractTypes GetType() => YisoInteractTypes.STORE;

        public override void HandleData(object data) {
            var storeId = (int) data;
            var itemService = YisoServiceProvider.Instance.Get<IYisoItemService>();
            if (!itemService.TryGetStore(storeId, out currentStore))
                currentStore = itemService.CreateDevStore(storeId);
            
            SetItems(Types.INVENTORY);
            SetItems(Types.STORE);
            SetMoney();
        }

        private UnityAction<bool> OnToggleTab(int index) => flag => {
            var type = ToInvType(index);
            currentType = type;

            if (selectionMode) {
                selectionUI.Clear();
                ActiveSelectionMode(false);
            }

            holdersUI.Clear(Types.INVENTORY);
            if (type is YisoItem.InventoryType.EQUIP or YisoItem.InventoryType.USE)
                holdersUI.Clear(Types.STORE);
            descriptionUI.Clear();

            SetItems(Types.INVENTORY);
            SetItems(Types.STORE);
        };

        private void SetItems(Types type) {
            if (type == Types.INVENTORY) {
                var units = player.InventoryModule.InventoryUnits[currentType];
                foreach (var (position, item) in units.ItemDict) {
                    holdersUI.SetItem(type, position, item);
                }
                
                return;
            }
            
            if (currentType is YisoItem.InventoryType.ETC) return;
            var storeItems = currentStore[currentType];
            foreach (var item in storeItems) {
                holdersUI.SetItem(type, -1, item.Item, item.Price);
            }
        }

        private void SetMoney() {
            moneyText.SetText(player.InventoryModule.Money.ToCommaString());
        }
        
        private YisoItem.InventoryType ToInvType(int index) => index switch {
            0 => YisoItem.InventoryType.EQUIP,
            1 => YisoItem.InventoryType.USE,
            _ => YisoItem.InventoryType.ETC
        };
        
        public enum Types {
            STORE, INVENTORY
        }
    }
}