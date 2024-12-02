using System;
using Core.Domain.Locale;

namespace Core.Domain.Types {
    public enum YisoMenuTypes {
        INVENTORY,
        SKILL,
        QUEST,
        MAP,
        SETTINGS

    }

    public static class YisoMenuTypesUtils {
        public static string ToString(this YisoMenuTypes type, YisoLocale.Locale locale) => type switch {
            YisoMenuTypes.INVENTORY => locale == YisoLocale.Locale.KR ? "인벤토리" : "Inventory",
            YisoMenuTypes.SKILL => locale == YisoLocale.Locale.KR ? "스킬" : "Skill",
            YisoMenuTypes.QUEST => locale == YisoLocale.Locale.KR ? "퀘스트" : "Quest",
            YisoMenuTypes.MAP => locale == YisoLocale.Locale.KR ? "맵" : "Map",
            YisoMenuTypes.SETTINGS => locale == YisoLocale.Locale.KR ? "설정" : "Setting",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public enum YisoInteractTypes {
        STORAGE,
        STORE,
        QUEST,
        BLACKSMITH,
        NOTICE,
        WORLD_MAP,
        BOUNTY,
    }

    public static class YisoInteractTypesUtils {
        public static string ToString(this YisoInteractTypes type, YisoLocale.Locale locale) => type switch {
            YisoInteractTypes.STORAGE => locale == YisoLocale.Locale.KR ? "창고" : "Storage",
            YisoInteractTypes.STORE => locale == YisoLocale.Locale.KR ? "상점" : "Store",
            YisoInteractTypes.BOUNTY => locale == YisoLocale.Locale.KR ? "현상금 퀘스트" : "Wasted Quest",
            YisoInteractTypes.BLACKSMITH => locale == YisoLocale.Locale.KR ? "대장간" : "Blacksmith",
            YisoInteractTypes.NOTICE => locale == YisoLocale.Locale.KR ? "공지사항" : "Notice",
            YisoInteractTypes.WORLD_MAP => locale == YisoLocale.Locale.KR ? "지도" : "World Map",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    }

    public enum YisoGameUITypes {
        STORY_COMMENT,
        STORY_CLEAR,
        FLOATING_TEXT,
    }

    public static class YisoGameUITypeUtils {
        public static bool HasTitle(this YisoGameUITypes type) => type switch {
            YisoGameUITypes.STORY_COMMENT => false,
            YisoGameUITypes.STORY_CLEAR => false,
            YisoGameUITypes.FLOATING_TEXT => false,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public static bool HasCloseButton(this YisoGameUITypes type) => type switch {
            _ => false
        };

        public static string ToString(this YisoGameUITypes type, YisoLocale.Locale locale) => type switch {
            _ => ""
        };

        public static bool HasBlur(this YisoGameUITypes type) => type switch {
            YisoGameUITypes.STORY_CLEAR => true,
            _ => false
        };
    }

    public enum YisoPopupTypes {
        ALERT,
        ALERT_S,
        INPUT_NUMBER,
        DROP_ITEM_COUNT,
        INVENTORY_INPUT,
        QUEST,
        CABINET,
        NPC_DIALOGUE,
        GAME_DIRECTION,
        WELCOME,
        BOUNTY,
        BOUNTY_CLEAR,
        SELECT_STAGE,
    }

    public static class YisoPopupTypeUtils {
        public static bool ShowBackground(this YisoPopupTypes type) => type switch {
            YisoPopupTypes.BOUNTY_CLEAR => false,
            _ => true
        };
    }

    public enum YisoSelectionInputStates {
        DOWN, UP, HOLD
    }

    public enum YisoPopup2Types {
        NUMBER_INPUT
    }
}