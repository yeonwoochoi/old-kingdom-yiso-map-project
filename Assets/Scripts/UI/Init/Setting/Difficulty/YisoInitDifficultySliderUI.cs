using Core.Behaviour;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Init.Setting.Difficulty {
    public class YisoInitDifficultySliderUI : RunIBehaviour, IPointerUpHandler {
        public event UnityAction<float> OnDifficultyChangedEvent; 
        private Slider slider;
        private float cachedValue;

        protected override void Awake() {
            slider = GetComponent<Slider>();
        }

        protected override void Start() {
            cachedValue = slider.value;
        }

        public void SetDifficulty(float difficulty) {
            slider.value = difficulty;
            cachedValue = difficulty;
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (Mathf.Approximately(slider.value, cachedValue)) return;
            cachedValue = slider.value;
            OnDifficultyChangedEvent?.Invoke(cachedValue);
        }
    }
}