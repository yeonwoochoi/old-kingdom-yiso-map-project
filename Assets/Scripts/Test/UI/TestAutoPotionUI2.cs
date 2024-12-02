using System;
using System.Collections.Generic;
using DG.Tweening;
using MEC;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;
using Utils.Extensions;

namespace Test.UI {
    public class TestAutoPotionUI2 : MonoBehaviour {
        [SerializeField] private Button potionButton;
        [SerializeField] private Image hpImage;
        [SerializeField] private float duration = 5f;
        [SerializeField] private float fillDuration = 5f;
        [SerializeField] private int maxCount = 3;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private int maxHp;

        [SerializeField, Title("Cool Down")] private CanvasGroup cooldownCanvas;
        [SerializeField] private Image cooldownProgressImage;
        [SerializeField] private TextMeshProUGUI cooldownText;

        private int hp;
        private int targetIncreaseHp;
        private float startingHp;
        private bool isCool;
        private int currentCount;

        private void Start() {
            potionButton.onClick.AddListener(OnClickPotion);
            currentCount = maxCount;
            SetCount();
            SetHp(maxHp);
        }

        public void OnClickDamage() {
            var randomValue = Randomizer.Next(0.1, 0.5);
            var afterHp = hp * (1 - randomValue);
            var newHp = Mathf.CeilToInt((float) afterHp);
            if (isCool) {
                // 이미 회복된 양 계산 (현재 체력과 시작 체력의 차이)
                var progressRecovery = Mathf.Max(0, hp - startingHp);  // 음수 방지
                var remainingRecovery = Mathf.Max(0, targetIncreaseHp - progressRecovery);  // 남은 회복량 계산

                // 새로운 회복 목표를 남은 회복량으로 업데이트
                targetIncreaseHp = (int) Mathf.Max(0, remainingRecovery);  
                startingHp = newHp;  // 데미지를 받은 후 새로운 체력 설정

                // Debug.Log($"Damaged!({hp * randomValue}) New HP: {newHp}, targetIncreaseHp: {targetIncreaseHp}, ProgressRecovery: {progressRecovery}, remainingRecovery: {remainingRecovery}");
            }
            
            SetHp(newHp);
        }

        private void OnClickPotion() {
            Timing.RunCoroutine(DOCool());
        }

        private IEnumerator<float> DOCool() {
            currentCount--;
            SetCount();

            startingHp = hp;
            targetIncreaseHp = maxHp - hp;
            isCool = true;
            potionButton.interactable = false;

            cooldownCanvas.Visible(true);

            yield return Timing.WaitUntilDone(Timing.RunCoroutine(DOCool(duration, OnCoolDown)));

            if (currentCount > 0) {
                ClearCooldown();
                isCool = false;
                potionButton.interactable = true;
                yield break;
            }

            for (var i = 0; i < maxCount; i++) {
                yield return Timing.WaitUntilDone(Timing.RunCoroutine(DOCool(fillDuration)));
                currentCount++;
                SetCount();
            }
            
            ClearCooldown();
            isCool = false;
            potionButton.interactable = true;
        }

        private IEnumerator<float> DOCool(float duration, UnityAction<float> onProgress = null) {
            var currentDuration = duration;
            while (currentDuration > 0) {
                currentDuration -= Time.deltaTime;
                var progress = currentDuration / duration;
                onProgress?.Invoke(progress);
                cooldownProgressImage.fillAmount = progress;
                cooldownText.SetText((currentDuration % 60).ToString("0"));
                yield return Timing.WaitForOneFrame;
            }
        }

        private void OnCoolDown(float progress) {
            // progress에 따라 targetIncreaseHp 양만큼 회복
            var increaseValue = Mathf.Lerp(0, targetIncreaseHp, 1 - progress);  
            var newHp = startingHp + increaseValue;  // 시작 체력에 회복된 양을 더함

            newHp = Mathf.Min(newHp, maxHp);
            // Debug.Log($"[{progress * 100}%] Target: {targetIncreaseHp}, Increase: {increaseValue}, Hp: {newHp}");
            SetHp(Mathf.RoundToInt(newHp)); 
        }

        private void ClearCooldown() {
            cooldownCanvas.Visible(false);
            cooldownProgressImage.fillAmount = 1f;
            cooldownText.SetText("");
        }

        private void SetHp(int hp) {
            this.hp = hp;
            hpImage.DOFillAmount(this.hp / (float)maxHp, 0.25f);
            hpText.SetText(this.hp.ToCommaString());
        }

        private void SetCount() {
            countText.SetText($"x{currentCount}");
        }
    }
}