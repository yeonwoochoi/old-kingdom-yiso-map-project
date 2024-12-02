using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Behaviour;
using Core.Domain.Item;
using Sirenix.OdinInspector;
using UI.Menu.Inventory.Event;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Menu.Inventory.V2.Holder {
    public class YisoMenuInventoryV2ItemHoldersUI : RunIBehaviour {
        [SerializeField, Title("Defaults")] private YisoMenuInventoryV2ItemHolderUI[] defaultHolders; 
        [SerializeField, Title("Prefabs")] private GameObject holderPrefab;
        [SerializeField] private GameObject contentPrefab;
        [SerializeField] private ToggleGroup toggleGroup;

        public event UnityAction<InventoryUIEventArgs> OnInventoryEvent; 

        private readonly List<YisoMenuInventoryV2ItemHolderUI> holders = new();

        public void Clear(int position = -1) {
            if (position != -1) {
                holders[position].Clear();
                return;
            }
            
            foreach (var holder in holders) holder.Clear();
        }

        public void UpdateCount(int position, int count) {
            holders[position].UpdateCount(count);
        }

        public void UpdateDiffs() {
            foreach (var holder in holders) {
                if (!holder.Active) continue;
                holder.SetDiff();
            }
        }

        public void UpdateDiffs(YisoWeapon.AttackType type) {
            foreach (var holder in holders.Where(h => h.Active)) {
                holder.SetDiff(type);
            }
        }

        public void CreateHolders(int slotLimit) {
            for (var i = 0; i < defaultHolders.Length; i++) {
                var holder = defaultHolders[i];
                holder.Init();
                holder.ItemToggle.group = toggleGroup;
                holder.ItemToggle.onValueChanged.AddListener(OnToggleHolder(i));
                holders.Add(holder);
            }
            
            if (holders.Count == slotLimit) return;
            
            for (var i = holders.Count; i < slotLimit - holders.Count; i++) {
                var holder = CreateHolder();
                holder.ItemToggle.group = toggleGroup;
                holder.ItemToggle.onValueChanged.AddListener(OnToggleHolder(i));
                holders.Add(holder);
            }
        }

        public void SetItem(int position, YisoItem item, YisoWeapon.AttackType currentWeapon) {
            holders[position].SetItem(item, currentWeapon);
        }

        private YisoMenuInventoryV2ItemHolderUI CreateHolder() => CreateObject<YisoMenuInventoryV2ItemHolderUI>(holderPrefab, contentPrefab.transform);

        private UnityAction<bool> OnToggleHolder(int index) => flag => {
            if (!flag) {
                OnInventoryEvent?.Invoke(new InventoryUIItemUnSelectedEventArgs());
                return;
            }
            
            OnInventoryEvent?.Invoke(new InventoryUIItemSelectedEventArgs(holders[index].GetItem<YisoItem>()));
        };
    }
}