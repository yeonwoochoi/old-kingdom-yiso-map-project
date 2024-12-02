using System;
using Core.Domain.Locale;

namespace Core.Domain.Types {
    public enum YisoMapTypes {
        WORLD_MAP,
        BIRYUNA,
        MAHAN,
        MICHUHOL_EAST,
        MICHUHOL_SOUTH,
        MICHUHOL,
        SULCHUN,
        WIRYE,
        EMPTY
    }

    public static class YisoMapTypeUtils {
        public static string ToString(this YisoMapTypes type, YisoLocale.Locale locale) => type switch {
            YisoMapTypes.WORLD_MAP => locale == YisoLocale.Locale.KR ? "세계 지도" : "World Map",
            YisoMapTypes.BIRYUNA => locale == YisoLocale.Locale.KR ? "비류나" : "Biryuna",
            YisoMapTypes.MAHAN => locale == YisoLocale.Locale.KR ? "마한" : "Mahan",
            YisoMapTypes.MICHUHOL_EAST => locale == YisoLocale.Locale.KR ? "마한 동부" : "Mahan East",
            YisoMapTypes.MICHUHOL_SOUTH => locale == YisoLocale.Locale.KR ? "마한 남부" : "Mahan South",
            YisoMapTypes.MICHUHOL => locale == YisoLocale.Locale.KR ? "마한" : "Mahan",
            YisoMapTypes.SULCHUN => locale == YisoLocale.Locale.KR ? "술천성" : "Soulchun",
            YisoMapTypes.WIRYE => locale == YisoLocale.Locale.KR ? "위례성" : "Wirye",
            YisoMapTypes.EMPTY => "",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}