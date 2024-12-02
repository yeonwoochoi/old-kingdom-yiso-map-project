using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Item;
using Unity.VisualScripting;

namespace Core.Domain.Actor.Player.Modules.Inventory {
    public sealed class YisoPlayerInventoryUnit {
        private const int SLOT_LIMIT = 15;

        private readonly YisoItem.InventoryType type;

        private IEnumerable<YisoItem> Items => ItemDict.Values;

        public Dictionary<int, YisoItem> ItemDict { get; } = new();

        public YisoPlayerInventoryUnit(YisoItem.InventoryType type) {
            this.type = type;
        }

        public int SlotLimit => SLOT_LIMIT;
        
        private bool IsFull => ItemDict.Count >= SLOT_LIMIT;

        public bool CanAdd() => !IsFull;

        public void LoadItem(YisoItem item, int position) {
            ItemDict[position] = item;
        }

        public bool TryAddItem(YisoItem item, int count, out List<YisoPlayerInventoryEventArgs> argsList) {
            argsList = new List<YisoPlayerInventoryEventArgs>();

            var invType = item.InvType;

            if (invType == YisoItem.InventoryType.EQUIP || !TryFindBy(i => i.Id == item.Id, out var existItem)) {
                if (!TryAddNewItem(item, count, out var reason)) {
                    argsList.Add(new YisoPlayerInventoryFailureEventArgs(reason, invType));
                    return false;
                }
                argsList.Add(new YisoPlayerInventoryAddEventArgs(invType, item.Position));
                return true;
            }

            existItem.Quantity += count;
            argsList.Add(new YisoPlayerInventoryCountEventArgs(invType, existItem.Position, existItem.Id, existItem.Quantity));
            return true;
        }

        public bool TryRemoveItem(int position, int count, out YisoItem removedItem, out YisoPlayerInventoryEventArgs args) {
            args = null;
            removedItem = null;
            
            if (!TryGetItem(position, out removedItem)) {
                args = new YisoPlayerInventoryFailureEventArgs(YisoPlayerInventoryReasons.ITEM_NOT_FOUND, type);
                return false;
            }

            if (removedItem.InvType == YisoItem.InventoryType.EQUIP) {
                RemoveSlot(position);
                args = new YisoPlayerInventoryRemoveEventArgs(type, position);
                return true;
            }

            removedItem.Quantity -= count;
            if (removedItem.Quantity < 0) removedItem.Quantity = 0;
            if (removedItem.Quantity == 0) {
                RemoveSlot(position);
                args = new YisoPlayerInventoryRemoveEventArgs(type, position);
                return true;
            }

            args = new YisoPlayerInventoryCountEventArgs(type, position, removedItem.Id, removedItem.Quantity);
            return true;
        }

        private void AddItemInternal(YisoItem item, int count = 1) {
            if (TryFindBy(i => i.Id == item.Id, out var existItem) && item.InvType != YisoItem.InventoryType.EQUIP) {
                existItem.Quantity += count;
                return;
            }
            
            var position = GetNextFreeSlot();
            item.Position = position;
            ItemDict[position] = item;
            item.Quantity = count;
        }

        private bool TryAddNewItem(YisoItem item, int count, out YisoPlayerInventoryReasons reason) {
            reason = YisoPlayerInventoryReasons.NONE;

            if (!TryGetNextFreeSlot(out var position, out reason)) {
                return false;
            }

            ItemDict[position] = item;
            item.Quantity = count;
            item.Position = position;

            return true;
        }
        
        private int GetNextFreeSlot() {
            for (var i = 0; i < SlotLimit; i++) {
                if (ItemDict.ContainsKey(i)) continue;
                return i;
            }

            return -1;
        }

        public bool TryGetNextFreeSlot(out int position, out YisoPlayerInventoryReasons reason) {
            position = -1;
            reason = YisoPlayerInventoryReasons.NONE;

            if (IsFull) {
                reason = YisoPlayerInventoryReasons.INVENTORY_SLOT_FULL;
                return false;
            }

            for (var i = 0; i < SlotLimit; i++) {
                if (ItemDict.ContainsKey(i)) continue;
                position = i;
                break;
            }
            
            return position != -1;
        }

        public bool TryFindsBy(Func<YisoItem, bool> predicate, out IReadOnlyList<YisoItem> items) {
            items = Items.Where(predicate).ToList();
            return items is { Count: > 0 };
        }

        public bool TryFindsBy<T>(Func<T, bool> predicate, out IReadOnlyList<T> items) where T : YisoItem {
            items = Items.OfType<T>().Where(predicate).ToList();
            return items is { Count: > 0 };
        }

        public bool TryFirstBy<T>(Func<T, bool> predicate, out T item) where T : YisoItem {
            item = Items.OfType<T>().FirstOrDefault(predicate);
            return item != null;
        }

        public bool TryFindBy(Func<YisoItem, bool> predicate, out YisoItem item) {
            item = Items.FirstOrDefault(predicate);
            return item != null;
        }

        public bool TryFindBy<T>(Func<T, bool> predicate, out T item) where T : YisoItem {
            item = Items.OfType<T>().FirstOrDefault(predicate);
            return item != null;
        }

        private bool TryGetItem(int position, out YisoItem item) => ItemDict.TryGetValue(position, out item);
        
        private void RemoveSlot(int position) {
            ItemDict.Remove(position);
        }

        public int GetActiveItemCount() => ItemDict.Count;
        
        public YisoItem this[int position] => ItemDict[position];
    }
}