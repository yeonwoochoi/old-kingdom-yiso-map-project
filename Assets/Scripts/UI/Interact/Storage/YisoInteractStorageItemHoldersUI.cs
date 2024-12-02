using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Item;
using DG.Tweening;
using Sirenix.OdinInspector;
using UI.Interact.Storage.Event;
using UI.Menu.Inventory.Holder;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Storage {
    public class YisoInteractStorageItemHoldersUI : RunIBehaviour {
        [SerializeField, Title("Holders")] private YisoMenuInventoryItemHolderUI[] storageHolderUIs;
        [SerializeField] private YisoMenuInventoryItemHolderUI[] inventoryHolderUIs;
        [SerializeField] private ScrollRect[] scrollRects;

        private readonly Dictionary<Types, List<YisoMenuInventoryItemHolderUI>> holders = new();

        public event UnityAction<StorageUIEventArgs> OnStorageUIEvent; 

        private void InitHolders() {
            foreach (var type in EnumExtensions.Values<Types>()) {
                if (type == Types.STORAGE) holders[type] = new List<YisoMenuInventoryItemHolderUI>(storageHolderUIs);
                else holders[type] = new List<YisoMenuInventoryItemHolderUI>(inventoryHolderUIs);

                for (var i = 0; i < holders[type].Count; i++) {
                    holders[type][i].Init();
                    holders[type][i].ItemToggle.onValueChanged.AddListener(OnToggleHolder(i, type));
                }
            }
        }

        public void Clear(Types type, int index = -1) {
            if (index != -1) {
                holders[type][index].Clear();
                return;
            }
            
            foreach (var holder in holders[type]) holder.Clear();
            foreach (var rect in scrollRects) rect.DOVerticalNormalizedPos(1f, .1f);
        }

        public void SetItem(Types type, int position, YisoItem item) {
            if (holders.Count == 0) InitHolders();
            holders[type][position].SetItem(item);
        }

        public void UpdateCount(Types type, int position, int count) {
            holders[type][position].UpdateCount(count);
        }

        private UnityAction<bool> OnToggleHolder(int index, Types type) => flag => {
            if (!flag) {
                OnStorageUIEvent?.Invoke(new StorageUIItemUnSelectedEventArgs(type));
                return;
            }
            
            OnStorageUIEvent?.Invoke(new StorageUIItemSelectedEventArgs(type, holders[type][index].Item));
        };

        public enum Types {
            STORAGE, INVENTORY
        }
    }
}