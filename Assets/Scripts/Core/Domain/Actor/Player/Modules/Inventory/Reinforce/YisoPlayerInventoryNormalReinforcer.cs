using System.Linq;
using System.Text;
using Core.Constants;
using Core.Domain.Item;
using Core.Domain.Types;
using UnityEngine;
using Utils;
using Utils.Extensions;

namespace Core.Domain.Actor.Player.Modules.Inventory.Reinforce {
    public class YisoPlayerInventoryNormalReinforcer {
        public YisoPlayerInventoryNormalReinforceResult GetResult(YisoEquipItem item) {
            /*var requireLevel = item.TotalStats[YisoEquipStat.REQ_LV];
            var rank = item.Rank;
            var upgradedSlots = item.UpgradedSlots;

            var probability = CalculateSuccessRate(requireLevel, rank, upgradedSlots);
            var cost = CalculateCost(requireLevel, rank, upgradedSlots);
            var (attackInc, defenceInc) = GetUpgradeStats(item.Slot, upgradedSlots, requireLevel, rank);
            var success = Randomizer.Below(probability);

            return new YisoPlayerInventoryNormalReinforceResult(
                probability, success, cost,
                attackInc, defenceInc
            );*/
            return null;
        }

        public void Debug(YisoEquipItem item) {
            /*var requireLevel = item.TotalStats[YisoEquipStat.REQ_LV];
            var baseRate = 0f;
            foreach (var (key, value) in YisoGameConstants.REINFORCE_NORMAL_LEVEL_SUCCESS_RATE_MAP) {
                if (requireLevel > key) continue;
                baseRate = value;
                break;
            }

            if (baseRate == 0.0f)
                baseRate = YisoGameConstants.REINFORCE_NORMAL_LEVEL_SUCCESS_RATE_MAP.Last().Value;

            var builder = new StringBuilder("======= DEBUG =======\n");

            var attack = item.GetAttack();
            var defence = item.GetDefence();
            for (var i = 0; i < 7; i++) {
                var rate = Calculate(i);
                var cost = CalculateCost(requireLevel, item.Rank, i);
                var (attackInc, defenceInc) = GetUpgradeStats(item.Slot, i, requireLevel, item.Rank);

                builder.Append($"[강화 {i} => {i + 1}] 확률: {(rate * 100f):F2}% | ");
                builder.Append($"공격력 증가 {attack.ToCommaString()} => {(attack + attackInc).ToCommaString()}, ");
                builder.Append($"방어력 증가 {defence.ToCommaString()} => {(defence + defenceInc).ToCommaString()}, ");
                builder.Append($"가격: {cost.ToCommaString()}\n");
                
                attack += attackInc;
                defence += defenceInc;
            }
            
            UnityEngine.Debug.Log(builder.ToString());

            return;
            float Calculate(int upgradedSlots) {
                var rankAdjustment = 0.05f * (int)item.Rank;
                var upgradeAdjustment = Mathf.Exp(upgradedSlots * YisoGameConstants.REINFORCE_NORMAL_RATE_RATIO);
                var adjustedRate = (baseRate - (requireLevel * 0.05f) + rankAdjustment) / upgradeAdjustment;
                return Mathf.Clamp(adjustedRate, 0.1f, baseRate);
            }*/
        }

        private float CalculateSuccessRate(int requireLevel, YisoEquipRanks rank, int upgradedSlots) {
            var baseRate = 0f;
            foreach (var (key, value) in YisoGameConstants.REINFORCE_NORMAL_LEVEL_SUCCESS_RATE_MAP) {
                if (requireLevel > key) continue;
                baseRate = value;
                break;
            }

            if (baseRate == 0.0f)
                baseRate = YisoGameConstants.REINFORCE_NORMAL_LEVEL_SUCCESS_RATE_MAP.Last().Value;

            var rankAdjustment = 0.05f * (int)rank;
            var upgradeAdjustment = Mathf.Exp(upgradedSlots * YisoGameConstants.REINFORCE_NORMAL_RATE_RATIO);
            var adjustedRate = (baseRate - (requireLevel * 0.05f) + rankAdjustment) / upgradeAdjustment;
            return Mathf.Clamp(adjustedRate, 0.1f, baseRate);
        }

        private int CalculateCost(int requireLevel, YisoEquipRanks rank, int upgradedSlots) {
            var baseCost = 0;
            foreach (var (key, value) in YisoGameConstants.REINFORCE_NORMAL_LEVEL_COST_MAP) {
                if (requireLevel > key) continue;
                baseCost = value;
                break;
            }

            if (baseCost == 0)
                baseCost = YisoGameConstants.REINFORCE_NORMAL_LEVEL_COST_MAP.Last().Value;


            var rankMultiplier = (int)rank + 1;
            var upgradeCostMultiplier = Mathf.Exp(upgradedSlots * YisoGameConstants.REINFORCE_NORMAL_COST_RATIO);
            var cost = (int)(baseCost * Mathf.Pow(1.1f, requireLevel) * rankMultiplier * upgradeCostMultiplier);
            return cost;
        }

        private (int attack, int defence) GetUpgradeStats(YisoEquipSlots slot, int upgradedSlots, int reqLevel,
            YisoEquipRanks rank) {
            var attackInc = 0;
            var defenceInc = 0;

            var levelMultiplier = 1 + reqLevel * 0.1f; // 레벨에 따른 증가 비율
            var rankMultiplier = 1 + (int)rank * 0.2f; // 랭크에 따른 증가 비율

            switch (slot) {
                case YisoEquipSlots.WEAPON:
                    attackInc = (int)((10 + upgradedSlots * 5) * levelMultiplier * rankMultiplier);
                    defenceInc = (int)((2 + upgradedSlots * 1) * levelMultiplier * rankMultiplier);
                    break;
                case YisoEquipSlots.HAT:
                case YisoEquipSlots.TOP:
                case YisoEquipSlots.BOTTOM:
                case YisoEquipSlots.SHOES:
                case YisoEquipSlots.GLOVE:
                    attackInc = (int)((2 + upgradedSlots * 1) * levelMultiplier * rankMultiplier);
                    defenceInc = (int)((5 + upgradedSlots * 3) * levelMultiplier * rankMultiplier);
                    break;
            }

            return (attackInc, defenceInc);
        }
    }

    
}