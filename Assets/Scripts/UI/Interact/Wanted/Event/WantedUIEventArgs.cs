using System;
using Core.Domain.Wanted;

namespace UI.Interact.Wanted.Event {
    public abstract class WantedUIEventArgs : EventArgs { }

    public class WantedUISelectedEventArgs : WantedUIEventArgs {
        public YisoWanted Wanted { get; }

        public WantedUISelectedEventArgs(YisoWanted wanted) {
            Wanted = wanted;
        }
    }
    
    public class WantedUIUnSelectedEventArgs : WantedUIEventArgs { }
}