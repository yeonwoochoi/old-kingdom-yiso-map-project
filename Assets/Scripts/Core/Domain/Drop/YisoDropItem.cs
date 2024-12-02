using Core.Domain.Actor.Enemy.Drop;
using Core.Domain.Item;

namespace Core.Domain.Drop {
    public sealed class YisoDropItem {
        public double MoneyValue { get; }
        public YisoItem Item { get; }
        public int ItemCount { get; }
        
        public Types Type { get; }

        public YisoDropItem(double moneyValue) {
            MoneyValue = moneyValue;
            ItemCount = 0;
            Item = null;
            Type = Types.MONEY;
        }

        public YisoDropItem(YisoItem item, int itemCount) {
            Item = item;
            ItemCount = itemCount;
            MoneyValue = 0;
            Type = Types.ITEM;
        }
        
        public enum Types {
            MONEY, ITEM
        }

        public static YisoDropItem CreateMoneyDrop(double money) => new(money);
        public static YisoDropItem CreateItemDrop(YisoItem item) => new(item, 1);

    }
}