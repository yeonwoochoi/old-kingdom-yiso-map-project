using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Types;
using UnityEngine.Events;
using Utils.Extensions;

namespace Core.Domain.Stats {
    public sealed class YisoStats {
        public event UnityAction<YisoEquipStat, int> OnStatChangedEvent;
        
        private readonly Dictionary<YisoEquipStat, YisoStat> stats = new();
        private static YisoEquipStat[] EXCLUDE_STATS = { YisoEquipStat.REQ_LV };
        public static List<YisoEquipStat> ACTIVE_STATS = null;

        public YisoStats(bool isBuff = false) {
            ACTIVE_STATS ??= EnumExtensions.Values<YisoEquipStat>()
                .Where(stat => !EXCLUDE_STATS.Contains(stat))
                .ToList();

            foreach (var stat in ACTIVE_STATS) {
                stats[stat] = CreateStat(stat, isBuff);
                stats[stat].OnValueChangedEvent += OnValueChanged;
            }
        }

        private void OnValueChanged(YisoEquipStat stat, int value) {
            OnStatChangedEvent?.Invoke(stat, value);
        }

        public double GetNormalizedValue(YisoEquipStat stat) => GetValue(stat) * 0.01d;

        public int GetValue(YisoEquipStat stat) => stats[stat].Value;

        public void SetValue(YisoEquipStat stat, int value, bool notify = true) =>
            stats[stat].ApplyValue(value, notify);

        public void RegisterChangedEvent(YisoEquipStat stat, UnityAction<YisoStat.ChangedEventArgs> handler) {
            stats[stat].ChangedEvent += handler;
        }

        public void UnregisterChangedEvent(YisoEquipStat stat, UnityAction<YisoStat.ChangedEventArgs> handler) {
            stats[stat].ChangedEvent -= handler;
        }

        public void Copy(YisoStats other, bool notify = false) {
            foreach (var (equipStat, stat) in stats) {
                var otherStat = other.GetValue(equipStat);
                stat.SetValue(otherStat, notify);
            }
        }

        ~YisoStats() {
            foreach (var stat in ACTIVE_STATS) {
                stats[stat].OnValueChangedEvent -= OnValueChanged;
            }
        }

        private YisoStat CreateStat(YisoEquipStat stat, bool isBuff = false) => stat switch {
            YisoEquipStat.DEFENCE => new YisoStat(stat),
            YisoEquipStat.DEFENCE_INC => new YisoStat(stat, isPercentage:true),
            YisoEquipStat.DEFENCE_DMG_DEC => new YisoStat(stat, isPercentage:true, expression:isBuff ? YisoStat.Expressions.SUM : YisoStat.Expressions.MULTIPLY),
            YisoEquipStat.ATTACK => new YisoStat(stat),
            YisoEquipStat.ATTACK_INC => new YisoStat(stat, isPercentage:true),
            YisoEquipStat.ATTACK_DMG_INC => new YisoStat(stat, isPercentage:true, expression:isBuff ? YisoStat.Expressions.SUM : YisoStat.Expressions.MULTIPLY),
            YisoEquipStat.MOVE_SPEED => new YisoStat(stat, isPercentage:true, expression:isBuff ? YisoStat.Expressions.SUM : YisoStat.Expressions.MULTIPLY),
            YisoEquipStat.ATTACK_SPEED => new YisoStat(stat, isPercentage:true, expression:isBuff ? YisoStat.Expressions.SUM : YisoStat.Expressions.MULTIPLY),
            YisoEquipStat.BOSS_DMG_INC => new YisoStat(stat, isPercentage:true),
            YisoEquipStat.CRI_DMG_INC => new YisoStat(stat, isPercentage:true),
            YisoEquipStat.CRI_PERCENT => new YisoStat(stat, isPercentage:true, expression:isBuff ? YisoStat.Expressions.SUM : YisoStat.Expressions.MULTIPLY),
            YisoEquipStat.FINAL_DMG_INC => new YisoStat(stat, isPercentage:true, expression:isBuff ? YisoStat.Expressions.SUM : YisoStat.Expressions.MULTIPLY),
            YisoEquipStat.MHP => new YisoStat(stat),
            YisoEquipStat.MHP_INC => new YisoStat(stat, isPercentage:true),
            YisoEquipStat.TENACITY => new YisoStat(stat, isPercentage:true, expression:isBuff ? YisoStat.Expressions.SUM : YisoStat.Expressions.MULTIPLY),
            YisoEquipStat.IGNORE_TARGET_DEF => new YisoStat(stat, isPercentage:true, expression:isBuff ? YisoStat.Expressions.SUM : YisoStat.Expressions.MULTIPLY),
            YisoEquipStat.DROP_RATE => new YisoStat(stat, isPercentage:true, expression:isBuff ? YisoStat.Expressions.SUM : YisoStat.Expressions.MULTIPLY)
        };
    }
}