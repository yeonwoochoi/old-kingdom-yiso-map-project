using System;
using BaseCamp.Animation;
using BaseCamp.Manager;
using BaseCamp.Player;
using Core.Behaviour;
using Core.Domain.Types;
using Core.Service;
using Core.Service.UI;
using Core.Service.UI.HUD;
using Domain.Direction;
using Sirenix.OdinInspector;
using TMPro;
using UI.HUD.Interact;
using UnityEngine;

namespace BaseCamp {
    [RequireComponent(typeof(Collider2D))]
    public class YisoBaseCampInteractObject : RunIBehaviour {
        [SerializeField] private TextMeshPro objectText;
        [SerializeField, Title("Settings")] private YisoInteractTypes type;
        [SerializeField] private bool isNpc;
        [SerializeField, ShowIf("isNpc")] private Animator animator;
        [SerializeField] private bool hasId;
        [SerializeField, ShowIf("hasId")] private int id;

        private YisoBaseCampPlayerController controller;
        private YisoBaseCampNpcAnimator npcAnimator;
        private IYisoUIService uiService;

        protected override void Start() {
            base.Start();
            if (!isNpc) return;
            npcAnimator = new YisoBaseCampNpcAnimator(animator) {
                Direction = YisoObjectDirection.DOWN
            };
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;
            objectText.gameObject.SetActive(false);
            /*controller ??= other.GetComponent<YisoBaseCampPlayerController>();
            uiService ??= YisoServiceProvider.Instance.Get<IYisoUIService>();
            uiService.ActiveInteractButtonOnBaseCamp(true, OnInteract);
            controller.interactInfo.OnClick = OnInteract;*/
            
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>().ShowInteractButton(YisoHudUIInteractTypes.SPEECH, OnInteract);
        }
        
        private void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;
            objectText.gameObject.SetActive(true);
            /*uiService ??= YisoServiceProvider.Instance.Get<IYisoUIService>();
            uiService.ActiveInteractButtonOnBaseCamp(false);
            controller ??= other.GetComponent<YisoBaseCampPlayerController>();
            controller.interactInfo.Reset();*/
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>().HideInteractButton(YisoHudUIInteractTypes.SPEECH);
        }

        private void OnInteract() {
            var data = hasId ? (object)id : null;
            YisoServiceProvider.Instance.Get<IYisoUIService>().ShowInteractUI(type, data);
        }
    }
}