using System.Linq;
using Core.Domain.Locale;
using Core.Domain.Stats;
using UnityEngine;
using Utils.Extensions;

namespace Core.Domain.Types {
    public enum YisoEquipStat {
        REQ_LV,
        DROP_RATE,
        DEFENCE,
        DEFENCE_INC,
        DEFENCE_DMG_DEC,

        ATTACK,
        ATTACK_INC,
        ATTACK_DMG_INC,
        
        MOVE_SPEED,
        ATTACK_SPEED,
        BOSS_DMG_INC,
        CRI_DMG_INC,
        CRI_PERCENT,
        FINAL_DMG_INC,
        MHP,
        MHP_INC,
        TENACITY,
        IGNORE_TARGET_DEF,
    }
    
    public static class YisoEquipStatUtils {
        public static string ToUITitle(this YisoEquipStat stat, YisoLocale.Locale locale = YisoLocale.Locale.KR) =>
            stat switch {
                YisoEquipStat.REQ_LV => locale == YisoLocale.Locale.KR ? "요구 레벨" : "Req Lev",
                YisoEquipStat.DROP_RATE => locale == YisoLocale.Locale.KR ? "아이템 드롭률" : "Item Drop Rate(%)",
                YisoEquipStat.DEFENCE => locale == YisoLocale.Locale.KR ? "물리 방어력" : "Weapon Defence",
                YisoEquipStat.DEFENCE_INC => locale == YisoLocale.Locale.KR ? $"물리 방어력(%)" : $"Weapon Defence(%)",
                YisoEquipStat.DEFENCE_DMG_DEC => locale == YisoLocale.Locale.KR ? "물리 피해 감소(%)" : "Weapon Damage Dec(%)",
                YisoEquipStat.ATTACK => locale == YisoLocale.Locale.KR ? "물리 공격력" : "Weapon Attack",
                YisoEquipStat.ATTACK_INC => locale == YisoLocale.Locale.KR ? "물리 공격력(%)" : "Weapon Attack(%)",
                YisoEquipStat.ATTACK_DMG_INC => locale == YisoLocale.Locale.KR ? "물리 대미지(%)" : "Weapon Damage(%)",
                 YisoEquipStat.MOVE_SPEED => locale == YisoLocale.Locale.KR ? "이동 속도(%)" : "Move Speed(%)",
                YisoEquipStat.ATTACK_SPEED => locale == YisoLocale.Locale.KR ? "공격 속도(%)" : "Attack Speed(%)",
                YisoEquipStat.BOSS_DMG_INC => locale == YisoLocale.Locale.KR ? "보스 대미지(%)" : "Boss Damage(%)",
                YisoEquipStat.CRI_DMG_INC => locale == YisoLocale.Locale.KR ? "치명타 피해(%)" : "Critical Damage(%)",
                YisoEquipStat.CRI_PERCENT => locale == YisoLocale.Locale.KR ? "치명타 확률(%)" : "Critical Percent(%)",
                YisoEquipStat.FINAL_DMG_INC => locale == YisoLocale.Locale.KR ? "최종 대미지(%)" : "Final Damage(%)",
                YisoEquipStat.MHP => locale == YisoLocale.Locale.KR ? "최대 HP" : "Max HP",
                YisoEquipStat.MHP_INC => locale == YisoLocale.Locale.KR ? "최대 HP(%)" : "Max HP(%)",
                YisoEquipStat.TENACITY => locale == YisoLocale.Locale.KR ? "상태 이상 내성(%)" : "Tenacity(%)",
                YisoEquipStat.IGNORE_TARGET_DEF => locale == YisoLocale.Locale.KR ? "방어력 무시(%)" : "Ignore Defence(%)",
            };

        public static string ToUIValue(this YisoEquipStat stat, int value) => stat switch {
            YisoEquipStat.REQ_LV => value.ToCommaString(),
            YisoEquipStat.DROP_RATE => value.ToPercentage(),
            YisoEquipStat.DEFENCE => value.ToCommaString(),
            YisoEquipStat.DEFENCE_INC => value.ToPercentage(),
            YisoEquipStat.DEFENCE_DMG_DEC => value.ToPercentage(),
            YisoEquipStat.ATTACK => value.ToCommaString(),
            YisoEquipStat.ATTACK_INC => value.ToPercentage(),
            YisoEquipStat.ATTACK_DMG_INC => value.ToPercentage(),
            YisoEquipStat.MOVE_SPEED => value.ToPercentage(),
            YisoEquipStat.ATTACK_SPEED => value.ToPercentage(),
            YisoEquipStat.BOSS_DMG_INC => value.ToPercentage(),
            YisoEquipStat.CRI_DMG_INC => value.ToPercentage(),
            YisoEquipStat.CRI_PERCENT => value.ToPercentage(),
            YisoEquipStat.FINAL_DMG_INC => value.ToPercentage(),
            YisoEquipStat.MHP => value.ToCommaString(),
            YisoEquipStat.MHP_INC => value.ToPercentage(),
            YisoEquipStat.TENACITY => value.ToPercentage(),
            YisoEquipStat.IGNORE_TARGET_DEF => value.ToPercentage()
        };

