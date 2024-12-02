using Core.Domain.Item;
using Core.Domain.Item.Equip;
using Utils.Extensions;

namespace Core.Domain.Actor.Player.Modules.Inventory.Reinforce {
    public abstract class YisoPlayerInventoryReinforceResult {
        private readonly float probability;
        
        public int Price { get; }
        
        public string GetPercentage() => (probability * 100f).ToPercentage();

        protected YisoPlayerInventoryReinforceResult(
            float probability, int price) {
            this.probability = probability;
            Price = price;
        }

    }
    
    public class YisoPlayerInventoryNormalReinforceResult : YisoPlayerInventoryReinforceResult {
        public int AttackInc { get; }
        public int DefenceInc { get; }
        
        public bool Success { get; }
        
        public YisoPlayerInventoryNormalReinforceResult(float probability, bool success, int price,
            int attackInc, int defenceInc) : base(probability, price) {
            Success = success;
            AttackInc = attackInc;
            DefenceInc = defenceInc;
        }
    }

    public class YisoPlayerInventoryPotentialReinforceResult : YisoPlayerInventoryReinforceResult {
        public bool UpgradeRank { get; }

        public bool ExistPotential => Potential1 != null;
        
        public YisoEquipPotential Potential1 { get; } = null;
        public YisoEquipPotential Potential2 { get; } = null;
        public YisoEquipPotential Potential3 { get; } = null;

        public YisoPlayerInventoryPotentialReinforceResult(float probability, int price, bool upgradeRank) : base(probability, price) {
            UpgradeRank = upgradeRank;
        }
        
        public YisoPlayerInventoryPotentialReinforceResult(float probability, int price,
            bool upgradeRank, YisoEquipPotential potential1, YisoEquipPotential potential2, YisoEquipPotential potential3) : this(probability, price, upgradeRank) {
            Potential1 = potential1;
            Potential2 = potential2;
            Potential3 = potential3;
        }
    }
}