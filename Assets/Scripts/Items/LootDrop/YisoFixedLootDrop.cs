using System;
using System.Collections.Generic;
using Core.Domain.Drop;
using Core.Domain.Item;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items.LootDrop {
    /// <summary>
    /// 지정된 아이템을 드랍할때
    /// </summary>
    [AddComponentMenu("Yiso/Items/Loot/Fixed Loot Drop")]
    public class YisoFixedLootDrop : YisoLootDrop {
        [Title("Settings")] public List<DropItem> dropItems;
        protected int currentIndex = 0;

        protected override void Initialization() {
            base.Initialization();
            currentIndex = 0;
        }

        protected override void SpawnOneLoot() {
            base.SpawnOneLoot();
            if (limitedLootQuantity && remainingQuantity <= 0) return;
            if (currentIndex >= dropItems.Count) return;
            var currentDropItem = dropItems[currentIndex];

            switch (currentDropItem.itemType) {
                case YisoDropItem.Types.MONEY:
                    for (var i = 0; i < currentDropItem.count; i++) {
                        var min = Mathf.Min(currentDropItem.minMoney, currentDropItem.maxMoney);
                        var max = Mathf.Max(currentDropItem.minMoney, currentDropItem.maxMoney);
                        var randomMoney = Random.Range(min, max);
                        SpawnMoney(randomMoney);
                    }

                    break;
                case YisoDropItem.Types.ITEM:
                    SpawnOneItem(new YisoDropItem(currentDropItem.itemSO.CreateItem(), currentDropItem.count));
                    break;
            }

            currentIndex++;
        }

        protected override int GetDropItemQuantity() {
            return dropItems?.Count ?? 0;
        }

        [Serializable]
        public class DropItem {
            public YisoDropItem.Types itemType;

            [ShowIf("itemType", YisoDropItem.Types.MONEY)]
            public int minMoney = 100;

            [ShowIf("itemType", YisoDropItem.Types.MONEY)]
            public int maxMoney = 200;

            [ShowIf("itemType", YisoDropItem.Types.ITEM)]
            public YisoItemSO itemSO;

            public int count = 1;
        }
    }
}