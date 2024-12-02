using Core.Domain.Locale;
using Utils.Extensions;

namespace Core.Domain.Types {
    public enum YisoBuffStat {
        // POTION
        HP_REC, CURE,
        
        // DOPING
        EXP, MONEY, ITEM
    }
    
    public static class YisoBuffStatUtils {
        public static string ToUITitle(this YisoBuffStat stat, YisoLocale.Locale locale = YisoLocale.Locale.KR) => stat switch {
            YisoBuffStat.HP_REC => locale == YisoLocale.Locale.KR ? "HP 회복" : "HP Recovery",
            YisoBuffStat.CURE => locale == YisoLocale.Locale.KR ? "상태이상 회복" : "Cure",
            YisoBuffStat.EXP => locale == YisoLocale.Locale.KR ? "경험치 증가율" : "Exp Inc",
            YisoBuffStat.MONEY => locale == YisoLocale.Locale.KR ? "량 획득률" : "NYANG Inc",
            YisoBuffStat.ITEM => locale == YisoLocale.Locale.KR ? "아이템 드롭률" : "Item Drop Inc",
        };

        public static string ToUIValue(this YisoBuffStat stat, int value) => stat switch {
            YisoBuffStat.HP_REC => value.ToCommaString(),
            YisoBuffStat.CURE => value.ToCommaString(),
            YisoBuffStat.EXP => value.ToPercentage(),
            YisoBuffStat.MONEY => value.ToPercentage(),
            YisoBuffStat.ITEM => value.ToPercentage(),
        };
    }
}