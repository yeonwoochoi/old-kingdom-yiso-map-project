using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Test.UI {
    public class TestPanelHolderUI : MonoBehaviour {
        [SerializeField] private Image image;
        [SerializeField] private GameObject toggleObject;
        [SerializeField] private GameObject panelObject;
        
        [SerializeField] private float toggleRate = 0.1f;
        
        [SerializeField] private VerticalLayoutGroup layoutGroup;
        private RectTransform rect;
        private RectTransform toggleRect;
        private RectTransform panelRect;

        private bool isOn = false;
        private bool toggleMode = false;
        
        private void Start() {
            rect = (RectTransform) transform;
            toggleRect = (RectTransform)toggleObject.transform;
            panelRect = (RectTransform)panelObject.transform;
        }

        [Button]
        public void ToggleMode(bool flag) {
            image.raycastTarget = flag;
        }

        private void ShowToggle() {
            toggleObject.SetActive(true);
            FitBoth();
        }

        private void HideToggle() {
            toggleObject.SetActive(false);
            FitOnly();
        }

        [Button]
        public void OnToggle() {
            isOn = !isOn;
            if (isOn) {
                ShowToggle();
            } else {
                HideToggle();
            }
        }

        private void FitOnly() {
            var width = GetWidth();
            SetWidth(panelRect, width);
        }

        private void FitBoth() {
            var width = GetWidth();
            var toggleWidth = width * toggleRate;
            var panelWidth = width - toggleWidth;
            SetWidth(toggleRect, toggleWidth);
            SetWidth(panelRect, panelWidth);
        }

        private void SetWidth(RectTransform rect, float width) {
            var size = rect.sizeDelta;
            size.x = width;
            rect.sizeDelta = size;
        }

        private float GetWidth() {
            var width = rect.rect.width;
            var padding = layoutGroup.padding.left + layoutGroup.padding.right;
            return width - padding;
        }
    }
}