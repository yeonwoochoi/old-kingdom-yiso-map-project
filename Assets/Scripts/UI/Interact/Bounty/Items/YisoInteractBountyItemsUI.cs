using System.Collections.Generic;
using Core.Domain.Bounty;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using UI.Base;
using UI.Interact.Bounty.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Interact.Bounty.Items {
    public class YisoInteractBountyItemsUI : YisoUIController {
        [SerializeField, Title("Prefab")] private GameObject itemPrefab;
        [SerializeField] private GameObject contentPrefab;
        [SerializeField] private YisoInteractBountyItemUI[] defaultItems;
        [SerializeField, Title("Toggle")] private ToggleGroup toggleGroup;
        [SerializeField, Title("ScrollRect")] private ScrollRect scrollRect;

        public event UnityAction<BountyUIEventArgs> OnBountyEvent;

        private readonly List<YisoInteractBountyItemUI> items = new();
        
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

        public void SetItem(YisoBounty bounty, YisoBountyStatus status) {
            if (items.Count == 0) InitHolders();

            if (!TryFindEmptyItemIndex(out var index)) {
                var newItem = CreateItem();
                newItem.ItemToggle.group = toggleGroup;
                items.Add(newItem);
                index = items.Count;
                newItem.ItemToggle.onValueChanged.AddListener(OnToggleItem(index));
            }
            
            items[index].SetBounty(bounty, status, CurrentLocale);
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
                OnBountyEvent?.Invoke(new BountyUIUnSelectedEventArgs());
                activeItemIndex = -1;
                return;
            }

            var bounty = items[index].Bounty;
            var status = items[index].Status;
            OnBountyEvent?.Invoke(new BountyUISelectedEventArgs(bounty, status));
            activeItemIndex = index;
        };
        
        private YisoInteractBountyItemUI CreateItem() {
            return CreateObject<YisoInteractBountyItemUI>(itemPrefab, contentPrefab.transform);
        }
    }
}