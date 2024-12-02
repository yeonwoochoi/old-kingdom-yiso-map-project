using System;
using Core.Domain.Locale;
using Utils.Extensions;

namespace Core.Domain.Types {
    public enum YisoBuffEffectTypes {
        CR_INC,
        DMG_INC,
        CRI_PERCENT_INC,
        CRI_DMG_INC,
        MOVE_SPEED_INC,
        ATTACK_SPEED_INC,
        SKILL_COOLDOWN_TIME_DEC,
        BOSS_DMG_INC,
        
        HP_REC_INC,

        MONEY_DROP_INC,
        ITEM_DROP_INC,
    }

    public static class YisoEffectTypesUtils {
        public static string ToString(this YisoBuffEffectTypes type, YisoLocale.Locale locale) => type 
            switch {
                YisoBuffEffectTypes.HP_REC_INC => locale == YisoLocale.Locale.KR ? "HP 회복(%)" : "HP Recovery(%)",
                YisoBuffEffectTypes.CR_INC => locale == YisoLocale.Locale.KR ? "명예점수 증가(%)" : "Honor Rating Inc(%)",
                YisoBuffEffectTypes.DMG_INC => locale == YisoLocale.Locale.KR ? "대미지 증가(%)" : "Damage Inc(%)",
                YisoBuffEffectTypes.CRI_PERCENT_INC => locale == YisoLocale.Locale.KR ? "치명타 확률 증가(%)" : "Critical Percent Inc(%)",
                YisoBuffEffectTypes.CRI_DMG_INC => locale == YisoLocale.Locale.KR ? "치명타 대미지 증가(%)" : "Critical Damage Inc(%)",
                YisoBuffEffectTypes.SKILL_COOLDOWN_TIME_DEC => locale == YisoLocale.Locale.KR ? "스킬 재사용 대기시간 감소(%)": "Skill CoolDown Time Dec(%)",
                YisoBuffEffectTypes.MOVE_SPEED_INC => locale == YisoLocale.Locale.KR ? "이동 속도 증가(%)" : "Move Speed Inc(%)",
                YisoBuffEffectTypes.ATTACK_SPEED_INC => locale == YisoLocale.Locale.KR ? "공격 속도 증가(%)" : "Attack Speed Inc(%)",
                YisoBuffEffectTypes.BOSS_DMG_INC => locale == YisoLocale.Locale.KR ? "보스 대미지 증가(%)" : "Boss Damage Inc(%)",
                YisoBuffEffectTypes.MONEY_DROP_INC => locale == YisoLocale.Locale.KR ? "량 획득량 증가(%)" : "Ryang Drop Inc(%)",
                YisoBuffEffectTypes.ITEM_DROP_INC => locale == YisoLocale.Locale.KR ? "장비 획득확률 증가(%)" : "Item Drop Inc(%)",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

        public static string ToDescription(this YisoBuffEffectTypes type, YisoLocale.Locale locale) => type switch {
            YisoBuffEffectTypes.HP_REC_INC => locale == YisoLocale.Locale.KR ? "HP 회복을" : "recovery hp",
            YisoBuffEffectTypes.CR_INC => locale == YisoLocale.Locale.KR ? "명예점수를 증가시키는" : "increase Honor Rating Inc(%)",
            YisoBuffEffectTypes.DMG_INC => locale == YisoLocale.Locale.KR ? "대미지를 증가시키는" : "increase damage",
            YisoBuffEffectTypes.CRI_PERCENT_INC => locale == YisoLocale.Locale.KR
                ? "치명타 확률을 증가시키는"
                : "increase critical percent",
            YisoBuffEffectTypes.CRI_DMG_INC => locale == YisoLocale.Locale.KR
                ? "치명타 대미지를 증가시키는"
                : "increase critical damage",
            YisoBuffEffectTypes.SKILL_COOLDOWN_TIME_DEC => locale == YisoLocale.Locale.KR
                ? "스킬 재사용 대기시간을 감소시키는"
                : "decrease skill coolDown",
            YisoBuffEffectTypes.MOVE_SPEED_INC => locale == YisoLocale.Locale.KR ? "이동 속도를 증가시키는" : "increase move speed",
            YisoBuffEffectTypes.ATTACK_SPEED_INC => locale == YisoLocale.Locale.KR ? "공격 속도를 증가시키는" : "increase attack speed",
            YisoBuffEffectTypes.BOSS_DMG_INC => locale == YisoLocale.Locale.KR ? "보스 대미지를 증가시키는" : "increase boss damage",
            YisoBuffEffectTypes.MONEY_DROP_INC => locale == YisoLocale.Locale.KR ? "량 획득량을 증가시키는" : "increase Ryang Drop",
            YisoBuffEffectTypes.ITEM_DROP_INC => locale == YisoLocale.Locale.KR ? "장비 획득확률을 증가시키는" : "increase item drop",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public static string ToString(this YisoBuffEffectTypes type, int value, YisoLocale.Locale locale) {
            var str = type.ToString(locale);
            var uiValue = type.ToUIValue(value);
            return $"{str}: +{uiValue}";
        }

        public static string ToUIValue(this YisoBuffEffectTypes type, int value) {
            return value.ToPercentage();
        }
    }
}