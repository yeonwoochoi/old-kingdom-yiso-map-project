using Core.Behaviour;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Character;
using Core.Service.Data;
using Core.Service.Scene;
using Core.Service.UI;
using Core.Service.UI.Popup;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Init.Setting {
    public class YisoInitDataSettingsPanelUI : RunIBehaviour {
        [SerializeField] private UnityEvent onCloseEvent;
        
        public void OnClickReset() {
            var popupService = YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
            var sceneService = YisoServiceProvider.Instance.Get<IYisoSceneService>();
            var uiService = YisoServiceProvider.Instance.Get<IYisoUIService>();
            popupService.AlertS("경고", "데이터가 전부 지워집니다.\n계속하시겠습니까?", () => {
                YisoServiceProvider.Instance.Get<IYisoCharacterService>().ResetPlayerData();
                if (sceneService.GetCurrentScene() == YisoSceneTypes.INIT) {
                    sceneService.LoadScene(YisoSceneTypes.STORY);
                    return;
                }
                
                sceneService.LoadScene(YisoSceneTypes.INIT);
                onCloseEvent?.Invoke();
            });
        }
    }
}