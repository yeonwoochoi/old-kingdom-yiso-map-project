using System.Collections.Generic;
using System.Linq;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = System.Random;

namespace Test {
    public class TestReinforce : MonoBehaviour {
        [Button]
        public void PrintReinforceStats() {
            YisoEquipStatUtils.GetRandomStat();
        }

        [Button]
        public void Pick(int count = 10) {
            var weights1 = new Dictionary<YisoEquipStat, double> {
                { YisoEquipStat.DEFENCE, 4.08163 },
                { YisoEquipStat.DEFENCE_INC, 2.04082 },
                { YisoEquipStat.DEFENCE_DMG_DEC, 2.04082 },
                { YisoEquipStat.ATTACK, 4.08163 },
                { YisoEquipStat.ATTACK_INC, 2.04082 },
                { YisoEquipStat.ATTACK_DMG_INC, 2.04082 },
                { YisoEquipStat.MOVE_SPEED, 4.5455 },
                { YisoEquipStat.ATTACK_SPEED, 4.5455 },
                { YisoEquipStat.BOSS_DMG_INC, 4.8780 },
                { YisoEquipStat.CRI_DMG_INC, 10.8108 },
                { YisoEquipStat.CRI_PERCENT, 4.8780 },
                { YisoEquipStat.FINAL_DMG_INC, 2.04082 },
                { YisoEquipStat.MHP, 6.8182 },
                { YisoEquipStat.MHP_INC, 4.5455 },
                { YisoEquipStat.TENACITY, 4.8780 },
                { YisoEquipStat.IGNORE_TARGET_DEF, 4.5455 },
            };

            var weights2 = new Dictionary<YisoEquipStat, double> {
                { YisoEquipStat.DEFENCE, 0.4082 },
                { YisoEquipStat.DEFENCE_INC, 0.2041 },
                { YisoEquipStat.DEFENCE_DMG_DEC, 0.2041 },
                { YisoEquipStat.ATTACK, 0.4082 },
                { YisoEquipStat.ATTACK_INC, 0.2041 },
                { YisoEquipStat.ATTACK_DMG_INC, 0.2041 },
                { YisoEquipStat.MOVE_SPEED, 0.4545 },
                { YisoEquipStat.ATTACK_SPEED, 0.4545 },
                { YisoEquipStat.BOSS_DMG_INC, 0.4878 },
                { YisoEquipStat.CRI_DMG_INC, 1.08108 },
                { YisoEquipStat.CRI_PERCENT, 8.3721 },
                { YisoEquipStat.FINAL_DMG_INC, 0.2041 },
                { YisoEquipStat.MHP, 10.8000 },
                { YisoEquipStat.MHP_INC, 0.4545 },
                { YisoEquipStat.TENACITY, 0.4878 },
                { YisoEquipStat.IGNORE_TARGET_DEF, 0.4545 },
            };

            var weights3 = new Dictionary<YisoEquipStat, double> {
                { YisoEquipStat.DEFENCE, 0.04082 },
                { YisoEquipStat.DEFENCE_INC, 0.02041 },
                { YisoEquipStat.DEFENCE_DMG_DEC, 0.02041 },
                { YisoEquipStat.ATTACK, 0.04082 },
                { YisoEquipStat.ATTACK_INC, 0.02041 },
                { YisoEquipStat.ATTACK_DMG_INC, 0.02041 },
                { YisoEquipStat.MOVE_SPEED, 0.04545 },
                { YisoEquipStat.ATTACK_SPEED, 0.04545 },
                { YisoEquipStat.BOSS_DMG_INC, 0.04878 },
                { YisoEquipStat.CRI_DMG_INC, 0.1081 },
                { YisoEquipStat.CRI_PERCENT, 9.2093 },
                { YisoEquipStat.FINAL_DMG_INC, 0.02041 },
                { YisoEquipStat.MHP, 11.8800 },
                { YisoEquipStat.MHP_INC, 0.04545 },
                { YisoEquipStat.TENACITY, 0.04878 },
                { YisoEquipStat.IGNORE_TARGET_DEF, 0.04545 },
            };

            var selector1 = new WeightedRandomSelector<YisoEquipStat>(weights1);
            var selector2 = new WeightedRandomSelector<YisoEquipStat>(weights2);
            var selector3 = new WeightedRandomSelector<YisoEquipStat>(weights3);

            for (var i = 0; i < count; i++) {
                var item1 = selector1.GetRandomItem();
                var item2 = selector2.GetRandomItem();
                var item3 = selector3.GetRandomItem();
                Debug.Log($"{item1.ToUITitle()}\t {item2.ToUITitle()}\t {item3.ToUITitle()}");
            }
        }

        private class WeightedRandomSelector<T> {
            private readonly List<(T item, double comulativeWeight)> weightedItems;
            private readonly Random random;

            public WeightedRandomSelector(Dictionary<T, double> weights) {
                random = new Random();
                weightedItems = new List<(T item, double comulativeWeight)>();

                var cumulativeWeight = 0d;

                foreach (var kvp in weights) {
                    cumulativeWeight += kvp.Value;
                    weightedItems.Add((kvp.Key, cumulativeWeight));
                }
            }

            public T GetRandomItem() {
                var randomValue = random.NextDouble() * weightedItems.Last().comulativeWeight;
                return weightedItems.First(item => item.comulativeWeight >= randomValue).item;
            }
        }
    }
}