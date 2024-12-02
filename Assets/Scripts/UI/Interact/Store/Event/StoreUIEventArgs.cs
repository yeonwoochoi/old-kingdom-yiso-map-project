using System;
using Core.Domain.Item;
using UI.Interact.Store.Holder;

namespace UI.Interact.Store.Event {
    public abstract class StoreUIEventArgs : EventArgs {
        public YisoInteractStoreContentUI.Types Type { get; }

        protected StoreUIEventArgs(YisoInteractStoreContentUI.Types type) {
            Type = type;
        }
    }
    
    public class StoreUIItemSelectedEventArgs : StoreUIEventArgs {
        public YisoItem Item { get; }

        public double Price { get; }
        
        public StoreUIItemSelectedEventArgs(YisoInteractStoreContentUI.Types type, YisoItem item, double price) : base(type) {
            Item = item;
            Price = price;
        }
    }
    
    public class StoreUIItemUnSelectedEventArgs : StoreUIEventArgs {
        public StoreUIItemUnSelectedEventArgs(YisoInteractStoreContentUI.Types type) : base(type) { }
    }

    public class StoreUIInventoryItemSelectedEventArgs : StoreUIEventArgs {
        public YisoItem Item { get; }
        public bool Selected { get; }
        public StoreUIInventoryItemSelectedEventArgs(bool selected, YisoItem item) : base(YisoInteractStoreContentUI.Types.INVENTORY) {
            Selected = selected;
            Item = item;
        }
    }
}