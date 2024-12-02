using System;
using System.Collections.Generic;
using Core.Domain.Drop;
using Core.Domain.Item;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items {
    [Serializable]
    public class Loot {
        public GameObject loot;
        public YisoDropItem.Types itemType = YisoDropItem.Types.MONEY;

        [ShowIf("itemType", YisoDropItem.Types.MONEY)]
        public int minMoney = 100;

        [ShowIf("itemType", YisoDropItem.Types.MONEY)]
        public int maxMoney = 200;

        [ShowIf("itemType", YisoDropItem.Types.ITEM)]
        public YisoItemSO itemSO;

        public float weight = 1f;
        [ReadOnly] public float chancePercentage;
        public float RangeFrom { get; set; }
        public float RangeTo { get; set; }
    }

    [Serializable]
    public class LootTable {
        public List<Loot> objectsToLoot;

        [ReadOnly] public float weightsTotal;

        protected float currentMaximumWeight = 0f;
        protected bool weightsComputed = false;

        public virtual void ComputeWeights() {
            if (objectsToLoot == null || objectsToLoot.Count == 0) return;
            currentMaximumWeight = 0f;
            foreach (var lootDropItem in objectsToLoot) {
                if (lootDropItem.weight >= 0f) {
                    lootDropItem.RangeFrom = currentMaximumWeight;
                    currentMaximumWeight += lootDropItem.weight - 1;
                    lootDropItem.RangeTo = currentMaximumWeight;
                    currentMaximumWeight++;
                }
                else {
                    lootDropItem.weight = 0f;
                }
            }

            weightsTotal = currentMaximumWeight;

            foreach (var lootDropItem in objectsToLoot) {
                lootDropItem.chancePercentage = lootDropItem.weight / weightsTotal * 100;
            }

            weightsComputed = true;
        }

        public virtual Loot GetLoot() {
            if (objectsToLoot == null || objectsToLoot.Count == 0) return null;
            if (!weightsComputed) ComputeWeights();
            var index = Random.Range(0, weightsTotal);
            foreach (var lootDropItem in objectsToLoot) {
                if (index >= lootDropItem.RangeFrom && index <= lootDropItem.RangeTo) {
                    return lootDropItem;
                }
            }

            return null;
        }
    }
}