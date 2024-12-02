using System;

namespace Core.Domain.Item.Utils {
    public static class YisoItemUtils {
        public static YisoItem DeepCopy(this YisoItem item) => item.InvType switch {
            YisoItem.InventoryType.EQUIP => new YisoEquipItem((YisoEquipItem) item),
            YisoItem.InventoryType.USE => new YisoUseItem((YisoUseItem) item),
            YisoItem.InventoryType.ETC => new YisoEtcItem((YisoEtcItem) item),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}