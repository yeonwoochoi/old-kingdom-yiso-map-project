using System;
using Core.Domain.Item;

namespace UI.Interact.Blacksmith.Event {
    public abstract class BlacksmithUIEventArgs : EventArgs {
        
    }

    public class BlacksmithUIItemSelectedEventArgs : BlacksmithUIEventArgs {
        public YisoItem Item { get; }

        public BlacksmithUIItemSelectedEventArgs(YisoItem item) {
            Item = item;
        }
    }
    
    public class BlacksmithUIItemUnSelectedEventArgs : BlacksmithUIEventArgs { }
}