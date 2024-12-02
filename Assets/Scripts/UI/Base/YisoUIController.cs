using Core.Behaviour;
using Core.Domain.Locale;
using Core.Service;
using Core.Service.Game;

namespace UI.Base {
    public abstract class YisoUIController : RunIBehaviour {
        protected IYisoGameService gameService;

        protected override void Awake() {
            base.Awake();
            gameService = YisoServiceProvider.Instance.Get<IYisoGameService>();
        }

        public YisoLocale.Locale CurrentLocale {
            get {
                gameService ??= YisoServiceProvider.Instance.Get<IYisoGameService>();
                return gameService.GetCurrentLocale();
            }
        }
    }
}