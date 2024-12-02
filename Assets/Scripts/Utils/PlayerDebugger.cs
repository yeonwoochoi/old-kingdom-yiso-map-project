using Core.Behaviour;
using Core.Service;
using Core.Service.Character;
using Sirenix.OdinInspector;

namespace Utils {
    public class PlayerDebugger : RunIBehaviour {
        [Button]
        public void Debug() {
            var service = YisoServiceProvider.Instance.Get<IYisoCharacterService>();
            var player = service.GetPlayer();
            var questModule = player.QuestModule;
            UnityEngine.Debug.Log("Player");
        }
    }
}