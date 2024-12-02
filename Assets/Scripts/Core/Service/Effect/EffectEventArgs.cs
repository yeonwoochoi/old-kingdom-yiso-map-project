using Core.Domain.Effect;

namespace Core.Service.Effect {
    public abstract class EffectEventArgs {
        public YisoEffect Effect { get; }

        protected EffectEventArgs(YisoEffect effect) {
            Effect = effect;
        }
    }
    
    public class EffectStartEventArgs : EffectEventArgs {
        public EffectStartEventArgs(YisoEffect effect) : base(effect) { }
    }

    public class EffectCompleteEventArgs : EffectEventArgs {
        public EffectCompleteEventArgs(YisoEffect effect) : base(effect) { }
    }

    public class EffectRestartEventArgs : EffectEventArgs {
        public EffectRestartEventArgs(YisoEffect effect) : base(effect) { }
    }
}