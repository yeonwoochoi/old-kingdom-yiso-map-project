using System.Collections.Generic;
using System.Linq;
using Core.Domain.Types;
using DG.Tweening;
using TMPro;
using UI.Game.Base;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Game.Floating {
    public class YisoGameFloatingTextContentUI : YisoGameBasePanelUI {
        [SerializeField] private List<TextMeshProUGUI> floatingTexts;

        private readonly List<RectTransform> floatingTextRects = new();
        private readonly ReactiveCollection<bool> floatingTextFlags = new();

        protected override void Start() {
            base.Start();
            floatingTexts.ForEach(text => {
                floatingTextRects.Add((RectTransform)text.transform);
                floatingTextFlags.Add(false);
            });
        }

        protected override void HandleData(object data) {
            var (value, onComplete) = ((string, UnityAction)) data;
            Floating(value, onComplete);
        }

        private void Floating(string value, UnityAction onComplete = null) {
            if (!floatingTextFlags.ToList().Exists(flag => !flag)) return;
            var existIndex = floatingTextFlags.ToList().FindIndex(flag => !flag);
            floatingTextFlags[existIndex] = true;
            floatingTexts[existIndex].text = value;
            floatingTextRects[existIndex].DOAnchorPosY(100f, 2f)
                .OnComplete(() => {
                    floatingTexts[existIndex].text = "";
                    floatingTextRects[existIndex].DOAnchorPosY(0f, 0f);
                    floatingTextFlags[existIndex] = false;
                    onComplete?.Invoke();
                });
        }

        public override YisoGameUITypes GetUIType() => YisoGameUITypes.FLOATING_TEXT;
    }
}