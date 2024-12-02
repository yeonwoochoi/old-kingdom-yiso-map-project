using Core.Domain.Item;
using Core.Domain.Types;

namespace UI.Menu.Inventory.Event {
    public abstract class InventoryUIEventArgs { }

    public class InventoryUIItemSelectedEventArgs : InventoryUIEventArgs {
        public YisoItem Item { get; }

        public InventoryUIItemSelectedEventArgs(YisoItem item) {
            Item = item;
        }
    }
    
    public class InventoryUIItemUnSelectedEventArgs : InventoryUIEventArgs { }

    public class InventoryUIEquippedItemSelectEventArgs : InventoryUIEventArgs {
        public YisoEquipItem Item { get; }

        public InventoryUIEquippedItemSelectEventArgs(YisoEquipItem item) {
            Item = item;
        }
    }

    public class InventoryUIEquippedItemUnSelectEventArgs : InventoryUIEventArgs { }

    public class InventoryUISwitchWeaponEventArgs : InventoryUIEventArgs {
        public bool IsWeaponSelected { get; }

        public InventoryUISwitchWeaponEventArgs(bool isWeaponSelected) {
            IsWeaponSelected = isWeaponSelected;
        }
    }
}