using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Behaviour;
using DG.Tweening;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Utils;
using Utils.Extensions;

namespace UI.Global.FloatingText {
    public class YisoGlobalFloatingTextUI : RunIBehaviour {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private List<TextMeshProUGUI> floatingTexts;
        [SerializeField] private GameObject prefab;
        
        private readonly List<Item> items = new();
        private readonly Queue<(string, Color)> queue = new();
        private IEnumerator updateCoroutine = null;

        protected override void Start() {
            items.AddRange(floatingTexts.Select(text => new Item(text)));
        }

        public void AddFloating(string text, Color color) {
            queue.Enqueue((text, color));
        }

        protected override void OnEnable() {
            base.OnEnable();
            updateCoroutine = DOUpdate();
            StartCoroutine(updateCoroutine);
        }

        protected override void OnDisable() {
            base.OnDisable();
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }

        public override void OnUpdate() {
            canvasGroup.alpha = ExistActiveItem() ? 1f : 0f;
        }

        private IEnumerator DOUpdate() {
            while (true) {
                if (queue.TryDequeue(out var queueItem)) {
                    var (text, color) = queueItem;
                
                    if (!TryGetNonActiveItem(out var index)) {
                        var newText = CreateItem();
                        items.Add(new Item(newText));
                        index = items.Count - 1;
                    }

                    yield return YieldInstructionCache.WaitForSeconds(0.1f);

                    items[index].Active = true;
                    items[index].SetText(text, color);
                    items[index].Rect.DOAnchorPosY(100f, 2f)
                        .OnComplete(() => {
                            items[index].Clear();
                        });
                }

                yield return null;
            }
        }

        private bool TryGetNonActiveItem(out int index) {
            index = -1;

            for (var i = 0; i < items.Count; i++) {
                if (items[i].Active) continue;
                index = i;
                return true;
            }

            return false;
        }

        private bool ExistActiveItem() => items.Any(item => item.Active);
        
        private TextMeshProUGUI CreateItem() {
            return CreateObject<TextMeshProUGUI>(prefab, transform);
        }

        private class Item {
            private readonly TextMeshProUGUI text;

            public RectTransform Rect => text.rectTransform;

            public bool Active { get; set; } = false;

            public Item(TextMeshProUGUI text) {
                this.text = text;
            }

            public void SetText(string text, Color color) {
                this.text.SetText(text);
                this.text.color = color;
            }

            public void Clear() {
                text.SetText("");
                Rect.DOAnchorPosY(0f, 0f);
                Active = false;
            }
        }
    }
}