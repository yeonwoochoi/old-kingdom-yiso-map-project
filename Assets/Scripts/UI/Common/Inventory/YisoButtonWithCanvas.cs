using Core.Behaviour;
using TMPro;
using UI.Effects;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Common.Inventory {
    [RequireComponent(typeof(CanvasGroup), typeof(YisoUIHoldEffect))]
    public class YisoButtonWithCanvas : Button {
        private TextMeshProUGUI buttonText;
        private CanvasGroup canvasGroup;
        
        protected override void Awake() {
            base.Awake();
            canvasGroup = GetComponent<CanvasGroup>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            transition = Transition.None;
        }

        public void SetText(string text) {
            buttonText.SetText(text);
        }

        public bool Visible {
            get => canvasGroup.IsVisible();
            set => canvasGroup.Visible(value);
        }
    }
}