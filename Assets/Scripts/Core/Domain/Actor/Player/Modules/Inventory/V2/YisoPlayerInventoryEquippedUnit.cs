using System;
using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Domain.Data;
using Core.Domain.Item;
using Core.Domain.Types;
using Core.Logger;
using Utils.Extensions;

namespace Core.Domain.Actor.Player.Modules.Inventory.V2 {
    public sealed class YisoPlayerInventoryEquippedUnit {
        private readonly Dictionary<YisoEquipSlots, Item> items = new();
        private readonly Dictionary<YisoWeapon.AttackType, Item> weaponItems = new();

        public YisoWeapon.AttackType CurrentWeapon { get; private set; } = YisoWeapon.AttackType.Slash;

        public YisoPlayerInventoryEquippedUnit() {
            foreach (var slot in EnumExtensions.Values<YisoEquipSlots>()) {
                items[slot] = new Item();
            }

            foreach (var slot in EnumExtensions.Values<YisoWeapon.AttackType>().Where(type => type != YisoWeapon.AttackType.None)) {
                weaponItems[slot] = new Item();
            }
        }

        public YisoEquipItem SwitchWeapon(out YisoWeapon.AttackType nextWeapon) {
            nextWeapon = CurrentWeapon.GetNextType();
            var item = weaponItems[nextWeapon];
            CurrentWeapon = nextWeapon;
            return item.EquipItem;
        }

        public YisoEquipItem SwitchExistWeapon(out YisoWeapon.AttackType nextWeapon) {
            nextWeapon = CurrentWeapon.GetNextType();
            YisoEquipItem item = null;
            var count = 0;
            do {
                if (count == 10) throw new Exception("No exist weapon!");
                nextWeapon = nextWeapon.GetNextType();
                item = weaponItems[nextWeapon].EquipItem;
                count++;
            } while (item == null);

            return item;
        }

        public YisoEquipItem GetWeapon(YisoWeapon.AttackType type = YisoWeapon.AttackType.None) {
            if (type == YisoWeapon.AttackType.None) type = CurrentWeapon;
            var item = weaponItems[type];
            return item.EquipItem;
        }

        public bool TryGetItem(YisoEquipSlots slot, out YisoEquipItem equipItem) {
            equipItem = null;
            var item = items[slot];
            if (!item.Equipped) return false;
            equipItem = item.EquipItem;
            return true;
        }

        public void SetItem(YisoEquipItem item) {
            var slot = item.Slot;
            if (slot == YisoEquipSlots.WEAPON) {
                var attackType = item.AttackType;
                weaponItems[attackType].EquipItem = item;
                return;
            }
            items[slot].EquipItem = item;
        }

        public void UnSetItem(YisoEquipItem item) {
            var slot = item.Slot;
            if (slot == YisoEquipSlots.WEAPON) {
                var attackType = item.AttackType;
                weaponItems[attackType].EquipItem = null;
                return;
            }
            items[slot].EquipItem = null;
        }

        public IEnumerable<YisoEquipItem> GetEquippedItems() =>
            items.Values.Where(item => item.Equipped).Select(item => item.EquipItem);

        public void Save(ref YisoPlayerData data) {
            foreach (var (slot, item) in items) {
                if (!item.Equipped) continue;
                data.inventoryData.items.Add(item.EquipItem.Save());
                data.inventoryData.equippedData.slotItems[(int) slot] = item.EquipItem.ObjectId;
            }

            foreach (var (weapon, item) in weaponItems) {
                if (!item.Equipped) continue;
                data.inventoryData.items.Add(item.EquipItem.Save());
                data.inventoryData.equippedData.weaponItems[(int)weapon] = item.EquipItem.ObjectId;
            }
        }

        public void Load(YisoEquipItem item) {
            if (item.Slot != YisoEquipSlots.WEAPON) {
                items[item.Slot].EquipItem = item;
                return;
            }

            weaponItems[item.AttackType].EquipItem = item;
        }

        public void Load(int currentWeaponType) {
            CurrentWeapon = currentWeaponType.ToEnum<YisoWeapon.AttackType>();
        }
        

        public void Reset() {
            foreach (var slot in EnumExtensions.Values<YisoEquipSlots>()) {
                items[slot].EquipItem = null;
            }
            
            foreach (var slot in EnumExtensions.Values<YisoWeapon.AttackType>().Where(type => type != YisoWeapon.AttackType.None)) {
                weaponItems[slot].EquipItem = null;
            }
        }

        public class Item {
            public YisoEquipItem EquipItem { get; set; } = null;
            public int ItemId => EquipItem?.Id ?? -1;
            public string ObjectId => EquipItem?.ObjectId ?? string.Empty;

            public bool Equipped => EquipItem != null;
        }
    }
}