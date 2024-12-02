using System;
using Core.Domain.Bounty;
using Core.Domain.Types;

namespace Core.Service.Bounty {
    public abstract class YisoBountyEventArgs : EventArgs {
        public YisoBounty Bounty { get; }
        public int BountyId => Bounty?.Id ?? -1;

        protected YisoBountyEventArgs(YisoBounty bounty) {
            Bounty = bounty;
        }
    }

    public class YisoBountyStatusChangeEventArgs : YisoBountyEventArgs {
        public YisoBountyStatus To { get; }

        public bool IsProgress => To == YisoBountyStatus.PROGRESS;

        public YisoBountyStatusChangeEventArgs(YisoBounty bounty, YisoBountyStatus to) : base(bounty) {
            To = to;
        }
    }

    public class YisoBountyDrawEventArgs : YisoBountyEventArgs {
        public YisoBountyDrawEventArgs(YisoBounty bounty) : base(bounty) { }
    }
    
    
}