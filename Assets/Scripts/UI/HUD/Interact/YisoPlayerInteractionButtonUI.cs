using System;
using Core.Behaviour;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.HUD.Interact {
    public class YisoPlayerInteractionButtonUI : RunIBehaviour {

        private Button button;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;

        private UnityAction onClick = null;

        protected override void Start() {
            button = GetComponent<Button>();
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = (RectTransform) transform;

            button.OnClickAsObservable().Throttle(TimeSpan.FromMilliseconds(50))
                .Subscribe(_ => onClick?.Invoke())
                .AddTo(this);
        }

        public bool IsVisible => canvasGroup.IsVisible();

        public void Visible(bool flag, UnityAction onClick = null) {
            if (!flag) {
                this.onClick = null;
                canvasGroup.Visible(false);
                return;
            }

            this.onClick = onClick;
            canvasGroup.DOVisible(1f, .25f);
            rectTransform.DOScale(1.05f, 0.2f)
                .OnComplete(() => rectTransform.DOScale(1f, 0.2f));
        }
    }
}