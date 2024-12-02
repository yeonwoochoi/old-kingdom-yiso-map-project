using System.Collections.Generic;
using Core.Domain.Wanted;
using Sirenix.OdinInspector;
using UI.Base;
using UI.Interact.Wanted.Event;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Interact.Wanted.Item {
    public class YisoInteractWantedItemsUI : YisoUIController {
        [SerializeField, Title("Prefab")] private GameObject itemPrefab;
        [SerializeField] private GameObject contentPrefab;
        [SerializeField] private YisoInteractWantedItemUI[] defaultItems;
        [SerializeField, Title("Toggle")] private ToggleGroup toggleGroup;
        [SerializeField, Title("ScrollRect")] private ScrollRect scrollRect;

        public event UnityAction<WantedUIEventArgs> OnWantedUIEvent; 

        private readonly List<YisoInteractWantedItemUI> items = new();

        private int activeItemIndex = -1;
        
        public void Clear() {
            foreach (var item in items) item.Init();
        }

        public void UnsetItem() {
            scrollRect.verticalNormalizedPosition = 1f;
            if (activeItemIndex == -1) return;
            items[activeItemIndex].ItemToggle.isOn = false;
        }

        private void InitHolders() {
            items.AddRange(defaultItems);
            for (var i = 0; i < items.Count; i++) {
                var item = items[i];
                item.ItemToggle.group = toggleGroup;
                item.Init();
                item.ItemToggle.onValueChanged.AddListener(OnToggleItem(i));
            }
        }

        public void SetItem(YisoWanted wanted) {
            if (items.Count == 0) InitHolders();

            if (!TryFindEmptyItemIndex(out var index)) {
                var newItem = CreateItem();
                newItem.ItemToggle.group = toggleGroup;
                items.Add(newItem);
                index = items.Count;
                newItem.ItemToggle.onValueChanged.AddListener(OnToggleItem(index));
            }
            
            items[index].SetItem(wanted, CurrentLocale);
        }
        
        private bool TryFindEmptyItemIndex(out int index) {
            index = -1;

            for (var i = 0; i < items.Count; i++) {
                if (items[i].Active) continue;
                index = i;
                break;
            }

            return index != -1;
        }
        
        private UnityAction<bool> OnToggleItem(int index) => flag => {
            if (!flag) {
                OnWantedUIEvent?.Invoke(new WantedUIUnSelectedEventArgs());
                activeItemIndex = -1;
                return;
            }
            
            OnWantedUIEvent?.Invoke(new WantedUISelectedEventArgs(items[index].Wanted));
            activeItemIndex = index;
        };
        
        private YisoInteractWantedItemUI CreateItem() {
            return CreateObject<YisoInteractWantedItemUI>(itemPrefab, contentPrefab.transform);
        }
    }
}