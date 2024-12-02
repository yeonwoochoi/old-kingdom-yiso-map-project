using Core.Domain.CoolDown;

namespace Core.Service.CoolDown {
    public abstract class CoolDownEventArgs {
        public YisoPlayerCoolDownHolder Holder { get; }
        public int SourceId => Holder.SourceId;

        protected CoolDownEventArgs(YisoPlayerCoolDownHolder holder) {
            Holder = holder;
        }
    }

    public class CoolDownStartEventArgs : CoolDownEventArgs {
        public CoolDownStartEventArgs(YisoPlayerCoolDownHolder holder) : base(holder) { }
    }

    public class CoolDownCompleteEventArgs : CoolDownEventArgs {
        public CoolDownCompleteEventArgs(YisoPlayerCoolDownHolder holder) : base(holder) { }
    }

    public class CoolDownStopEventArgs : CoolDownEventArgs {
        public CoolDownStopEventArgs(YisoPlayerCoolDownHolder holder) : base(holder) { }
    }
}