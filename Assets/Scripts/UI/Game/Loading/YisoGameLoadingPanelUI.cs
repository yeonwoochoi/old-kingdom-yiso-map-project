using System;
using Core.Behaviour;
using DG.Tweening;
using UnityEngine;
using Utils.Extensions;

namespace UI.Game.Loading {
    public class YisoGameLoadingPanelUI : RunIBehaviour {
        private CanvasGroup canvas;

        protected override void Start() {
            canvas = GetComponent<CanvasGroup>();
        }

        public void Visible(bool flag, Action callback = null) {
            var value = flag ? 1f : 0f;
            canvas.DOVisible(value, .25f)
                .OnComplete(() => callback?.Invoke());
        }
    }
}