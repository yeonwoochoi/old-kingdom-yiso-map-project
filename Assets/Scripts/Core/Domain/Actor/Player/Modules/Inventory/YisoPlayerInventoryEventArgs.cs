using System;
using Character.Weapon;
using Core.Domain.Item;
using Core.Domain.Types;

namespace Core.Domain.Actor.Player.Modules.Inventory {
    public abstract class YisoPlayerInventoryEventArgs : EventArgs {
        public YisoItem.InventoryType Type { get; }

        protected YisoPlayerInventoryEventArgs(YisoItem.InventoryType type) {
            Type = type;
        }
    }

    public abstract class YisoPlayerInventoryBaseEventArgs : YisoPlayerInventoryEventArgs {
        public int Position { get; }

        protected YisoPlayerInventoryBaseEventArgs(YisoItem.InventoryType inventory, int position) :
            base(inventory) {
            Position = position;
        }
    }

    public class YisoPlayerInventoryFailureEventArgs : YisoPlayerInventoryEventArgs {
        public YisoPlayerInventoryReasons Reason { get; }

        public YisoPlayerInventoryFailureEventArgs(YisoPlayerInventoryReasons reason, YisoItem.InventoryType type) :
            base(type) {
            Reason = reason;
        }
    }

    public class YisoPlayerInventoryAddEventArgs : YisoPlayerInventoryBaseEventArgs {
        public YisoPlayerInventoryAddEventArgs(YisoItem.InventoryType inventory, int position) : base(inventory,
            position) { }
    }

    public class YisoPlayerInventoryRemoveEventArgs : YisoPlayerInventoryBaseEventArgs {
        public YisoPlayerInventoryRemoveEventArgs(YisoItem.InventoryType inventory, int position) : base(inventory,
            position) { }
    }

    public class YisoPlayerInventoryDropEventArgs : YisoPlayerInventoryBaseEventArgs {
        public YisoItem DroppedItem { get; }

        public YisoPlayerInventoryDropEventArgs(YisoItem.InventoryType inventory, YisoItem item, int position) : base(
            inventory, position) {
            DroppedItem = item;
        }
    }

    public class YisoPlayerInventoryCountEventArgs : YisoPlayerInventoryBaseEventArgs {
        public int AfterCount { get; }
        public int ItemId { get; }

        public YisoPlayerInventoryCountEventArgs(YisoItem.InventoryType inventory, int position, int itemId,
            int afterCount) : base(inventory, position) {
            AfterCount = afterCount;
            ItemId = itemId;
        }
    }

    public class YisoPlayerInventoryV2EquipEventArgs : YisoPlayerInventoryEventArgs {
        public YisoEquipSlots Slot { get; }

        public YisoPlayerInventoryV2EquipEventArgs(YisoEquipSlots slot) : base(YisoItem.InventoryType.EQUIP) {
            Slot = slot;
        }
    }

    public class YisoPlayerInventoryEquipEventArgs : YisoPlayerInventoryBaseEventArgs {
        public YisoEquipSlots Slot { get; }
        public YisoWeapon.AttackType AttackType { get; }

        public YisoPlayerInventoryEquipEventArgs(int position, YisoEquipSlots slot) :
            base(YisoItem.InventoryType.EQUIP, position) {
            Slot = slot;
        }

        public YisoPlayerInventoryEquipEventArgs(int position, YisoEquipSlots slot, YisoWeapon.AttackType attackType) :
            this(position, slot) {
            AttackType = attackType;
        }
    }

    public class YisoPlayerInventoryUnEquipEventArgs : YisoPlayerInventoryBaseEventArgs {
        public YisoEquipSlots Slot { get; }
        
        public YisoWeapon.AttackType AttackType { get; }

        public YisoPlayerInventoryUnEquipEventArgs(int position, YisoEquipSlots slot) : base(
            YisoItem.InventoryType.EQUIP, position) {
            Slot = slot;
        }

        public YisoPlayerInventoryUnEquipEventArgs(int position, YisoEquipSlots slot, YisoWeapon.AttackType attackType) :
            this(position, slot) {
            AttackType = attackType;
        }
    }

    public class YisoPlayerInventoryReOrderedEventArgs : YisoPlayerInventoryEventArgs {
        public YisoPlayerInventoryReOrderedEventArgs(YisoItem.InventoryType type) : base(type) { }
    }

    public class YisoPlayerInventorySwitchWeaponEventArgs : YisoPlayerInventoryEventArgs {
        public YisoWeapon.AttackType BeforeType { get; }
        public YisoWeapon.AttackType AfterType { get; }
        public YisoEquipItem SwitchedWeapon { get; }
        
        public double CombatRating { get; }

        public YisoPlayerInventorySwitchWeaponEventArgs(YisoEquipItem switchedWeapon, double combatRating, YisoWeapon.AttackType beforeType, YisoWeapon.AttackType afterType) : base(YisoItem.InventoryType.EQUIP) {
            SwitchedWeapon = switchedWeapon;
            BeforeType = beforeType;
            AfterType = afterType;
            CombatRating = combatRating;
        }
    }
}