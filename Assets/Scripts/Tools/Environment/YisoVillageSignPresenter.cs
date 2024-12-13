using Core.Behaviour;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using TMPro;
using UnityEngine;

namespace Tools.Environment {
    [AddComponentMenu("Yiso/Environment/Yiso Village Sign Displayer")]
    public class YisoVillageSignPresenter: RunIBehaviour {
        [SerializeField, Tooltip("The name of the village this sign points to.")]
        private string villageName;

        [SerializeField, Tooltip("TextMeshPro object to display the village name.")]
        private TextMeshPro signText;

        [SerializeField, Tooltip("Detect radius")]
        private float detectRadius = 5f;

        private CanvasGroup canvasGroup;
        private CircleCollider2D circleCollider2D;
            
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoVillageSignPresenter>();

        protected override void Awake() {
            base.Awake();
            if (signText == null) {
                LogService.Error("[YisoVillageSignPresenter.Awake] SignText is not assigned.");
                enabled = false;
                return;
            }

            if (!gameObject.TryGetComponent(out circleCollider2D)) {
                circleCollider2D = gameObject.AddComponent<CircleCollider2D>();
                circleCollider2D.isTrigger = true;
                circleCollider2D.radius = detectRadius;
            }

            signText.text = villageName;
            signText.gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (!collision.CompareTag("Player")) return;
            ShowText();
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (!collision.CompareTag("Player")) return;
            HideText();
        }

        private void ShowText() {
            signText.gameObject.SetActive(true);
        }

        private void HideText() {
            signText.gameObject.SetActive(false);
        }
    }
}