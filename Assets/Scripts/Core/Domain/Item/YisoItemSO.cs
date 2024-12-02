using System;
using Core.Domain.Data;
using Core.Domain.Locale;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using Utils.ObjectId;

namespace Core.Domain.Item {
    /// <summary>
    /// Yiso Item
    /// Id Code -> 1
    /// 1010xxxx -> Sword
    ///     10101xxx -> Sword
    ///     10102xxx -> Spear
    ///     10103xxx -> Bow
    ///     10104xxx -> Wand
    /// 1011xxxx -> Shoes
    /// 1012xxxx -> Hat
    /// 1013xxxx -> Glove
    /// 1014xxxx -> Clothes
    ///     10141xxx -> Suit
    ///     10142xxx -> Top
    ///     10143xxx -> Bottom
    /// 1015xxxx -> Item
    ///     10151xxx -> Use
    ///     10152xxx -> Etc
    /// 1016xxxx -> Set Item  
    /// </summary>
    public abstract class YisoItemSO : ScriptableObject {
        [Title("Basic"), PreviewField, Required] public Sprite icon;
        public int id;
        public new YisoLocale name;
        public YisoLocale description;
        public YisoItem.InventoryType inventoryType;
        public int sellPrice;

        public abstract YisoItem CreateItem();
    }

    [Serializable]
    public class YisoItem {
        public int Id { get; private set; }
        public InventoryType InvType { get; private set; }
        
        public Sprite Icon { get; private set; }
        
        public double SellPrice { get; set; }
        
        public string ObjectId { get; internal set;  }

        public int Quantity { get; set; } = 0;
        public int Position { get; set; } = -1;
        
        
        private YisoLocale name;
        private YisoLocale description;

        protected YisoItem() { }

        protected YisoItem(YisoItemSO so) {
            Id = so.id;
            name = so.name;
            description = so.description;
            InvType = so.inventoryType;
            SellPrice = so.sellPrice;
            Icon = so.icon;
        }

        public YisoItem(YisoItem item) {
            Id = item.Id;
            InvType = item.InvType;
            Icon = item.Icon;
            SellPrice = item.SellPrice;
            Quantity = item.Quantity;
            Position = item.Position;
            name = item.name;
            description = item.description;
            ObjectId = YisoObjectID.GenerateString();
        }

        public virtual string GetName(YisoLocale.Locale locale = YisoLocale.Locale.KR) => name[locale];

        public string GetDescription(YisoLocale.Locale locale = YisoLocale.Locale.KR) => description[locale];

        public virtual void Load(YisoPlayerItemData data, YisoItemSO so) {
            Position = data.position;
            Quantity = data.quantity;
            ObjectId = data.objectId;
        }

        public virtual YisoPlayerItemData Save() {
            var data = new YisoPlayerItemData {
                quantity = Quantity,
                position = Position,
                itemId = Id,
                objectId = ObjectId
            };
            return data;
        }
        
        public enum InventoryType {
            EQUIP, USE, ETC
        }
    }
}