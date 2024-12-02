using System;
using System.Diagnostics.CodeAnalysis;
using Core.Behaviour;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Components {
    [RequireComponent(typeof(Button))]
    public class YisoHoldButtonUI : RunIBehaviour {
        [Title("Fill Image")] [SerializeField, Required] private Image fillImage;
        [Title("Settings")] [SerializeField] private float duration = 3f;
        [SerializeField] private float initValue = 0f;

        private RectTransform rectTransform;
        private Button button;
        private Image buttonImage;

        private bool needHold = true;
        private bool isPressed = false;
        private float timeHolding = 0f;
        private float currentValue = 0f;
        private float currentAlphaValue = 0f;
        private float initAlphaValue;
        private bool isDone = false;
        private UnityAction onClick = null;
        private Color initColor;
        private Color buttonImageColor;

        protected override void Start() {
            initColor = fillImage.color;
            initAlphaValue = initColor.a;
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();
            buttonImageColor = buttonImage.color;
            rectTransform = (RectTransform) transform;
            
            button.OnPointerDownAsObservable()
                .Where(_ => button.interactable)
                .Subscribe(OnButtonDown).AddTo(this);

            button.OnPointerUpAsObservable()
                .Where(_ => button.interactable)
                .Subscribe(OnButtonUp).AddTo(this);
        }

        public void SetButton(bool needHold, [NotNull] Action onClick) {
            this.onClick = () => {
                needHold = false;
                onClick?.Invoke();
                this.onClick = null;
            };
            
            this.needHold = needHold;
        }

        public override void OnUpdate() {
            if (!isPressed) return;
            if (!needHold) return;

            if (Mathf.Approximately(currentValue, 1f) && Mathf.Approximately(currentAlphaValue, 1f) && !isDone) {
                onClick();
                isDone = true;
                return;
            }
            
            timeHolding += Time.deltaTime;
            currentValue = Mathf.Lerp(0.0f, 1.0f, Mathf.Clamp(timeHolding / duration, 0f, 1f));
            currentAlphaValue = Mathf.Lerp(initAlphaValue, 1.0f, Mathf.Clamp(timeHolding / duration, 0f, 1f));
            fillImage.fillAmount = currentValue;
            buttonImageColor.a = currentAlphaValue;
            buttonImage.color = buttonImageColor;
        }

        private void OnButtonDown(PointerEventData eventData) {
            rectTransform.localScale = new Vector3(0.95f, 0.95f);
            var color = fillImage.color;
            buttonImageColor = buttonImage.color;
            color.a = 1f;
            fillImage.color = color;
            isPressed = true;
        }

        private void OnButtonUp(PointerEventData eventData) {
            if (!needHold) {
                onClick?.Invoke();
            }
            rectTransform.localScale = Vector3.one;
            isPressed = false;
            ResetHolding();
        }
        
        private void ResetHolding() {
            timeHolding = 0f;
            currentValue = initValue;
            currentAlphaValue = initAlphaValue;
            fillImage.fillAmount = currentValue;
            
            fillImage.color = initColor;
            buttonImage.color = initColor;
            isDone = false;
        }
    }
}