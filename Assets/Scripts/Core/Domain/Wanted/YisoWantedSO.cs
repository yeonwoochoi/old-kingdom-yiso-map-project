using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Item;
using Core.Domain.Locale;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Wanted {
    [CreateAssetMenu(fileName = "Wanted", menuName = "Yiso/Wanted/Wanted")]
    public class YisoWantedSO : ScriptableObject {
        public int id;
        [PreviewField] public Sprite targetImage;
        public YisoLocale targetName;
        public YisoLocale targetDescription;
        public double bounty;
        [RequiredListLength(0, 5)]
        public List<Item> rewardItems;

        public YisoWanted CreateWanted() => new(this);

        [Serializable]
        public class Item {
            public YisoItemSO itemSO;
            [HideIf("@this.itemSO != null && this.itemSO.inventoryType == YisoItem.InventoryType.EQUIP")]
            [MinValue(1)]
            public int count = 1;
        }
    }

    public class YisoWanted {
        public int Id { get; }
        public Sprite TargetImage { get; }
        public double Bounty { get; }
        public List<Item> RewardItems { get; }

        private readonly YisoLocale targetName;
        private readonly YisoLocale targetDescription;

        public YisoWanted(YisoWantedSO so) {
            Id = so.id;
            TargetImage = so.targetImage;
            targetName = so.targetName;
            targetDescription = so.targetDescription;
            Bounty = so.bounty;
            var itemCount = so.rewardItems.Count;
            if (itemCount > 5) itemCount = 5;
            RewardItems = new List<Item>(so.rewardItems.Take(itemCount).Select(item => new Item(item)));
        }

        public string GetTargetName(YisoLocale.Locale locale) => targetName[locale];
        public string GetTargetDescription(YisoLocale.Locale locale) => targetDescription[locale];
        
        public class Item {
            public int Id { get; }
            public int Count { get; } = 1;
            public Sprite Icon { get; }
            
            private readonly YisoLocale itemName;

            public Item(YisoWantedSO.Item item) {
                Id = item.itemSO.id;
                Icon = item.itemSO.icon;
                itemName = item.itemSO.name;
                Count = item.count;
            }

            public string GetName(YisoLocale.Locale locale) => itemName[locale];
        }
    }
}