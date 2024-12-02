using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Actor.Player.Modules.Base;
using Core.Domain.Actor.Player.Modules.Inventory.Reinforce;
using Core.Domain.Data;
using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Locale;
using Core.Domain.Quest;
using Core.Domain.Types;
using Core.Logger;
using Core.Service;
using Core.Service.Character;
using Core.Service.Data;
using Core.Service.Data.Item;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace Core.Domain.Actor.Player.Modules.Inventory {
    public class YisoPlayerInventoryModule : YisoPlayerBaseModule {
        public event UnityAction<double> OnMoneyChanged;
        public event UnityAction<YisoPlayerInventoryEventArgs> OnInventoryEvent;

        private readonly YisoPlayerInventoryNormalReinforcer normalReinforcer = new();
        private readonly YisoPlayerInventoryPotentialReinforcer potentialReinforcer = new();

        public Dictionary<YisoEquipSlots, int> EquippedItemPositions { get; } = new();
        public Dictionary<YisoItem.InventoryType, YisoPlayerInventoryUnit> InventoryUnits { get; } = new();

        private double money = 0;

        public double Money => money;


        public YisoPlayerInventoryModule(YisoPlayer player) : base(player) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                InventoryUnits[type] = new YisoPlayerInventoryUnit(type);
            }
        }

        public void ResetData() {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                InventoryUnits[type] = new YisoPlayerInventoryUnit(type);
            }
        }
        
        #region MONEY

        public void SetMoney(double money) {
            this.money = money;
            RaiseMoneyChanged();
        }

        public void AddMoney(double value) {
            money += value;
            RaiseMoneyChanged();
        }

        public bool TryReduceMoney(double value, out YisoPlayerInventoryReasons reason) {
            reason = YisoPlayerInventoryReasons.NONE;
            if (money - value < 0) {
                reason = YisoPlayerInventoryReasons.MONEY_NOT_ENOUGH;
                return false;
            }

            money -= value;
            RaiseMoneyChanged();

            return reason == YisoPlayerInventoryReasons.NONE;
        }
        
        #endregion

        #region STORE
        
        public void SellItem(YisoItem item, int count, double price) {
            var type = item.InvType;
            var position = item.Position;
            if (!InventoryUnits[type].TryRemoveItem(position, count, out _, out var args)) {
                throw new Exception("Cannot Remove Item!");
            }

            AddMoney(count * price);
            RaiseInventoryEvent(args);
        }

        public void PurchaseItem(YisoItem item, int count, double price) {
            AddItem(item, count);
            AddMoney(-price);
        }

        public void SellItems(YisoItem.InventoryType type, List<int> itemIds, double price) {
            
            foreach (var id in itemIds) {
                if (!InventoryUnits[type].TryFindBy(i => i.Id == id, out var item))
                    throw new Exception($"Item(id={id}|type={type}) not exist!");
                var position = item.Position;
                if (!InventoryUnits[type].TryRemoveItem(position, item.Quantity, out _, out var args)) {
                    throw new Exception($"Item(id={{id}}|type={type}) cannot remove!");
                }
                
                RaiseInventoryEvent(args);
            }
            
            AddMoney(price);
        }
        
        #endregion

        #region STORE
        
        public void StoreItem(YisoItem item, int count) {
            var position = item.Position;
            var quantity = count;
            InventoryUnits[item.InvType].TryRemoveItem(position, quantity, out _, out var args);
            RaiseInventoryEvent(args);
            player.StorageModule.AddItem(item, quantity);
        }
        
        #endregion

        #region Reinforce

        public YisoPlayerInventoryNormalReinforceResult GetReinforceNormalResult(YisoEquipItem item) {
            return normalReinforcer.GetResult(item);
        }

        public YisoPlayerInventoryPotentialReinforceResult GetReinforcePotentialResult(YisoEquipItem item) {
            return potentialReinforcer.GetResult(item);
        }

        /*
        public void Reinforce(YisoPlayerInventoryReinforceResult result, YisoEquipItem item) {
            var equipItem = (YisoEquipItem) InventoryUnits[YisoItem.InventoryType.EQUIP][item.Position];
            equipItem.Reinforce(result);
            AddMoney(-result.Price);
            player.StatModule.CalculateCombatRating();
        }
        */

        #endregion

        #region Inventory Menu
        
        /*public void EquipItem(YisoEquipItem item) {
            var slot = item.Slot;
            var position = item.Position;
            var unEquipped = false;
            if (EquippedItemPositions.TryGetValue(slot, out var unEquipPosition)) {
                var unEquipItem = (YisoEquipItem)InventoryUnits[YisoItem.InventoryType.EQUIP][unEquipPosition];
                unEquipItem.Equipped = false;
                unEquipped = true;
            }

            item.Equipped = true;
            EquippedItemPositions[slot] = position;
            player.StatModule.CalculateCombatRating();
            RaiseInventoryEvent(new YisoPlayerInventoryEquipEventArgs(position, slot, unEquipped, unEquipPosition));
        }

        public void UnEquipItem(YisoEquipItem item) {
            var slot = item.Slot;
            item.Equipped = false;
            player.StatModule.CalculateCombatRating();
            EquippedItemPositions.Remove(slot);
            RaiseInventoryEvent(new YisoPlayerInventoryUnEquipEventArgs(item.Position, slot));
        }*/
        

        public void DropItem(YisoItem item, int count = 1) {
            var type = item.InvType;
            var position = item.Position;
            if (!InventoryUnits[type].TryRemoveItem(position, count, out var removedItem, out var args)) {
                throw new Exception("Cannot Remove Item!");
            }

            RaiseInventoryEvent(args);
            RaiseInventoryEvent(new YisoPlayerInventoryDropEventArgs(type, removedItem.DeepCopy(), position));
        }

        /*public void UseItem(YisoUseItem item, int position = -1) {
            // CREATE EFFECT
            item.Quantity -= 1;
            RaiseInventoryEvent(new YisoPlayerInventoryCountEventArgs(YisoItem.InventoryType.USE, item.Position,
                item.Id, item.Quantity));
            if (position == -1) return;
            player.UIModule.UpdateItem(position, item.Quantity);
        }

        public void UseItem(int sourceId, int position) {
            if (!InventoryUnits[YisoItem.InventoryType.USE].TryFindBy(i => i.Id == sourceId, out var item)) {
                throw new Exception($"Item({sourceId}) not exists!");
            }

            var useItem = (YisoUseItem)item;
            useItem.Quantity -= 1;
            player.UIModule.UpdateItem(position, useItem.Quantity);
            // CREATE EFFECT
            RaiseInventoryEvent(new YisoPlayerInventoryCountEventArgs(YisoItem.InventoryType.USE, position, item.Id,
                item.Quantity));
        }*/

        public bool TryUseArrow(int count, out YisoPlayerInventoryReasons reason) {
            reason = YisoPlayerInventoryReasons.NONE;

            if (!InventoryUnits[YisoItem.InventoryType.USE]
                    .TryFirstBy<YisoUseItem>(item => item.IsArrow, out var arrowItem)) {
                reason = YisoPlayerInventoryReasons.ARROW_NOT_EXIST;
                return false;
            }

            if (arrowItem.Quantity != 0) {
                arrowItem.Quantity -= count;
                var position = arrowItem.Position;
                RaiseInventoryEvent(new YisoPlayerInventoryCountEventArgs(
                    YisoItem.InventoryType.USE,
                    position,
                    arrowItem.Id,
                    arrowItem.Quantity));
                return true;
            }

            reason = YisoPlayerInventoryReasons.ARROW_NOT_ENOUGH;
            return false;
        }

        public int GetArrowCount() {
            InventoryUnits[YisoItem.InventoryType.USE]
                .TryFindsBy<YisoUseItem>(item => item.IsArrow, out var items);
            return items.Select(item => item.Quantity).Sum();
        }
        
        #endregion
        
        #region ITEM CONTROL

        public void AddItem(YisoItem item, int count = 1) {
            item = item.DeepCopy();
            if (item.InvType != YisoItem.InventoryType.EQUIP && item.Quantity > 0 && item.Quantity != count) {
                count = item.Quantity;
            }

            InventoryUnits[item.InvType].TryAddItem(item, count, out var argsList);

            foreach (var args in argsList) {
                RaiseInventoryEvent(args);
            }
        }

        public bool TryAddItem(int itemId, int count = 1) {
            var dataService = YisoServiceProvider.Instance.Get<IYisoItemService>();
            var item = dataService.GetItemOrElseThrow(itemId);
            var newItem = item.DeepCopy();
            newItem.Quantity = item.InvType == YisoItem.InventoryType.EQUIP ? 1 : count;

            var isAdded = InventoryUnits[newItem.InvType].TryAddItem(newItem, count, out var argList);
            foreach (var args in argList) RaiseInventoryEvent(args);
            return isAdded;
        }

        public bool TryRemoveItem(int itemId, int count) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (!unit.TryFindBy(i => i.Id == itemId, out var item)) continue;
                var result = unit.TryRemoveItem(item.Position, count, out _, out var args);
                RaiseInventoryEvent(args);
                return result;
            }

            return false;
        }

        public void RemoveItem(int itemId, int count) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (unit.TryFindBy(i => i.Id == itemId, out var item)) {
                    if (unit.TryRemoveItem(item.Position, count, out _, out var args))
                        RaiseInventoryEvent(args);
                    break;
                }
            }
        }

        public void RemoveItemAll(int itemId) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (unit.TryFindBy(i => i.Id == itemId, out var item)) {
                    if (unit.TryRemoveItem(item.Position, item.Quantity, out _, out var args))
                        RaiseInventoryEvent(args);
                    break;
                }
            }
        }


        public bool CanAdd(YisoItem item) => InventoryUnits[item.InvType].CanAdd();

        public bool CanBuy(YisoItem item, double price, out YisoPlayerInventoryReasons reason) {
            reason = YisoPlayerInventoryReasons.NONE;
            var invType = item.InvType;
            var unit = InventoryUnits[invType];

            if (!unit.CanAdd()) {
                reason = YisoPlayerInventoryReasons.INVENTORY_SLOT_FULL;
                return false;
            }

            if (money - price < 0) {
                reason = YisoPlayerInventoryReasons.MONEY_NOT_ENOUGH;
                return false;
            }

            return true;
        }

        public void CheckStageQuestItemExist(int stageId) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                foreach (var (_, item) in InventoryUnits[type].ItemDict) {
                    player.QuestModule.UpdateQuestRequirement(stageId, YisoQuestRequirement.Types.ITEM, item.Id);
                }
            }
        }

        public void CheckStageQuestItemExist(IReadOnlyList<int> stageIds) {
            foreach (var stageId in stageIds) CheckStageQuestItemExist(stageId);
        }

        public YisoEquipItem GetEquippedWeapon() {
            if (!EquippedItemPositions.TryGetValue(YisoEquipSlots.WEAPON, out var position)) return null;
            return (YisoEquipItem)InventoryUnits[YisoItem.InventoryType.EQUIP][position];
        }

        public bool ExistItem(int itemId, int count = -1) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (!unit.TryFindBy(i => i.Id == itemId, out var item)) continue;
                
                if (count > 0) return item.Quantity >= count;
                return true;
            }

            return false;
        }

        public bool TryFindItemById(int itemId, out YisoItem item) {
            item = null;
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (!unit.TryFindBy(i => i.Id == itemId, out item)) continue;
                return true;
            }

            return false;
        }

        public T GetItem<T>(YisoItem.InventoryType type, int position) where T : YisoItem =>
            InventoryUnits[type][position] as T;

        #endregion
        
        #region DATA LOAD, RESET ETC

        public void SaveData(ref YisoPlayerData data) {
            foreach (var unit in InventoryUnits.Values) {
                foreach (var (position, item) in unit.ItemDict) {
                    var saveItem = item.Save();
                    data.inventoryData.items.Add(saveItem);
                }
            }
        }


        public void LoadData(YisoPlayerData data) {
            var dataService = YisoServiceProvider.Instance.Get<IYisoItemService>();
            foreach (var itemData in data.inventoryData.items) {
                YisoItemSO so = null;
                var item = dataService.GetItemOrElseThrow(itemData.itemId);
                if (item.InvType == YisoItem.InventoryType.EQUIP) {
                    so = dataService.GetItemSOOrElseThrow(itemData.itemId);
                }

                item.Load(itemData, so);
                item = item.DeepCopy();
                var position = itemData.position;
                InventoryUnits[item.InvType].LoadItem(item, position);
                
                if (item is not YisoEquipItem equipItem) continue;
                if (!equipItem.Equipped) continue;
                EquippedItemPositions[equipItem.Slot] = equipItem.Position;
            }

            // money = data.inventoryData.money;
            AddMoney(9999999);

            var tempItemId = 40000000;

            var tempItem = dataService.GetItemOrElseThrow(tempItemId);
            tempItem.Quantity = 5;
            
            AddItem(tempItem);
            return;


            var arrowId = 20000101;
            var arrow = dataService.GetItemOrElseThrow(arrowId);
            
            arrow.Quantity = 100;
            AddItem(arrow);

            var bowId = 30000059;
            var bow = dataService.GetItemOrElseThrow(bowId);

            AddItem(bow);
        }
        
        #endregion
        
        #region EVENT RAISE

        private void RaiseMoneyChanged() {
            OnMoneyChanged?.Invoke(money);
        }

        private void RaiseInventoryEvent(YisoPlayerInventoryEventArgs args) {
            OnInventoryEvent?.Invoke(args);
        }
        
        #endregion
    }
}