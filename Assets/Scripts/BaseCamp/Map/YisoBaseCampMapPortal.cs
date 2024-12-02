using System;
using BaseCamp.Player;
using Core.Behaviour;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Game;
using Core.Service.Scene;
using Core.Service.Stage;
using Core.Service.UI;
using Core.Service.UI.Game;
using Core.Service.UI.HUD;
using Core.Service.UI.Popup2;
using UI.HUD.Interact;
using Unity.VisualScripting;
using UnityEngine;

namespace BaseCamp.Map {
    [RequireComponent(typeof(Collider2D))]
    public class YisoBaseCampMapPortal : RunIBehaviour {
        private YisoBaseCampPlayerController controller = null;
        private IYisoUIService uiService;
        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>().ShowInteractButton(YisoHudUIInteractTypes.SPEECH, OnInteract);
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>().HideInteractButton(YisoHudUIInteractTypes.SPEECH);
        }

        private void OnInteract() {
            var stageService = YisoServiceProvider.Instance.Get<IYisoStageService>();
            if (!stageService.ExistNextStage()) {
                YisoServiceProvider.Instance.Get<IYisoGameUIService>().FloatingText("왈왈! (다음 스테이지는 개발중입니다!)");
                return;
            }
            
            YisoServiceProvider.Instance.Get<IYisoSceneService>()
                .LoadScene(YisoSceneTypes.STORY);
        }
    }
}