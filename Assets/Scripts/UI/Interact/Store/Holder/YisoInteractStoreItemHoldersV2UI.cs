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
    public class YisoInteractStoreItemHoldersV2UI : YisoUIController {
        [SerializeField, Title("Prefabs")] private GameObject holderPrefab;
        [SerializeField] private GameObject holderContent;

        [SerializeField, Title("Holders")] private List<YisoInteractStoreItemHolderUI> storeItemHolderUis;
        [SerializeField] private List<YisoInteractStoreInventoryItemHolderUI> inventoryItemHolderUis;

        [SerializeField, Title("Toggle")] private ToggleGroup toggleGroup;
        [SerializeField, Title("Scroll")] private ScrollRect[] scrollRects;

        private readonly List<YisoInteractStoreItemHolderUI> storeHolders = new();
        private readonly List<YisoInteractStoreInventoryItemHolderUI> inventoryHolders = new();

        public event UnityAction<StoreUIEventArgs> OnStoreUIEvent;
        
        private (YisoInteractStoreContentUI.Types type, int index) activeInfo;

        private void InitHolders() {
            inventoryHolders.AddRange(inventoryItemHolderUis);
            for (var i = 0; i < inventoryHolders.Count; i++) {
                inventoryHolders[i].InitItemHolder(toggleGroup, OnToggleHolder(i, YisoInteractStoreContentUI.Types.INVENTORY));
                inventoryHolders[i].OnSelectEvent += OnSelectHolder(i);
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            for (var i = 0; i < inventoryHolders.Count; i++) {
                inventoryHolders[i].OnSelectEvent -= OnSelectHolder(i);
            }
        }

        public void SetItem(YisoInteractStoreContentUI.Types type, int position, YisoItem item, double price = -1d) {
            if (inventoryHolders.IsEmpty()) InitHolders();

            if (type == YisoInteractStoreContentUI.Types.INVENTORY) {
                inventoryHolders[position].Active = true;
                inventoryHolders[position].SetItem(type, item, item.SellPrice, CurrentLocale);
                return;
            }

            if (!TryFindEmptyStoreHolderIndex(out var index)) {
                var newHolder = CreateHolder();
                newHolder.ItemToggle.group = toggleGroup;
                storeHolders.Add(newHolder);
                index = storeHolders.Count - 1;
                newHolder.ItemToggle.onValueChanged.AddListener(OnToggleHolder(index, type));
            }
            
            storeHolders[index].SetItem(type, item, price, CurrentLocale);
        }

        public void Clear(YisoInteractStoreContentUI.Types type, int index = -1) {
            switch (type) {
                case YisoInteractStoreContentUI.Types.INVENTORY when index != -1:
                    inventoryHolders[index].Clear();
                    return;
                case YisoInteractStoreContentUI.Types.STORE: {
                    foreach (var holder in storeHolders) holder.Clear();
                    break;
                }
                default: {
                    foreach (var holder in inventoryHolders) holder.Clear();
                    break;
                }
            }
            
            foreach (var rect in scrollRects) rect.verticalNormalizedPosition = 1f;
            foreach (var holderUI in inventoryItemHolderUis) holderUI.SetSelectMode(false);
        }

        public void ActiveSelectionMode(bool flag) {
            foreach (var holderUI in inventoryItemHolderUis) {
                holderUI.SetSelectMode(flag);
                if (holderUI.Active)
                    holderUI.IsOn = flag;
            }
        }

        public void UpdateCount(int position, int count) {
            inventoryHolders[position].UpdateCount(count, CurrentLocale);
        }
        
        public void UnSetItem() {
            if (activeInfo.index == -1) return;
            if (activeInfo.type == YisoInteractStoreContentUI.Types.STORE)
                storeHolders[activeInfo.index].ItemToggle.isOn = false;
            else
                inventoryHolders[activeInfo.index].HolderToggle.isOn = false;
        }

        private bool TryFindEmptyStoreHolderIndex(out int index) {
            index = -1;

            for (var i = 0; i < storeHolders.Count; i++) {
                if (storeHolders[i].Active) continue;
                index = i;
                break;
            }

            return index != -1;
        }
        
        private UnityAction<bool> OnSelectHolder(int index) => flag => {
            var holder = inventoryHolders[index];
            OnStoreUIEvent?.Invoke(new StoreUIInventoryItemSelectedEventArgs(flag, holder.Item));
        };

        private UnityAction<bool> OnToggleHolder(int index, YisoInteractStoreContentUI.Types type) => flag => {
            if (!flag) {
                OnStoreUIEvent?.Invoke(new StoreUIItemUnSelectedEventArgs(type));
                activeInfo.index = -1;
                return;
            }

            YisoItem item = null;
            var price = 0d;
            if (type == YisoInteractStoreContentUI.Types.STORE) {
                var holder = storeHolders[index];
                item = holder.Item;
                price = holder.Price;
            } else {
                var invHolder = inventoryHolders[index];
                item = invHolder.Item;
                price = invHolder.Price;
            }
            
            OnStoreUIEvent?.Invoke(new StoreUIItemSelectedEventArgs(type, item, price));
            activeInfo = (type, index);
        };
        
        private YisoInteractStoreItemHolderUI CreateHolder() {
            return CreateObject<YisoInteractStoreItemHolderUI>(holderPrefab, holderContent.transform);
        }
    }
}