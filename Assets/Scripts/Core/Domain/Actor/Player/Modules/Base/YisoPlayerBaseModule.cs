using Core.Service;

namespace Core.Domain.Actor.Player.Modules.Base {
    public abstract class YisoPlayerBaseModule {
        protected readonly YisoPlayer player;

        protected YisoPlayerBaseModule(YisoPlayer player) {
            this.player = player;
        }

        protected T GetService<T>() where T : IYisoService => YisoServiceProvider.Instance.Get<T>();
    }
}