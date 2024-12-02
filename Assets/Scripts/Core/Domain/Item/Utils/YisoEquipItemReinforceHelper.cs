using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Domain.Types;
using Core.Logger;
using JetBrains.Annotations;
using UnityEngine;
using Utils;
using Utils.Extensions;

namespace Core.Domain.Item.Utils {
    public static class YisoEquipItemReinforceHelper {
        public static readonly Dictionary<YisoBuffEffectTypes, double> BUFF_EFFECT_WEIGHTS_1 = new() {
            { YisoBuffEffectTypes.DMG_INC, 2.04082 },
            { YisoBuffEffectTypes.CRI_PERCENT_INC, 2.04082 },
            { YisoBuffEffectTypes.CRI_DMG_INC, 1.9302 },
            { YisoBuffEffectTypes.MOVE_SPEED_INC, 4.08163 },
            { YisoBuffEffectTypes.ATTACK_SPEED_INC, 4.08163 },
            { YisoBuffEffectTypes.SKILL_COOLDOWN_TIME_DEC, 1.9302 },
            { YisoBuffEffectTypes.BOSS_DMG_INC, 1.9302 },
            { YisoBuffEffectTypes.MONEY_DROP_INC, 2.04082 },
            { YisoBuffEffectTypes.ITEM_DROP_INC, 2.04082 },
        };
        
        public static readonly Dictionary<YisoBuffEffectTypes, double> BUFF_EFFECT_WEIGHTS_2 = new() {
            { YisoBuffEffectTypes.DMG_INC, 1.04082 },
            { YisoBuffEffectTypes.CRI_PERCENT_INC, 1.04082 },
            { YisoBuffEffectTypes.CRI_DMG_INC, 0.9302 },
            { YisoBuffEffectTypes.MOVE_SPEED_INC, 3.08163 },
            { YisoBuffEffectTypes.ATTACK_SPEED_INC, 3.08163 },
            { YisoBuffEffectTypes.SKILL_COOLDOWN_TIME_DEC, 0.9302 },
            { YisoBuffEffectTypes.BOSS_DMG_INC, 0.9302 },
            { YisoBuffEffectTypes.MONEY_DROP_INC, 1.04082 },
            { YisoBuffEffectTypes.ITEM_DROP_INC, 1.04082 },
        };
        
        public static readonly Dictionary<YisoBuffEffectTypes, double> BUFF_EFFECT_WEIGHTS_3 = new() {
            { YisoBuffEffectTypes.DMG_INC, 0.04082 },
            { YisoBuffEffectTypes.CRI_PERCENT_INC, 0.04082 },
            { YisoBuffEffectTypes.CRI_DMG_INC, 0.09302 },
            { YisoBuffEffectTypes.MOVE_SPEED_INC, 2.08163 },
            { YisoBuffEffectTypes.ATTACK_SPEED_INC, 2.08163 },
            { YisoBuffEffectTypes.SKILL_COOLDOWN_TIME_DEC, 0.09302 },
            { YisoBuffEffectTypes.BOSS_DMG_INC, 0.09302 },
            { YisoBuffEffectTypes.MONEY_DROP_INC, 0.4082 },
            { YisoBuffEffectTypes.ITEM_DROP_INC, 0.4082 },
        };

        public static readonly Dictionary<YisoEquipRanks, double> UPGRADE_RANK_PROBS = new() {
            { YisoEquipRanks.M, 15.5 },
            { YisoEquipRanks.C, 7.2 },
            { YisoEquipRanks.B, 3.5 },
            { YisoEquipRanks.A, 2.3 },
            { YisoEquipRanks.S, 1.4 },
        };

        public static double GetUpgradeRankCost(int level, YisoEquipRanks rank) {
            var levelMultiplier = 0;
            switch (level) {
                case > 0 and <= 25:
                    levelMultiplier = 2;
                    break;
                case > 25 and <= 50:
                    levelMultiplier = 3;
                    break;
                case > 50 and <= 75:
                    levelMultiplier = 5;
                    break;
                default:
                    levelMultiplier = 10;
                    break;
            }

            var rankDefaultValue = 0;

            switch (rank) {
                case YisoEquipRanks.S:
                    rankDefaultValue = 160000;
                    break;
                case YisoEquipRanks.A:
                    rankDefaultValue = 110000;
                    break;
                case YisoEquipRanks.B:
                    rankDefaultValue = 70000;
                    break;
                case YisoEquipRanks.C:
                    rankDefaultValue = 50000;
                    break;
                case YisoEquipRanks.M:
                    rankDefaultValue = 25000;
                    break;
                case YisoEquipRanks.N:
                    rankDefaultValue = 10000;
                    break;
            }

            return levelMultiplier * rankDefaultValue * 0.25;
        } 
        
        public static Dictionary<YisoBuffEffectTypes, int> GetPotentialStatsByStageLevel(int stageId, YisoEquipRanks rank) {
            var inc = 0;
            var speed = 0;
            switch (stageId) {
                case > 0 and <= 25:
                    inc = 6;
                    speed = 14;
                    break;
                case > 25 and <= 50:
                    inc = 8;
                    speed = 16;
                    break;
                case > 50 and <= 75:
                    inc = 10;
                    speed = 18;
                    break;
                default:
                    inc = 12;
                    speed = 20;
                    break;
            }

            var rankMultiplier = (int)rank / (float) YisoEquipRanks.S;
            inc = Mathf.RoundToInt(inc * rankMultiplier);
            speed = Mathf.RoundToInt(speed * rankMultiplier);

            return new Dictionary<YisoBuffEffectTypes, int> {
                { YisoBuffEffectTypes.DMG_INC, inc },
                { YisoBuffEffectTypes.CRI_PERCENT_INC, inc },
                { YisoBuffEffectTypes.CRI_DMG_INC, inc },
                { YisoBuffEffectTypes.MOVE_SPEED_INC, speed },
                { YisoBuffEffectTypes.ATTACK_SPEED_INC, speed },
                { YisoBuffEffectTypes.SKILL_COOLDOWN_TIME_DEC, inc },
                { YisoBuffEffectTypes.BOSS_DMG_INC, inc },
                { YisoBuffEffectTypes.MONEY_DROP_INC, inc },
                { YisoBuffEffectTypes.ITEM_DROP_INC, inc },
            };
        }
    }
}