        public static string ToUIText(this YisoEquipStat stat, int value,
            YisoLocale.Locale locale = YisoLocale.Locale.KR) => stat switch {
            YisoEquipStat.REQ_LV => locale == YisoLocale.Locale.KR
                ? $"요구 레벨: {value.ToCommaString()}"
                : $"Req Lev: {value}",
            YisoEquipStat.DROP_RATE => locale == YisoLocale.Locale.KR
                ? $"아이템 드롭률: {value.ToPercentage()}"
                : $"Item Drop Rate: {value.ToPercentage()}",
            YisoEquipStat.DEFENCE => locale == YisoLocale.Locale.KR
                ? $"물리 방어력: +{value.ToCommaString()}"
                : $"Weapon Defence: {value.ToCommaString()}",
            YisoEquipStat.DEFENCE_INC => locale == YisoLocale.Locale.KR
                ? $"물리 방어력: +{value.ToPercentage()}"
                : $"Weapon Defence: +{value.ToPercentage()}",
            YisoEquipStat.DEFENCE_DMG_DEC => locale == YisoLocale.Locale.KR
                ? $"물리 피해 감소: -{value.ToPercentage()}"
                : $"Weapon Damage Dec: -{value.ToPercentage()}",
            YisoEquipStat.ATTACK => locale == YisoLocale.Locale.KR
                ? $"물리 공격력: +{value.ToCommaString()}"
                : $"Weapon Attack: +{value.ToCommaString()}",
            YisoEquipStat.ATTACK_INC => locale == YisoLocale.Locale.KR
                ? $"물리 공격력: +{value.ToPercentage()}"
                : $"Weapon Attack: +{value.ToPercentage()}",
            YisoEquipStat.ATTACK_DMG_INC => locale == YisoLocale.Locale.KR
                ? $"물리 대미지: +{value.ToPercentage()}"
                : $"Weapon Damage: +{value.ToPercentage()}",
            YisoEquipStat.MOVE_SPEED => locale == YisoLocale.Locale.KR
                ? $"이동 속도: +{value.ToPercentage()}"
                : $"Move Speed: +{value.ToPercentage()}",
            YisoEquipStat.ATTACK_SPEED => locale == YisoLocale.Locale.KR
                ? $"공격 속도: +{value.ToPercentage()}"
                : $"Attack Speed: +{value.ToPercentage()}",
            YisoEquipStat.BOSS_DMG_INC => locale == YisoLocale.Locale.KR
                ? $"보스 공격력: +{value.ToPercentage()}"
                : $"Boss Damage: +{value.ToPercentage()}",
            YisoEquipStat.CRI_DMG_INC => locale == YisoLocale.Locale.KR
                ? $"치명타 피해: +{value.ToPercentage()}"
                : $"Critical Damage: +{value.ToPercentage()}",
            YisoEquipStat.CRI_PERCENT => locale == YisoLocale.Locale.KR
                ? $"치명타 확률: +{value.ToPercentage()}"
                : $"Critical Percent: +{value.ToPercentage()}",
            YisoEquipStat.FINAL_DMG_INC => locale == YisoLocale.Locale.KR
                ? $"최종 대미지: +{value.ToPercentage()}"
                : $"Final Damage: +{value.ToPercentage()}",
            YisoEquipStat.MHP => locale == YisoLocale.Locale.KR
                ? $"최대 HP: +{value.ToCommaString()}"
                : $"Max HP: +{value.ToCommaString()}",
            YisoEquipStat.MHP_INC => locale == YisoLocale.Locale.KR
                ? $"최대 HP: +{value.ToPercentage()}"
                : $"Max HP: +{value.ToPercentage()}",
            YisoEquipStat.TENACITY => locale == YisoLocale.Locale.KR
                ? $"상태 이상 내성: +{value.ToPercentage()}"
                : $"Tenacity: +{value.ToPercentage()}",
            YisoEquipStat.IGNORE_TARGET_DEF => locale == YisoLocale.Locale.KR
                ? $"방어력 무시: +{value.ToPercentage()}"
                : $"Ignore Defence: +{value.ToPercentage()}"
        };

        public static void GetRandomStat() {
            var activeStats = YisoStats.ACTIVE_STATS;
            if (activeStats == null)
                new YisoStats();
            activeStats = YisoStats.ACTIVE_STATS;
            var stats = EnumExtensions.Values<YisoEquipStat>()
                .Where(stat => activeStats.Contains(stat))
                .ToList();
            var r = string.Join('\n', stats.Select(s => s.ToUITitle()));
            Debug.Log(r);
        }

        public static bool IsPercentage(this YisoEquipStat stat) {
            switch (stat) {
                case YisoEquipStat.DEFENCE_INC:
                case YisoEquipStat.DROP_RATE:
                case YisoEquipStat.DEFENCE_DMG_DEC:
                case YisoEquipStat.ATTACK_INC:
                case YisoEquipStat.ATTACK_DMG_INC:
                case YisoEquipStat.MOVE_SPEED:
                case YisoEquipStat.ATTACK_SPEED:
                case YisoEquipStat.BOSS_DMG_INC:
                case YisoEquipStat.CRI_DMG_INC:
                case YisoEquipStat.CRI_PERCENT:
                case YisoEquipStat.FINAL_DMG_INC:
                case YisoEquipStat.MHP_INC:
                case YisoEquipStat.TENACITY:
                case YisoEquipStat.IGNORE_TARGET_DEF:
                    return true;
            }

            return false;
        }
    }
}