using Core.Domain.Item;
using Core.Domain.Item.Equip;
using Core.Domain.Item.Utils;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Stage;
using UnityEngine;
using Utils;

namespace Core.Domain.Actor.Player.Modules.Inventory.Reinforce {
    public class YisoPlayerInventoryPotentialReinforcer {
        private readonly Randomizer.WeightedRandomSelector<YisoBuffEffectTypes> selector1 = new(YisoEquipItemReinforceHelper
            .BUFF_EFFECT_WEIGHTS_1);
        private readonly Randomizer.WeightedRandomSelector<YisoBuffEffectTypes> selector2 = new(YisoEquipItemReinforceHelper
            .BUFF_EFFECT_WEIGHTS_2);
        private readonly Randomizer.WeightedRandomSelector<YisoBuffEffectTypes> selector3 = new(YisoEquipItemReinforceHelper
            .BUFF_EFFECT_WEIGHTS_3);

        public YisoPlayerInventoryPotentialReinforceResult GetResult(YisoEquipItem item) {
            var stage = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();
            var upgradeRank = UpgradeRank(item, stage, out var cost, out double prob);
            var (potential1, potential2, potential3) = GetRandomPotentials(item, stage, upgradeRank);
            return new YisoPlayerInventoryPotentialReinforceResult((float)prob, (int)cost, upgradeRank, potential1, potential2, potential3);
        }

        private bool UpgradeRank(YisoEquipItem item, int stageId, out double cost, out double prob) {
            cost = -1;
            prob = -1;

            var rankProbs = YisoEquipItemReinforceHelper.UPGRADE_RANK_PROBS;

            if (!item.Rank.TryGetNextRank(out var nextRank)) {
                return false;
            }
            
            cost = YisoEquipItemReinforceHelper.GetUpgradeRankCost(stageId, nextRank);
            prob = rankProbs[nextRank];
            var success = Randomizer.Below(prob * 0.01f);
            
            return success;
        }

        private (YisoEquipPotential potential1, YisoEquipPotential potential2, YisoEquipPotential potential3) GetRandomPotentials(
            YisoEquipItem item, int stageId, bool rankUpgraded) {
            if (item.Rank == YisoEquipRanks.N && !rankUpgraded) 
                return (null, null, null);
            
            var stat1 = selector1.GetRandomItem();
            var stat2 = selector2.GetRandomItem();
            var stat3 = selector3.GetRandomItem();

            var rank = rankUpgraded ? item.Rank.NextRank() : item.Rank;
            var statValues =
                YisoEquipItemReinforceHelper.GetPotentialStatsByStageLevel(stageId, rank);

            var statValue1 = statValues[stat1];
            var statValue2 = statValues[stat2];
            var statValue3 = statValues[stat3];

            var potential1 = new YisoEquipPotential();
            var potential2 = new YisoEquipPotential();
            var potential3 = new YisoEquipPotential();
            
            potential1.SetValue(stat1, statValue1);
            potential2.SetValue(stat2, statValue2);
            potential3.SetValue(stat3, statValue3);
            
            return (potential1, potential2, potential3);
        }
    }
}