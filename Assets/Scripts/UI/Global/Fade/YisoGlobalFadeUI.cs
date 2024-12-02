using System;
using Core.Behaviour;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using Utils.Extensions;

namespace UI.Global.Fade {
    public class YisoGlobalFadeUI : RunIBehaviour {
        private CanvasGroup fadeCanvas;

        private Sequence sequence;

        protected override void Start() {
            base.Start();
            fadeCanvas = GetComponent<CanvasGroup>();
        }

        public TweenerCore<float, float, FloatOptions> DOFade(bool flag, float duration) {
            var value = flag ? 1f : 0f;
            return fadeCanvas.DOVisible(value, duration);
        }

        public void Fade(bool flag, float duration, Action onComplete = null) {
            var value = flag ? 1f : 0f;
            fadeCanvas.DOVisible(value, duration).OnComplete(() => onComplete?.Invoke());
        }

        public void FadeInOut(float duration, Action onFadeIn = null, Action onComplete = null) {
            sequence ??= DOTween.Sequence()
                .Append(fadeCanvas.DOFade(1f, duration / 2f))
                .Append(fadeCanvas.DOFade(0f, duration / 2f))
                .SetAutoKill(false);

            sequence.InsertCallback(duration / 2f, () => onFadeIn?.Invoke());
            sequence.OnComplete(() => onComplete?.Invoke());
            sequence.Restart();
        }
    }
}