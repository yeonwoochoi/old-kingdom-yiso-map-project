using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Item;
using Sirenix.OdinInspector;
using UI.Interact.Blacksmith.Event;
using UI.Menu.Inventory.Event;
using UI.Menu.Inventory.Holder;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Interact.Blacksmith {
    public class YisoInteractBlacksmithItemHoldersUI : RunIBehaviour {
        [SerializeField, Title("Settings")] private int holderCount = 15;
        [SerializeField, Title("Holders")] private YisoMenuInventoryItemHolderUI[] holderUIs;
        
        private readonly List<YisoMenuInventoryItemHolderUI> holders = new();

        private int currentHolderIndex = -1;
        
        public event UnityAction<BlacksmithUIEventArgs> OnEventRaised; 
        
        public void Clear(int index = -1) {
            if (index != -1) {
                holders[index].Clear();
                return;
            }
            foreach (var holder in holders) holder.Clear();
        }

        private void InitHolders() {
            holders.AddRange(holderUIs);
            for (var i = 0; i < holders.Count; i++) {
                holders[i].Init();
                holders[i].ItemToggle.onValueChanged.AddListener(OnToggleHolder(i));
            }
        }

        public void UnSelect() {
            if (currentHolderIndex == -1) return;
            holders[currentHolderIndex].ItemToggle.isOn = false;
            currentHolderIndex = -1;
        }

        public void SetItem(int slot, YisoItem item) {
            if (holders.Count == 0) {
                InitHolders();
            }
            holders[slot].SetItem(item);
        }

        public void UpdateItem(YisoItem item) {
            holders[item.Position].UpdateItem();
        }

        private UnityAction<bool> OnToggleHolder(int index) => flag => {
            if (!flag) {
                OnEventRaised?.Invoke(new BlacksmithUIItemUnSelectedEventArgs());
                return;
            }

            currentHolderIndex = index;
            OnEventRaised?.Invoke(new BlacksmithUIItemSelectedEventArgs(holders[index].Item));
        };
    }
}