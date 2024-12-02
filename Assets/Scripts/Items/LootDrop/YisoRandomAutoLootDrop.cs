using System;
using System.Collections;
using System.Collections.Generic;
using Core.Domain.Item;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Data.Item;
using Core.Service.Log;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items.LootDrop {
    [AddComponentMenu("Yiso/Items/Loot/Random Loot Drop")]
    public class YisoRandomAutoLootDrop: YisoAutoLootDrop {
        [Title("Random Item Settings")]
        public bool useRankRange = false;
        [ShowIf("@!useRankRange")] public YisoEquipRanks itemRank;
        [ShowIf("useRankRange")] public YisoEquipRanks itemRankFrom;
        [ShowIf("useRankRange")] public YisoEquipRanks itemRankTo;
        public int itemCount = 1;
        
        protected override IEnumerator SpawnLootCo() {
            yield return new WaitForSeconds(delay);
            var dropItems = GetRandomDropItems();
            if (dropItems == null || dropItems.Count == 0) yield break;
            foreach (var dropItem in dropItems) {
                SpawnOneItem(dropItem);
            }
            lootFeedback?.PlayFeedbacks();
        }
        
        protected virtual List<YisoItem> GetRandomDropItems() {
            var dropItems = new List<YisoItem>();
            for (var i = 0; i < itemCount; i++) {
                dropItems.Add(useRankRange
                    ? YisoServiceProvider.Instance.Get<IYisoItemService>().CreateRandomEquip(GetRandomItemRankInRange())
                    : YisoServiceProvider.Instance.Get<IYisoItemService>().CreateRandomEquip(itemRank));
            }
            return dropItems;
        }

        private YisoEquipRanks GetRandomItemRankInRange() {
            var min = Mathf.Min((int) itemRankTo, (int) itemRankFrom);
            var max = Mathf.Max((int) itemRankTo, (int) itemRankFrom);
            if (min == max) return itemRankTo;

            var rankIndex = Random.Range(min, max + 1);
            
            if (Enum.IsDefined(typeof(YisoEquipRanks), rankIndex)) {
                return (YisoEquipRanks) rankIndex;
            }
            
            var logger = YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoRandomAutoLootDrop>();
            logger.Warn($"[YisoRandomAutoLootDrop] Invalid rank index: {rankIndex}. Returning default rank: {itemRankFrom}.");
            return itemRankFrom;
        }
    }
}