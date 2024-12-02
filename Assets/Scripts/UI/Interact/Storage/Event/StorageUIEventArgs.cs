using System;
using Core.Domain.Item;

namespace UI.Interact.Storage.Event {
    public abstract class StorageUIEventArgs : EventArgs {
        public YisoInteractStorageItemHoldersUI.Types Type { get; }

        protected StorageUIEventArgs(YisoInteractStorageItemHoldersUI.Types type) {
            Type = type;
        }
    }

    public class StorageUIItemSelectedEventArgs : StorageUIEventArgs {
        public YisoItem Item { get; }

        public StorageUIItemSelectedEventArgs(YisoInteractStorageItemHoldersUI.Types type, YisoItem item) : base(type) {
            Item = item;
        }
    }
    
    public class StorageUIItemUnSelectedEventArgs : StorageUIEventArgs {
        public StorageUIItemUnSelectedEventArgs(YisoInteractStorageItemHoldersUI.Types type) : base(type) { }
    }
}