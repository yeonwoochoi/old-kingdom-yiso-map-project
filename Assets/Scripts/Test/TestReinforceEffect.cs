using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace Test {
    public class TestReinforceEffect : MonoBehaviour {
        public Image itemImage;

        [Title("Normal Success Settings")] public float blendingValue = 0.8f;
        public float duration = 1f;
        public float scaleValue = 1.1f;
        [Title("Normal Failure Settings")] public float strength = 1f;
        public int vibrato = 10;
        public bool useScale = false;

        private Material material;
        private RectTransform rectTransform;

        private void Start() {
            material = itemImage.material;
            rectTransform = itemImage.rectTransform;
        }

        [Button]
        public void TestNormalSuccess() {
            StartCoroutine(DOSuccess());
        }

        [Button]
        public void TestNormalFailure() {
            if (useScale) {
                rectTransform.DOShakeScale(duration, strength: strength, vibrato: vibrato);
            } else {
                rectTransform.DOShakePosition(duration, strength: strength, vibrato: vibrato);
            }
            
            // Use Position and strength: 10, vibrato: 10
        }
        
        private IEnumerator DOSuccess() {
            material.ActiveHitEffect(true);
            material.DOHit(Color.white, blendingValue, duration / 2f);
            yield return rectTransform.DOScale(scaleValue, duration / 2f).WaitForCompletion();
            
            material.DOHit(Color.white, 0f, duration / 2f);
            yield return rectTransform.DOScale(1f, duration / 2f).WaitForCompletion();
            material.ActiveHitEffect(false);
        }
    }
}