using System;
using System.Collections;
using System.Collections.Generic;
using Core.Behaviour;
using DG.Tweening;
using MEC;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Extensions;
using Utils.ObjectId;

namespace UI.Init.Loading {
    public class YisoInitLoadingProgressUI : RunIBehaviour {
        [SerializeField] private Image progressImage;
        [SerializeField] private TextMeshProUGUI progressText;

        private const float START_PROGRESS = 0f;

        public CanvasGroup PanelCanvas { get; private set; }

        protected override void Start() {
            this.ObserveEveryValueChanged(t => t.progressImage.fillAmount)
                .Subscribe(OnChangeProgress)
                .AddTo(this);

            progressImage.fillAmount = START_PROGRESS;
            PanelCanvas = GetComponent<CanvasGroup>();
            PanelCanvas.Visible(false);
        }
        
        public IEnumerator DOProgress(float duration, Func<bool> predicate) {
            var randomStop = Randomizer.Next(0.8f, 0.95f);
            var elapsed = 0f;
            while (elapsed < duration) {
                if (progressImage.fillAmount >= randomStop) {
                    if (!predicate()) {
                        yield return null;
                        continue;
                    }
                }
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                var easedT = EaseInQuad(t);
                progressImage.fillAmount = easedT;
                yield return null;
            }

            progressImage.fillAmount = 1f;
            yield break;

            float EaseInQuad(float t) {
                return t * t;
            }
        }
        

        private void OnChangeProgress(float value) {
            var textValue = (value * 100f).ToPercentage();
            progressText.SetText(textValue);
        }
    }
}