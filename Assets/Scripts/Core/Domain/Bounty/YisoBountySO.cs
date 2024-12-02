using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Actor.Enemy;
using Core.Domain.Actor.Player;
using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Domain.Map;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Data;
using Core.Service.Data.Item;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Bounty {
    [CreateAssetMenu(fileName = "Bounty", menuName = "Yiso/Bounty/Bounty")]
    public class YisoBountySO : ScriptableObject {
        public int id;
        [PreviewField] public Sprite targetImage;
        public YisoEnemySO target;
        public YisoLocale targetName;
        public YisoLocale description;
        public bool useTimeLimit = true;
        [ShowIf("useTimeLimit")]
        public int timeLimit = 60;

        public double bountyReward;
        
        public List<YisoItemSO> rewards;
        [Required]
        public YisoMapSO mapSO;
        [Required]
        public GameObject enemySpawnerPrefab; 

        public YisoBounty CreateBounty() => new(this);
    }

    public class YisoBounty {
        public int Id { get; }
        public Sprite TargetIcon { get; }

        public int TargetId { get; }
        
        private readonly YisoLocale targetName;
        private readonly YisoLocale description;
        
        public bool UseTimeLimit { get; }
        public int TimeLimit { get; }

        public List<int> RewardItemIds { get; }
        public List<ItemUI> ItemUIs { get; }
        public double BountyReward { get; }
        public int MapId { get; }
        public GameObject EnemySpawnerPrefab { get; }

        internal YisoBounty(YisoBountySO so) {
            Id = so.id;
            TargetIcon = so.targetImage;
            TargetId = so.target.id;
            targetName = so.targetName;
            description = so.description;
            UseTimeLimit = so.useTimeLimit;
            TimeLimit = so.timeLimit;
            RewardItemIds = new List<int>(so.rewards.Select(i => i.id));
            ItemUIs = new List<ItemUI>(so.rewards.Select(i => new ItemUI(i)));
            BountyReward = so.bountyReward;
            MapId = so.mapSO.id;
            EnemySpawnerPrefab = so.enemySpawnerPrefab;
        }

        public string GetTargetName(YisoLocale.Locale locale) => targetName[locale];
        public string GetDescription(YisoLocale.Locale locale) => description[locale];

        public bool GiveReward(YisoPlayer player, Func<int, YisoItem> itemFunc, out YisoBountyGiveReason reason) {
            reason = YisoBountyGiveReason.NONE;
            player.InventoryModule.Money += BountyReward;

            return true;
            foreach (var itemId in RewardItemIds) {
                var item = itemFunc(itemId);
                if (!player.InventoryModule.CanAdd(item)) {
                    reason = YisoBountyGiveReason.INVENTORY_FULL;
                    return false;
                }
                
                player.InventoryModule.AddItem(item);
            }

            return true;
        }

        public class ItemUI {
            public Sprite Icon { get; }

            private readonly YisoLocale itemName;

            public ItemUI(YisoItemSO so) {
                Icon = so.icon;
                var item = so.CreateItem();
                itemName = new YisoLocale {
                    kr = item.GetName(),
                    en = item.GetName(YisoLocale.Locale.EN)
                };
            }

            public string GetName(YisoLocale.Locale locale) => itemName[locale];
        }
    }
}