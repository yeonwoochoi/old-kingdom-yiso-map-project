using System;
using System.Collections.Generic;
using System.Linq;
using Core.Behaviour;
using Core.Domain.Item;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UI.Menu.Inventory.Event;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Menu.Inventory.Holder {
    public class YisoMenuInventoryItemHoldersUI : RunIBehaviour {
        [SerializeField, Title("Prefabs")] private GameObject holderPrefab;
        [SerializeField, Title("Toggles")] private ToggleGroup holderToggleGroup;
        [SerializeField, Title("Settings")] private int holderCount = 15;
        [SerializeField, Title("Holders")] private YisoMenuInventoryItemHolderUI[] holderUIs;
        
        private readonly List<YisoMenuInventoryItemHolderUI> holders = new();

        public event UnityAction<InventoryUIEventArgs> OnEventRaised; 
        
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

        public void SetItem(int slot, YisoItem item) {
            if (holders.Count == 0) {
                InitHolders();
            }
            holders[slot].SetItem(item);
        }

        public void UpdateCount(int position, int count) {
            holders[position].UpdateCount(count);
        }

        public void UpdateEquip(int position, bool equipped) {
            holders[position].UpdateEquip(equipped);
        }

        private YisoMenuInventoryItemHolderUI CreateItem(int index) {
            var holder = CreateObject<YisoMenuInventoryItemHolderUI>(holderPrefab);
            holder.ItemToggle.group = holderToggleGroup;
            holder.ItemToggle.onValueChanged.AddListener(OnToggleHolder(index));
            return holder;
        }

        private UnityAction<bool> OnToggleHolder(int index) => flag => {
            if (!flag) {
                OnEventRaised?.Invoke(new InventoryUIItemUnSelectedEventArgs());
                return;
            }
            
            OnEventRaised?.Invoke(new InventoryUIItemSelectedEventArgs(holders[index].Item));
        };
    }
}