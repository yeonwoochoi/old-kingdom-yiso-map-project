using System;
using BaseCamp.Animation;
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

namespace Test {
    public class TestInteractObject : RunIBehaviour {
        [SerializeField] private TextMeshPro objectText;
        [SerializeField, Title("Settings")] private YisoInteractTypes type;
        [SerializeField] private bool isNpc;
        [SerializeField, ShowIf("isNpc")] private Animator animator;
        [SerializeField] private bool hasId;
        [SerializeField, ShowIf("hasId")] private int id;

        private readonly YisoServiceProvider serviceProvider = YisoServiceProvider.Instance;
        private YisoBaseCampNpcAnimator npcAnimator;

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
            serviceProvider.Get<IYisoHUDUIService>().ShowInteractButton(YisoHudUIInteractTypes.SPEECH,
                () => {
                    var data = hasId ? (object)id : null;
                    serviceProvider.Get<IYisoUIService>().ShowInteractUI(type, data);
                });
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;
            objectText.gameObject.SetActive(true);
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>().HideInteractButton(YisoHudUIInteractTypes.SPEECH);
        }

        protected override void OnDisable() {
            base.OnDisable();
            objectText.gameObject.SetActive(false);
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>().HideInteractButton(YisoHudUIInteractTypes.SPEECH);
        }
    }
}