using System;
using Core.Domain.Locale;

namespace Core.Domain.Actor.Player.Modules.Inventory {
    public enum YisoPlayerInventoryReasons {
        NONE,
        MONEY_NOT_ENOUGH,
        INVENTORY_SLOT_FULL,
        ITEM_NOT_FOUND,
        ARROW_NOT_EXIST,
        ARROW_NOT_ENOUGH
    }

    public static class YisoPlayerInventoryReasonsUtils {
        public static string ToString(this YisoPlayerInventoryReasons reason, YisoLocale.Locale locale) =>
            reason switch {
                YisoPlayerInventoryReasons.NONE => "",
                YisoPlayerInventoryReasons.MONEY_NOT_ENOUGH => locale == YisoLocale.Locale.KR ? "소지 금액이 부족합니다" : "",
                YisoPlayerInventoryReasons.INVENTORY_SLOT_FULL => locale == YisoLocale.Locale.KR ? "인벤토리가 가득 찼습니다" : "",
                YisoPlayerInventoryReasons.ITEM_NOT_FOUND => locale == YisoLocale.Locale.KR ? "아이템을 찾지 못했습니다" : "",
                YisoPlayerInventoryReasons.ARROW_NOT_EXIST => locale == YisoLocale.Locale.KR ? "화살을 찾지 못했습니다" : "",
                YisoPlayerInventoryReasons.ARROW_NOT_ENOUGH => locale == YisoLocale.Locale.KR ? "화살이 부족합니다" : "",
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
            };
    }
}