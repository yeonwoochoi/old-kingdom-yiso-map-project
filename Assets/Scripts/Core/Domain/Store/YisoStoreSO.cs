using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Item;
using UnityEngine;

namespace Core.Domain.Store {
    [CreateAssetMenu(fileName = "Store", menuName = "Yiso/Store/StoreSO")]
    public class YisoStoreSO : ScriptableObject {
        public int id;
        public List<Item> equipItems;
        public List<Item> useItems;

        public YisoStore CreateStore() => new(this);
        
        [Serializable]
        public class Item {
            public YisoItemSO itemSO;
            public double price;
        }
    }

    public class YisoStore {
        public int Id { get; }
        public List<YisoStoreItem> EquipItems { get; }
        public List<YisoStoreItem> UseItems { get; }

        public YisoStore(YisoStoreSO so) {
            Id = so.id;
            EquipItems = new List<YisoStoreItem>(so.equipItems.Select(item => new YisoStoreItem(item)));
            UseItems = new List<YisoStoreItem>(so.useItems.Select(item => new YisoStoreItem(item)));
        }

        public YisoStore(int id) {
            Id = id;
            EquipItems = new List<YisoStoreItem>();
            UseItems = new List<YisoStoreItem>();
        }

        public List<YisoStoreItem> this[YisoItem.InventoryType type] => type switch {
            YisoItem.InventoryType.EQUIP => EquipItems,
            YisoItem.InventoryType.USE => UseItems,
            _ => null
        };
    }

    public class YisoStoreItem {
        public YisoItem Item { get; }
        public double Price { get; set; }

        public YisoStoreItem(YisoStoreSO.Item item) {
            Item = item.itemSO.CreateItem();
            Price = item.price;
        }

        public YisoStoreItem(YisoItem item, double price) {
            Item = item;
            Price = price;
        }
    }
}