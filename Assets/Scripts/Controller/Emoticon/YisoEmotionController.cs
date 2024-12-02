using System;
using System.Collections;
using System.Collections.Generic;
using Core.Behaviour;
using DG.Tweening;
using MEC;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.ObjectId;

namespace Controller.Emoticon {
    [AddComponentMenu("Yiso/Controller/Emoticon/Emotion Controller")]
    public class YisoEmotionController : RunIBehaviour {
        [SerializeField] protected Sprite emptyEmotion;
        [SerializeField] protected SpriteRenderer iconSpriteRenderer;
        [SerializeField] protected Emotion[] emotions;
        [SerializeField] protected float duration = 0.2f;

        protected float HalfDuration => duration / 2f;
        protected bool IsShowing => showing;

        protected string key;
        protected bool showing = false;
        protected SpriteRenderer spriteRenderer;
        protected IEnumerator emotionCoroutine = null;
        protected readonly Dictionary<EmotionType, Emotion> emotionDict = new();

        public virtual void Initialization() {
            transform.localPosition = new Vector3(0f, 1.6f);
            key = YisoObjectID.GenerateString();
            foreach (var emotion in emotions) {
                if (emotion.sequence) {
                    var sequences = new Sprite[emotion.icons.Length + 1];
                    sequences[0] = emptyEmotion;
                    for (int i = 1, index = 0; i < sequences.Length; i++, index++) {
                        sequences[i] = emotion.icons[index];
                    }

                    emotion.icons = sequences;
                }

                emotionDict[emotion.type] = emotion;
            }

            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = false;
            if (iconSpriteRenderer != null) iconSpriteRenderer.enabled = false;
        }

        public void ShowEmoticon(EmotionType type, Func<bool> stopPredicate, Action onComplete = null) {
            var emotion = emotionDict[type];
            if (showing) {
                CancelEmotion();
                StopCoroutine(emotionCoroutine);
            }

            if (emotion.sequence) {
                emotionCoroutine = DOEmotions(type, stopPredicate, onComplete);
                return;
            }

            emotionCoroutine = DOEmotion(type, stopPredicate, onComplete);
            StartCoroutine(emotionCoroutine);
        }

        public void ShowEmoticon(Sprite sprite, Func<bool> stopPredicate, Action onComplete = null) {
            if (showing) {
                CancelEmotion();
                StopCoroutine(emotionCoroutine);
            }

            emotionCoroutine = DOEmotion(sprite, stopPredicate, onComplete);
            StartCoroutine(emotionCoroutine);
        }

        public virtual void CancelEmotion(Action onComplete = null) {
            spriteRenderer.enabled = false;
            showing = false;
            StopCoroutine(emotionCoroutine);
        }

        private IEnumerator DOEmotions(EmotionType type, Func<bool> stopPredicate, Action onComplete = null) {
            showing = true;
            var sequenceEmotions = emotionDict[type].icons;
            yield return DOShowEmotion(sequenceEmotions[0]);
            Timing.RunCoroutine(DOAnimateEmotion(sequenceEmotions), key);

            while (!stopPredicate()) {
                yield return null;
            }

            Timing.KillCoroutines(key);
            yield return DOHideEmotion();
            showing = false;
            onComplete?.Invoke();
        }

        private IEnumerator DOEmotion(EmotionType type, Func<bool> stopPredicate, Action onComplete = null) {
            showing = true;
            var emotion = emotionDict[type];

            yield return DOShowEmotion(emotion.icon);
            while (!stopPredicate()) {
                yield return null;
            }

            yield return DOHideEmotion();
            showing = false;
            onComplete?.Invoke();
        }

        private IEnumerator DOEmotion(Sprite icon, Func<bool> stopPredicate, Action onComplete = null) {
            showing = true;

            yield return DOShowCustomEmotion(icon);
            while (!stopPredicate()) {
                yield return null;
            }

            yield return DOHideEmotion();
            showing = false;
            onComplete?.Invoke();
        }

        private IEnumerator DOShowCustomEmotion(Sprite icon) {
            if (iconSpriteRenderer == null) yield break;

            // Set Icon
            iconSpriteRenderer.sprite = icon;
            spriteRenderer.sprite = emptyEmotion;

            var bounds = spriteRenderer.sprite.bounds;
            var iconBounds = iconSpriteRenderer.sprite.bounds;

            var padding = 0.1f;
            var availableWidth = bounds.size.x * (1 - 2 * padding);
            var availableHeight = bounds.size.y * (1 - 2 * padding);

            var iconWidth = iconBounds.size.x;
            var iconHeight = iconBounds.size.y;
            var widthRatio = availableWidth / iconWidth;
            var heightRatio = availableHeight / iconHeight;
            var scaleRatio = Mathf.Min(widthRatio, heightRatio);

            spriteRenderer.transform.localScale = Vector3.zero;
            iconSpriteRenderer.transform.localScale = new Vector3(scaleRatio, scaleRatio, 1);
            iconSpriteRenderer.transform.localPosition = new Vector3(0, 0.06f, 0f);

            // Enable Icon
            spriteRenderer.enabled = true;
            iconSpriteRenderer.enabled = true;

            yield return spriteRenderer.transform.DOScale(1.25f, HalfDuration).WaitForCompletion();
            yield return spriteRenderer.transform.DOScale(Vector3.one, HalfDuration).WaitForCompletion();
        }

        private IEnumerator DOShowEmotion(Sprite icon) {
            spriteRenderer.transform.localScale = Vector3.zero;
            spriteRenderer.sprite = icon;
            spriteRenderer.enabled = true;
            yield return spriteRenderer.transform.DOScale(1.25f, HalfDuration).WaitForCompletion();
            yield return spriteRenderer.transform.DOScale(Vector3.one, HalfDuration).WaitForCompletion();
        }

        private IEnumerator DOHideEmotion() {
            yield return spriteRenderer.transform.DOScale(1.25f, HalfDuration).WaitForCompletion();
            yield return spriteRenderer.transform.DOScale(Vector3.zero, HalfDuration).WaitForCompletion();
            if (iconSpriteRenderer != null) iconSpriteRenderer.enabled = false;
            spriteRenderer.enabled = false;
        }

        private IEnumerator<float> DOAnimateEmotion(Sprite[] icons) {
            while (true) {
                foreach (var icon in icons) {
                    spriteRenderer.sprite = icon;
                    yield return Timing.WaitForSeconds(0.4f);
                }
            }
        }

        public enum EmotionType {
            EMPTY,
            SAD,
            ANGRY,
            SMILE,
            LAUGH,
            QUESTION_MARK,
            ALERT_MARK_BLACK,
            ALERT_MARK_RED,
            CONVERSATION,
            PROGRESS_1,
            PROGRESS_2,
            PROGRESS_3,
            STAR_MARK,
            ATTENTION
        }

        [Serializable]
        public class Emotion {
            public EmotionType type;
            public bool sequence = false;
            [HideIf("sequence")] public Sprite icon;
            [ShowIf("sequence")] public Sprite[] icons;
        }
    }
}