using System;
using System.Collections.Generic;
using Character.Weapon;
using Core.Domain.Item;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Actor.Player.SO {
    [CreateAssetMenu(fileName = "InventoryItems", menuName = "Yiso/Player/Inventory Items")]
    public class YisoPlayerInventoryItemsSO : ScriptableObject {
        public List<Item> items;
        [Serializable]
        public class Item {
            public bool random;
            [ShowIf("random")] public bool randomSlot = true;
            [ShowIf("@this.random && !this.randomSlot")] public YisoEquipSlots slots;
            [ShowIf("@this.random && !this.randomSlot && this.slots == YisoEquipSlots.WEAPON")] public YisoWeapon.AttackType attackType;
            [ShowIf("random")] public bool randomFaction = true;
            [ShowIf("@this.random && !this.randomFaction")] public YisoEquipFactions faction;
            [ShowIf("random")] public bool randomRank = true;
            [ShowIf("@this.random && !this.randomRank")] public YisoEquipRanks rank;
            [HideIf("random")]
            public YisoEquipItemSO itemSO;

            public bool RandomAll => randomSlot && randomFaction && randomRank;
        }
    }
}