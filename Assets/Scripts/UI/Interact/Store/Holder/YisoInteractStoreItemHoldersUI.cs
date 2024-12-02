using System.Collections.Generic;
using Core.Domain.Item;
using Sirenix.OdinInspector;
using UI.Base;
using UI.Interact.Store.Event;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Store.Holder {
    public class YisoInteractStoreItemHoldersUI : YisoUIController {
        [SerializeField, Title("Prefabs")] private GameObject holderPrefab;
        [SerializeField] private GameObject holderContent;
        
        [SerializeField, Title("Holders")] private List<YisoInteractStoreItemHolderUI> storeItemHolderUis;
        [SerializeField] private List<YisoInteractStoreItemHolderUI> inventoryItemHolderUis;

        [SerializeField, Title("Toggle")] private ToggleGroup storeToggleGroup;
        [SerializeField] private ToggleGroup inventoryToggleGroup;
        [SerializeField, Title("Scroll")] private ScrollRect[] scrollRects;

        private readonly Dictionary<YisoInteractStoreContentUI.Types, List<YisoInteractStoreItemHolderUI>> holders = new();

        public event UnityAction<StoreUIEventArgs> OnStoreUIEvent;

        private (YisoInteractStoreContentUI.Types type, int index) activeInfo;
        
        private void InitHolders() {
            foreach (var type in EnumExtensions.Values<YisoInteractStoreContentUI.Types>()) {
                var defaults = type == YisoInteractStoreContentUI.Types.STORE ? storeItemHolderUis : inventoryItemHolderUis;
                holders[type] = new List<YisoInteractStoreItemHolderUI>(defaults);
                for (var i = 0; i < holders[type].Count; i++) {
                    holders[type][i].Init();
                    holders[type][i].ItemToggle.onValueChanged.AddListener(OnToggleHolder(i, type));
                    holders[type][i].ItemToggle.group = storeToggleGroup;
                }
            }
        }

        public void Clear(YisoInteractStoreContentUI.Types type, int index = -1) {
            if (type == YisoInteractStoreContentUI.Types.INVENTORY && index != -1) {
                holders[type][index].Clear();
                return;
            }
            foreach (var holder in holders[type]) holder.Clear();
            foreach (var rect in scrollRects) {
                rect.verticalNormalizedPosition = 1f;
            }
            
        }

        public void UpdateCount(int position, int count) {
            holders[YisoInteractStoreContentUI.Types.INVENTORY][position].UpdateCount(count, CurrentLocale);
        }

        public void SetItem(YisoInteractStoreContentUI.Types type, int position, YisoItem item, double price = -1) {
            if (holders.Count == 0) InitHolders();
            
            if (type == YisoInteractStoreContentUI.Types.INVENTORY) {
                holders[type][position].SetItem(type, item, item.SellPrice, CurrentLocale);
                return;
            }

            if (!TryFindEmptyStoreHolderIndex(out var index)) {
                var newHolder = CreateHolder();
                newHolder.ItemToggle.group = storeToggleGroup;
                holders[type].Add(newHolder);
                index = holders[type].Count - 1;
                newHolder.ItemToggle.onValueChanged.AddListener(OnToggleHolder(index, type));
            }
            
            holders[type][index].SetItem(type, item, price, CurrentLocale);
        }

        public void UnSetItem() {
            holders[activeInfo.type][activeInfo.index].ItemToggle.isOn = false;
        }

        private bool TryFindEmptyStoreHolderIndex(out int index) {
            index = -1;
            
            for (var i = 0; i < holders[YisoInteractStoreContentUI.Types.STORE].Count; i++) {
                if (holders[YisoInteractStoreContentUI.Types.STORE][i].Active) continue;
                index = i;
                break;
            }
            
            return index != -1;
        }
        
        private YisoInteractStoreItemHolderUI CreateHolder() {
            return CreateObject<YisoInteractStoreItemHolderUI>(holderPrefab, holderContent.transform);
        }

        private UnityAction<bool> OnToggleHolder(int index, YisoInteractStoreContentUI.Types type) => flag => {
            if (!flag) {
                OnStoreUIEvent?.Invoke(new StoreUIItemUnSelectedEventArgs(type));
                return;
            }

            var holder = holders[type][index];
            OnStoreUIEvent?.Invoke(new StoreUIItemSelectedEventArgs(type, holder.Item, holder.Price));
            activeInfo = (type, index);
        };
    }
}