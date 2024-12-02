using Core.Domain.Actor.Player;
using Core.Service;
using Core.Service.Character;
using Core.Service.Data;

namespace UI.Base {
    public abstract class YisoPlayerUIController : YisoUIController {
        protected YisoPlayer player;

        protected override void Awake() {
            base.Awake();
            player = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer();
        }
    }
}