using System;
using BaseCamp.Manager;
using BaseCamp.Player;
using Core.Behaviour;
using Core.Domain.Types;
using TMPro;
using UnityEngine;

namespace BaseCamp.Test {
    public class TestBootCampObject : RunIBehaviour {
        [SerializeField] private TextMeshPro objectText;

        private YisoBaseCampPlayerController controller;
         
        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;
            objectText.gameObject.SetActive(false);
            controller ??= other.GetComponent<YisoBaseCampPlayerController>();
            /*YisoBaseCampManager.Instance.UIModule.ActiveInteract(true, () => {
                YisoBaseCampManager.Instance.UIModule.VisibleInteract(YisoInteractTypes.STORAGE);
            });
            controller.interactInfo.Active = true;
            controller.interactInfo.OnClick = () => {
                YisoBaseCampManager.Instance.UIModule.VisibleInteract(YisoInteractTypes.STORAGE);
            };*/
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;
            /*objectText.gameObject.SetActive(true);
            if (YisoBaseCampManager.Instance != null)
                YisoBaseCampManager.Instance.UIModule.ActiveInteract(false);
            controller ??= other.GetComponent<YisoBaseCampPlayerController>();
            controller.interactInfo.Reset();*/
        }
    }
}