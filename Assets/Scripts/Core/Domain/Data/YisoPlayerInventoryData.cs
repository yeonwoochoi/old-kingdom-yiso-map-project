using System;
using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Domain.Item;
using Core.Domain.Types;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Utils.Extensions;

namespace Core.Domain.Data {
    [Serializable]
    public class YisoPlayerInventoryData {
        public int slotCount = 20;
        public double money = 0;
        public List<YisoPlayerItemData> items = new();
        public YisoPlayerEquippedItemData equippedData = new();

        public void Reset() {
            money = 0;
            slotCount = 0;
            items.Clear();
        }
    }

    [Serializable]
    public class 
        YisoPlayerStorageData {
        public int slotCount = 15;
        public double money;
        public List<YisoPlayerItemData> items = new();
    }

    [Serializable]
    public class YisoPlayerItemData {
        public int id;
        public int itemId;
        public int position;
        public int quantity;
        public string objectId;
    }

    [Serializable]
    public class YisoPlayerEquipItemData : YisoPlayerItemData {
        public int upgradableSlots;
        public int upgradedSlots;
        public YisoEquipRanks rank;
        public YisoEquipFactions faction;
        public YisoEquipSubTypes subType;
        public YisoEquipSlots slot;
        public double combatRating;
        public YisoPlayerEquipPotentialsData potentials;
        public bool equipped = false;
    }

    [Serializable]
    public class YisoPlayerEquippedItemData {
        public Dictionary<int, string> slotItems = new();
        public Dictionary<int, string> weaponItems = new();
        public int currentWeapon = 3;

        public void VerifyId(string objectId) {
            foreach (var (key, id) in slotItems) {
                if (objectId != id) continue;
                return;
            }

            foreach (var (key, id) in weaponItems) {
                if (objectId != id) continue;
                return;
            }

            throw new Exception($"Cannot find {objectId} equipped!");
        }

        public int FindSlotByObjectId(string objectId) {
            foreach (var (key, id) in slotItems) {
                if (objectId != id) continue;
                return key;
            }

            throw new Exception($"Cannot find {objectId} equipped!");
        }

        public YisoPlayerEquippedItemData() {
            Reset();
        }

        public void Reset() {
            foreach (var slot in EnumExtensions.Values<YisoEquipSlots>()) {
                slotItems[(int)slot] = string.Empty;
            }

            foreach (var weapon in EnumExtensions.Values<YisoWeapon.AttackType>()) {
                weaponItems[(int)weapon] = string.Empty;
            }
        }
    }

    [Serializable]
    public sealed class YisoPlayerEquipPotentialsData {
        public YisoPlayerEquipPotentialData potential1;
        public YisoPlayerEquipPotentialData potential2;
        public YisoPlayerEquipPotentialData potential3;
    }

    [Serializable]
    public sealed class YisoPlayerEquipPotentialData {
        public YisoBuffEffectTypes type;
        public int value;
    }
}