using Core.Domain.Drop;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items.LootDrop {
    /// <summary>
    /// Loot Table에 등록된 아이템들을 확률에 맞게 드롭 (즉 여러개 드롭할때)
    /// </summary>
    [AddComponentMenu("Yiso/Items/Loot/LootTableDrop")]
    public class YisoLootTableDrop : YisoLootDrop {
        [Title("Loot Table")] public LootTable lootTable;

        [Title("Quantity")] [MinValue(0)] public int minSpawnQuantity = 1;
        [MinValue(0)] public int maxSpawnQuantity = 2;

        protected override void Awake() {
            InitializeLootTable();
            base.Awake();
        }

        protected virtual void InitializeLootTable() {
            lootTable.ComputeWeights();
        }

        protected override void SpawnOneLoot() {
            var objectToLoot = lootTable.GetLoot();
            if (limitedLootQuantity && remainingQuantity <= 0) return;
            double totalMoney = 0;

            switch (objectToLoot.itemType) {
                case YisoDropItem.Types.MONEY:
                    var min = Mathf.Min(objectToLoot.minMoney, objectToLoot.maxMoney);
                    var max = Mathf.Max(objectToLoot.minMoney, objectToLoot.maxMoney);
                    var randomMoney = Random.Range(min, max);
                    totalMoney += randomMoney;
                    break;
                case YisoDropItem.Types.ITEM:
                    SpawnOneItem(new YisoDropItem(objectToLoot.itemSO.CreateItem(), 1));
                    break;
            }

            if (totalMoney > 0) {
                SpawnMoney(totalMoney);
            }
        }

        protected override int GetDropItemQuantity() {
            var min = Mathf.Min(minSpawnQuantity, maxSpawnQuantity);
            var max = Mathf.Max(minSpawnQuantity, maxSpawnQuantity);
            return Random.Range(min, max + 1);
        }
    }
}