using Utils;
using Utils.Extensions;

namespace Core.Domain.Actor.Enemy.Drop {
    public abstract class YisoEnemyBaseDrop {
        public EnemyDropTypes Type { get; }

        protected YisoEnemyBaseDrop(EnemyDropTypes type) {
            Type = type;
        }
    }

    public class YisoEnemyMoneyDrop : YisoEnemyBaseDrop {
        public int Value { get; }

        public YisoEnemyMoneyDrop(int value) : base(EnemyDropTypes.MONEY) {
            Value = value;
        }
    }

    public class YisoEnemyItemDrop : YisoEnemyBaseDrop {
        public int ItemId { get; }

        private readonly int probability;


        public YisoEnemyItemDrop(int itemId, int probability) : base(EnemyDropTypes.ITEM) {
            ItemId = itemId;
            this.probability = probability;
        }

        public bool CanDrop() => Randomizer.Below(probability.ToNormalized());
    }

    public enum EnemyDropTypes {
        ITEM, MONEY
    }
}