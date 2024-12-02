using System;
using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Domain.Actor.Player.Modules.Base;
using Core.Domain.Actor.Player.Modules.Inventory.Reinforce;
using Core.Domain.Actor.Player.SO;
using Core.Domain.Data;
using Core.Domain.Effect;
using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Quest;
using Core.Domain.Types;
using Core.Logger;
using Core.Service;
using Core.Service.CoolDown;
using Core.Service.Data;
using Core.Service.Data.Item;
using Core.Service.Effect;
using Core.Service.Log;
using Core.Service.Stage;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace Core.Domain.Actor.Player.Modules.Inventory.V2 {
    public class YisoPlayerInventoryV2Module : YisoPlayerBaseModule {
        public event UnityAction<double> OnMoneyChanged;
        public event UnityAction<YisoPlayerInventoryEventArgs> OnInventoryEvent;

        public YisoPlayerInventoryEquippedUnit EquippedUnit { get; } = new();
        public Dictionary<YisoItem.InventoryType, YisoPlayerInventoryV2Unit> InventoryUnits { get; } = new();

        private double money = 0;

        public double Money {
            get => money;
            set {
                money = value;
                player.SaveData();
                OnMoneyChanged?.Invoke(value);
            }
        }

        public void AddMoney(double value) {
            Money += value;
        }

        public YisoPlayerInventoryV2Module(YisoPlayer player) : base(player) {
            ResetData();
        }

        #region INVENTORY FEATURES

        public bool IsEquipped(YisoEquipSlots slot, int id) {
            if (!EquippedUnit.TryGetItem(slot, out var item)) return false;
            return item.Id == id;
        }

        public int GetSlotLimit() => InventoryUnits[YisoItem.InventoryType.EQUIP].SlotLimit;

        public YisoEquipItem GetEquippedWeapon() => !EquippedUnit.TryGetItem(YisoEquipSlots.WEAPON, out var equipItem) ? null : equipItem;

        public YisoWeapon.AttackType GetCurrentEquippedWeaponType() => EquippedUnit.CurrentWeapon;

        public YisoEquipItem GetCurrentEquippedWeaponItem() => EquippedUnit.GetWeapon();

        public bool TryGetWeapon(out YisoEquipItem item) {
            item = EquippedUnit.GetWeapon();
            return item != null;
        }

        public void SwitchWeapon() {
            var currentWeaponType = EquippedUnit.CurrentWeapon;
            var weapon = EquippedUnit.SwitchWeapon(out var nextWeapon);
            var cr = player.StatModule.WeaponCombatRatings[nextWeapon];
            RaiseInventoryEvent(new YisoPlayerInventorySwitchWeaponEventArgs(weapon, cr, currentWeaponType, nextWeapon));
            player.SaveData();
        }

        public void SwitchExistWeapon() {
            var currentWeaponType = EquippedUnit.CurrentWeapon;
            var weapon = EquippedUnit.SwitchExistWeapon(out var nextWeapon);
            var cr = player.StatModule.WeaponCombatRatings[nextWeapon];
            RaiseInventoryEvent(new YisoPlayerInventorySwitchWeaponEventArgs(weapon, cr, currentWeaponType, nextWeapon));
        }

        public void EquipItem(YisoEquipItem item) {
            var slot = item.Slot;
            var position = item.Position;
            
            InventoryUnits[YisoItem.InventoryType.EQUIP].RemoveItem(position, 1, out _, out var args);
            RaiseInventoryEvent(args);
            
            if (EquippedUnit.TryGetItem(slot, out var unEquipItem)) {
                unEquipItem.Equipped = false;
                InventoryUnits[YisoItem.InventoryType.EQUIP].TryAddItem(unEquipItem, 1, out var args2);
                RaiseInventoryEvent(args2);
            }

            item.Position = -1;
            item.Equipped = true;
            EquippedUnit.SetItem(item);
            player.StatModule.CalculateCombatRatingAllWeapons(); 
            RaiseInventoryEvent(new YisoPlayerInventoryEquipEventArgs(-1, slot, item.AttackType));
            player.SaveData();
        }

        public void UnEquipItem(YisoEquipItem item) {
            var slot = item.Slot;
            if (!InventoryUnits[YisoItem.InventoryType.EQUIP].TryAddItem(item, 1, out var args)) {
                return;
            }

            RaiseInventoryEvent(args);
            
            item.Equipped = false;
            EquippedUnit.UnSetItem(item);
            player.StatModule.CalculateCombatRatingAllWeapons();
            RaiseInventoryEvent(new YisoPlayerInventoryUnEquipEventArgs(item.Position, slot, item.AttackType));
            player.SaveData();
        }
        
        public void DropItem(YisoItem item, int count = 1) {
            var type = item.InvType;
            var position = item.Position;
            InventoryUnits[type].RemoveItem(position, count, out var removedItem, out var args);
            RaiseInventoryEvent(args);
            RaiseInventoryEvent(new YisoPlayerInventoryDropEventArgs(type, removedItem.DeepCopy(), position));
            player.SaveData();
        }

        public void UseItem(YisoUseItem item) {
            UseItem(item.ObjectId);
            player.SaveData();
        }

        private void UseItem(string objectId) {
            if (!InventoryUnits[YisoItem.InventoryType.USE].TryGetByItemId(objectId, out var item)) throw new Exception($"Item({objectId}) not exists!");
            var effectService = YisoServiceProvider.Instance.Get<IYisoEffectService>();
            var cooldownService = YisoServiceProvider.Instance.Get<IYisoCoolDownService>();
            if (cooldownService.ExistCoolDown(item.Id)) {
                // TODO(ON COOLDOWN)
                return;
            }

            var useItem = (YisoUseItem) item;
            useItem.Quantity -= 1;
            RaiseInventoryEvent(new YisoPlayerInventoryCountEventArgs(YisoItem.InventoryType.USE, useItem.Position, item.Id, useItem.Quantity));
            effectService.CreateEffect(player, useItem);
            player.SaveData();
        }

        public bool TryUseArrow(int count, out YisoPlayerInventoryReasons reason) {
            reason = YisoPlayerInventoryReasons.NONE;

            if (!InventoryUnits[YisoItem.InventoryType.USE].TryGetBy<YisoUseItem>(i => i.IsArrow, out var item)) {
                reason = YisoPlayerInventoryReasons.ARROW_NOT_EXIST;
                return false;
            }

            if (item.Quantity < count) {
                reason = YisoPlayerInventoryReasons.ARROW_NOT_ENOUGH;
                return false;
            }

            item.Quantity -= count;
            var position = item.Position;
            RaiseInventoryEvent(new YisoPlayerInventoryCountEventArgs(
                YisoItem.InventoryType.USE, position, item.Id, item.Quantity));
            player.SaveData();
            return true;
        }
        
        public int GetArrowCount() => InventoryUnits[YisoItem.InventoryType.USE].Items
                .Cast<YisoUseItem>()
                .Where(item => item.IsArrow)
                .Sum(item => item.Quantity);

        public void ReOrderItems(YisoItem.InventoryType type) {
            InventoryUnits[type].ReOrder();
            RaiseInventoryEvent(new YisoPlayerInventoryReOrderedEventArgs(type));
            player.SaveData();
        }
        
        #endregion
        
        #region STORE FEATURES

        public void SellItem(YisoItem item, int count, double price) {
            var type = item.InvType;
            var position = item.Position;
            InventoryUnits[type].RemoveItem(position, count, out _, out var args);
            Money += (count * price);
            RaiseInventoryEvent(args);
            player.SaveData();
        }

        public void SellItems(YisoItem.InventoryType type, List<string> objectIds, double price) {
            foreach (var id in objectIds) {
                if (!InventoryUnits[type].TryGetByItemId(id, out var item))
                    throw new Exception($"Item(id={id}) not found!");
                var position = item.Position;
                InventoryUnits[type].RemoveItem(position, item.Quantity, out _, out var args);
                RaiseInventoryEvent(args);
            }

            Money += price;
            player.SaveData();
        }

        public void PurchaseItem(YisoItem item, int count, double price) {
            AddItem(item, count);
            Money -= price;
            player.SaveData();
        }

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
        
        #endregion
        
        #region STORAGE FEATURES

        public void StoreItem(YisoItem item, int count) {
            // TODO(STORAGE FEATURE)
        }
        
        #endregion
        
        #region ITEM FEATUERS

        public void AddItem(YisoItem item, int count = 1) {
            item = item.DeepCopy();
            if (item.InvType != YisoItem.InventoryType.EQUIP && item.Quantity > 0 && item.Quantity != count) {
                count = item.Quantity;
            }

            InventoryUnits[item.InvType].TryAddItem(item, count, out var args);
            RaiseInventoryEvent(args);
        }

        public bool TryAddItem(int itemId, int count = 1) {
            var dataService = GetService<IYisoItemService>();
            var item = dataService.GetItemOrElseThrow(itemId);
            var newItem = item.DeepCopy();
            newItem.Quantity = item.InvType == YisoItem.InventoryType.EQUIP ? 1 : count;

            var isAdded = InventoryUnits[newItem.InvType].TryAddItem(newItem, count, out var args);
            RaiseInventoryEvent(args);
            return isAdded;
        }

        public bool TryRemoveItem(string objectId, int count) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (!unit.TryGetByItemId(objectId, out var item)) continue;
                unit.RemoveItem(item.Position, count, out _, out var args);
                RaiseInventoryEvent(args);
                return true;
            }

            return false;
        }

        public bool TryRemoveItem(int itemId, int count) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (!unit.TryGetByItemId(itemId, out var item)) continue;
                unit.RemoveItem(item.Position, count, out _, out var args);
                RaiseInventoryEvent(args);
                return true;
            }

            return false;
        }

        public void RemoveItem(string objectId, int count) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (!unit.TryGetByItemId(objectId, out var item)) continue;
                if (count == -1) count = item.Quantity;
                unit.RemoveItem(item.Position, count, out _, out var args);
                RaiseInventoryEvent(args);
                break;
            }
        }

        public void RemoveItem(int itemId, int count) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (!unit.TryGetByItemId(itemId, out var item)) continue;
                if (count == -1) count = item.Quantity;
                unit.RemoveItem(item.Position, count, out _, out var args);
                RaiseInventoryEvent(args);
                break;
            }
        }

        public void RemoveItemAll(string objectId) {
            RemoveItem(objectId, -1);
        }

        public void RemoveItemAll(int itemId) {
            RemoveItem(itemId, -1);
        }

        public bool CanAdd(YisoItem item) => InventoryUnits[item.InvType].CanAdd();

        public bool ExistItem(string objectId, int count = -1) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (!unit.TryGetByItemId(objectId, out var item)) continue;
                
                if (count > 0) return item.Quantity >= count;
                return true;
            }

            return false;
        }

        public bool ExistItem(int itemId, int count = -1) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (!unit.TryGetByItemId(itemId, out var item)) continue;
                
                if (count > 0) return item.Quantity >= count;
                return true;
            }

            return false;
        }
        
        public bool TryFindItemById(string objectId, out YisoItem item) {
            item = null;

            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (!unit.TryGetByItemId(objectId, out item)) continue;
                return true;
            }
            
            return false;
        }

        public bool TryFindItemById(int itemId, out YisoItem item) {
            item = null;

            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                var unit = InventoryUnits[type];
                if (!unit.TryGetByItemId(itemId, out item)) continue;
                return true;
            }
            
            return false;
        }

        public T GetItem<T>(YisoItem.InventoryType type, int position) where T : YisoItem =>
            InventoryUnits[type][position] as T;
        
        #endregion
        
        #region QUEST FEATUERS

        public void CheckStageQuestItemExist(IReadOnlyList<int> stageIds) {
            foreach (var stageId in stageIds) CheckStageQuestItemExist(stageId); 
        }

        public void CheckStageQuestItemExist(int stageId) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                foreach (var item in InventoryUnits[type].Items) {
                    player.QuestModule.UpdateQuestRequirement(stageId, YisoQuestRequirement.Types.ITEM, item.Id);
                }
            }
        }
        
        #endregion
        
        #region REINFORCE FEATURES

        public YisoPlayerInventoryNormalReinforceResult GetReinforceNormalResult(YisoEquipItem item) => null;

        public YisoPlayerInventoryPotentialReinforceResult GetReinforcePotentialResult(YisoEquipItem item) => null;
        
        public void Reinforce(YisoPlayerInventoryReinforceResult result, YisoEquipItem item) { }
        
        #endregion
        
        #region DATA FEATURES

        public void SaveData(ref YisoPlayerData data) {
            data.inventoryData.money = Money;
            
            foreach (var unit in InventoryUnits.Values) {
                foreach (var item in unit.Items) {
                    var saveItem = item.Save();
                    data.inventoryData.items.Add(saveItem);
                }
            }

            data.inventoryData.equippedData.currentWeapon = (int) GetCurrentEquippedWeaponType();
            EquippedUnit.Save(ref data);
        }

        public void LoadData(YisoPlayerData data, YisoPlayerInventoryItemsSO inventoryItemsSO) {
            var itemService = GetService<IYisoItemService>();

            var playerItems = new Dictionary<string, YisoItem>();
            
            foreach (var itemData in data.inventoryData.items) {
                YisoItemSO so = null;
                var item = itemService.GetItemOrElseThrow(itemData.itemId);
                if (item.InvType == YisoItem.InventoryType.EQUIP) {
                    so = itemService.GetItemSOOrElseThrow(itemData.itemId);
                }
                
                item.Load(itemData, so);
                item = item.DeepCopy();

                item.ObjectId = itemData.objectId;
                item.Position = itemData.position;


                playerItems[item.ObjectId] = item;
            }

            foreach (var (objectId, item) in playerItems) {
                if (item.Position == -1) {
                    if (item is YisoEquipItem equipItem) {
                        data.inventoryData.equippedData.VerifyId(objectId);
                        EquippedUnit.Load(equipItem);
                    }
                } else InventoryUnits[item.InvType].Load(item, item.Position);
            }
            
            EquippedUnit.Load(data.inventoryData.equippedData.currentWeapon);
            
            if (!data.initData) {
                money += data.inventoryData.money;
                return;
            }
            
            var logger = YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoPlayerInventoryV2Module>();
            logger.Debug("New player detected. give high-end equips to player.");
            var weapon = itemService.CreateRandomWeapon(YisoWeapon.AttackType.None, YisoEquipRanks.S);
            AddItem(weapon);
            data.initData = false;

            Money += 9999999;

            var arrowItemId = 20000101;
            var arrowItem = itemService.GetItemOrElseThrow(arrowItemId);

            arrowItem.Quantity = 100;
            
            AddItem(arrowItem);

            var portionIds = new[] { 20000001, 20000002, 20000003, 20000004, 20000005 };
            foreach (var id in portionIds) {
                var portionItem = itemService.GetItemOrElseThrow(id);
                portionItem.Quantity = 50;
                AddItem(portionItem);
            }
            
            if (inventoryItemsSO == null) return;

            var items = YisoServiceProvider.Instance.Get<IYisoItemService>().CreateItemFromSO(inventoryItemsSO);
            foreach (var item in items) {
                AddItem(item);
            }
        }
        
        public void ResetData() {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                InventoryUnits[type] = new YisoPlayerInventoryV2Unit(type);
            }
            
            EquippedUnit.Reset();
            money = 0;
        }
        
        #endregion
        
        #region EVENT

        public void RaiseInventoryEvent(YisoPlayerInventoryEventArgs args) {
            OnInventoryEvent?.Invoke(args);
        }
        
        #endregion
        

    }
}