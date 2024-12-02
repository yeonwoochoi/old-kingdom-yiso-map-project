using System;
using System.Collections.Generic;
using MEC;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Test.UI.Timer {
    public class TestTimerPanelUI : MonoBehaviour {
        public Image progressBar;
        public TextMeshProUGUI timerText;

        private string timerTag = "TIMER";

        private void Start() {
            progressBar.fillAmount = 1;
            DisplayTime(0);
        }

        [Button]
        public void StartTimer(int time = 60) {
            Timing.RunCoroutine(DOTimer(time).CancelWith(gameObject), timerTag);
        }

        [Button]
        public void ResetTimer() {
            Timing.KillCoroutines(timerTag);
            DisplayTime(0);
        }

        private IEnumerator<float> DOTimer(int time) {
            var current = (float) time;
            while (current > 0) {
                current -= Time.deltaTime;
                var progress = current / time;
                progressBar.fillAmount = progress;
                DisplayTime(current);
                yield return Timing.WaitForOneFrame;
            }

            progressBar.fillAmount = 0;
            DisplayTime(0);
        }

        private void DisplayTime(float time) {
            var minutes = Mathf.FloorToInt(time / 60);
            var seconds = Mathf.FloorToInt(time % 60);
            if (minutes >= 100) {
                timerText.SetText($"{minutes:D3}:{seconds:D2}");
            } else {
                timerText.SetText($"{minutes:D2}:{seconds:D2}");
            }
        }
    }
}