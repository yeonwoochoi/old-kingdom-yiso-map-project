using System.Collections.Generic;
using MEC;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace Test.UI.AutoPotion {
    public class TestAutoPotionCooldownUI : MonoBehaviour {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image cooldownProgressImage;
        [SerializeField] private TextMeshProUGUI cooldownText;


        public void ClearCoolDown() {
            canvasGroup.Visible(false);
            cooldownProgressImage.fillAmount = 1f;
            cooldownText.SetText("");
        }

        public void StartCooldown(float duration, UnityAction<float> onProgress = null, UnityAction onComplete = null) {
            canvasGroup.Visible(true);
            Timing.RunCoroutine(DOCool(duration, onProgress, onComplete));
        }

        private IEnumerator<float> DOCool(float duration, UnityAction<float> onProgress = null, UnityAction onComplete = null) {
            var currentDuration = duration;
            while (currentDuration > 0) {
                currentDuration -= Time.deltaTime;
                var progress = currentDuration / duration;
                onProgress?.Invoke(progress);
                cooldownProgressImage.fillAmount = progress;
                cooldownText.SetText((currentDuration % 60).ToString("0"));
                yield return Timing.WaitForOneFrame;
            }
            
            onComplete?.Invoke();
            ClearCoolDown();
        }
    }
}