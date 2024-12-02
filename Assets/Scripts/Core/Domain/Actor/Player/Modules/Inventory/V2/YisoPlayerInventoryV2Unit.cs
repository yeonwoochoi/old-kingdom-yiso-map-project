using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Item;
using UnityEngine;

namespace Core.Domain.Actor.Player.Modules.Inventory.V2 {
    public class YisoPlayerInventoryV2Unit {
        public Dictionary<int, YisoItem> ItemDict { get; } = new();

        public IEnumerable<YisoItem> Items => ItemDict.Values;

        private readonly YisoItem.InventoryType type;

        private short slotLimit;
        
        public short SlotLimit {
            get => slotLimit;
            set {
                slotLimit = value;
                ChangeSlotLimit();
            }
        }

        public YisoPlayerInventoryV2Unit(YisoItem.InventoryType type, short slotLimit = 20) {
            this.type = type;
            SlotLimit = slotLimit;
        }

        public bool CanAdd() => !IsFull();

        public int GetActiveItemCount() => ItemDict.Count;

        public void ReOrder() {
            var tempDict = new Dictionary<int, YisoItem>(ItemDict);
            List<YisoItem> sortedList;
            if (type == YisoItem.InventoryType.EQUIP) {
                sortedList = tempDict
                    .OrderByDescending(kvp => ((YisoEquipItem)kvp.Value).Rank)
                    .ThenBy(kvp => ((YisoEquipItem)kvp.Value).Slot)
                    .Select(kvp => kvp.Value)
                    .ToList();
            } else {
                sortedList = tempDict
                    .OrderBy(kvp => kvp.Value.Id)
                    .Select(kvp => kvp.Value)
                    .ToList();
            }
            
            ItemDict.Clear();
            var position = 0;
            foreach (var item in sortedList) {
                item.Position = position;
                ItemDict[position] = item;
                position++;
            }
        }
        
        #region ADD

        public bool TryAddItem(YisoItem item, int count, out YisoPlayerInventoryEventArgs args) {
            var invType = item.InvType;

            if (invType == YisoItem.InventoryType.EQUIP || !TryGetByItemId(item.Id, out var exitItem)) {
                var addNewReason = AddNewItem(item, count);
                if (addNewReason == YisoPlayerInventoryReasons.NONE) {
                    args = new YisoPlayerInventoryAddEventArgs(invType, item.Position);
                    return true;
                }
                
                args = new YisoPlayerInventoryFailureEventArgs(addNewReason, invType);
                return false;
            }

            exitItem.Quantity += count;
            args = new YisoPlayerInventoryCountEventArgs(invType, exitItem.Position, exitItem.Id, exitItem.Quantity);
            return true;
        }

        private YisoPlayerInventoryReasons AddNewItem(YisoItem item, int count) {
            var freeSlotPosition = FindFreeSlotPosition();

            if (freeSlotPosition == -1) return YisoPlayerInventoryReasons.INVENTORY_SLOT_FULL;

            ItemDict[freeSlotPosition] = item;
            item.Quantity = count;
            item.Position = freeSlotPosition;
            return YisoPlayerInventoryReasons.NONE;
        }
        
        #endregion
        
        #region REMOVE

        public void RemoveItem(int position, int count, out YisoItem removedItem,
            out YisoPlayerInventoryEventArgs args) {
            args = null;
            removedItem = null;

            if (!TryGetItem(position, out removedItem)) {
                throw new Exception($"Item(position={position}) not found!");
            }

            if (removedItem.InvType == YisoItem.InventoryType.EQUIP) {
                RemoveSlot(position);
                args = new YisoPlayerInventoryRemoveEventArgs(type, position);
                return;
            }

            removedItem.Quantity -= count;
            removedItem.Quantity = Mathf.Max(0, removedItem.Quantity);
            if (removedItem.Quantity == 0) {
                RemoveSlot(position);
                args = new YisoPlayerInventoryRemoveEventArgs(type, position);
                return;
            }

            args = new YisoPlayerInventoryCountEventArgs(type, position, removedItem.Id, removedItem.Quantity);
        }
        
        private void RemoveSlot(int position) {
            ItemDict.Remove(position);
        }
        
        #endregion
        
        #region REPLACE

        public void SwapItem(YisoItem item1, YisoItem item2) {
            var position = item1.Position;
            ItemDict[position] = item2;
        }
        
        #endregion
        
        #region DATA

        public void Load(YisoItem item, int position) {
            ItemDict[position] = item;
        }

        public void Reset() {
            ItemDict.Clear();
        }
        
        #endregion
        
        #region HELPER_METHODS

        public bool TryGetByItemId(int itemId, out YisoItem item) => TryGetBy(i => i.Id == itemId, out item);

        public bool TryGetByItemId(string objectId, out YisoItem item) =>
            TryGetBy(i => i.ObjectId == objectId, out item);
        
        public bool TryGetByItemId<T>(string objectId, out T item) where T : YisoItem =>
            TryGetBy<T>(i => i.ObjectId == objectId, out item);

        public bool TryGetByItemId<T>(int itemId, out T item) where T : YisoItem =>
            TryGetBy<T>(i => i.Id == itemId, out item);

        public bool TryGetByObjectId(string objectId, out YisoItem item) =>
            TryGetBy(i => i.ObjectId == objectId, out item);

        public bool TryGetByObjectId<T>(string objectId, out T item) where T : YisoItem =>
            TryGetBy<T>(i => i.ObjectId == objectId, out item);

        public bool TryGetBy<T>(Func<T, bool> predicate, out T item) where T : YisoItem {
            item = Items.OfType<T>().FirstOrDefault(predicate);
            return item != null;
        }

        public bool TryGetBy(Func<YisoItem, bool> predicate, out YisoItem item) {
            item = Items.FirstOrDefault(predicate);
            return item != null;
        }
        
        private bool TryGetItem(int position, out YisoItem item) => ItemDict.TryGetValue(position, out item);

        
        #endregion

        private int FindFreeSlotPosition() {
            for (var i = 0; i < SlotLimit; i++) {
                if (ItemDict.ContainsKey(i)) continue;
                return i;
            }

            return -1;
        }
        
        private bool IsFull() => ItemDict.Count >= SlotLimit;

        private void ChangeSlotLimit() {
            
        }

        public YisoItem this[int position] => ItemDict[position];
    }
}