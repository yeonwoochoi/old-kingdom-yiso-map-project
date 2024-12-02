using System;
using Core.Domain.Bounty;
using Core.Domain.Types;

namespace UI.Interact.Bounty.Events {
    public abstract class BountyUIEventArgs : EventArgs { }

    public class BountyUISelectedEventArgs : BountyUIEventArgs {
        public YisoBounty Bounty { get; }
        public YisoBountyStatus Status { get; }

        public BountyUISelectedEventArgs(YisoBounty bounty, YisoBountyStatus status) {
            Bounty = bounty;
            Status = status;
        }
    }
    
    public class BountyUIUnSelectedEventArgs : BountyUIEventArgs { }
}