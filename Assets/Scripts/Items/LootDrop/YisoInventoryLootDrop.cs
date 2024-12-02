using Core.Domain.Drop;
using Core.Domain.Item;
using Spawn;
using UnityEngine;

namespace Items.LootDrop {
    /// <summary>
    /// 플레이어가 인벤토리에 있는 아이템 버릴때 
    /// </summary>
    [AddComponentMenu("Yiso/Items/Loot/Inventory Loot Drop")]
    public class YisoInventoryLootDrop : YisoLootDrop {
        protected override void Initialization() {
            base.Initialization();
            poolLoot = true;
            limitedLootQuantity = false;
            spawnLootOnDeath = false;
            spawnLootOnDamage = false;
            spawnInAreaProperties = new YisoObjectSpawnInAreaProperties();
        }

        public virtual void DropOneItem(YisoItem item) {
            if (item == null) return;
            SpawnOneItem(new YisoDropItem(item, 1));
        }

        public virtual void DropMoney(double amount) {
            SpawnMoney(amount);
        }
    }
}