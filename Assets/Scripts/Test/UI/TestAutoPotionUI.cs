using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using Test.UI.AutoPotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Extensions;

namespace Test.UI {
    public class TestAutoPotionUI : MonoBehaviour {
        [SerializeField] private Button potionButton;
        [SerializeField] private Image hpImage;
        [SerializeField] private float duration;
        [SerializeField] private float fillDuration = 15f;
        [SerializeField] private int maxCount = 3;
        [SerializeField] private TestAutoPotionCooldownUI cooldownUI;
        [SerializeField] private TextMeshProUGUI countText;

        [SerializeField, Title("Sample")] private int maxHp;
        [SerializeField] private TextMeshProUGUI hpText;

        private int hp;
        private int targetIncreaseHp;
        private float startingHp;
        private bool isCool;
        private int currentCount;

        private void Start() {
            potionButton.onClick.AddListener(OnClickButton);
            currentCount = maxCount;
            SetCount();
            SetHp(maxHp);
        }

        private void SetHp(int hp) {
            if (this.hp == hp) return;
            this.hp = hp;
            var progress = this.hp / (float) maxHp;
            // hpImage.fillAmount = progress;
            hpImage.DOFillAmount(progress, 0.25f);
            hpText.SetText(this.hp.ToCommaString());
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

                Debug.Log($"Damaged!({hp * randomValue}) New HP: {newHp}, targetIncreaseHp: {targetIncreaseHp}, ProgressRecovery: {progressRecovery}, remainingRecovery: {remainingRecovery}");
            }
            
            SetHp(newHp);
        }

        private void OnClickButton() {
            currentCount--;
            SetCount();

            startingHp = hp;
            targetIncreaseHp = maxHp - hp;
            isCool = true;

            potionButton.interactable = false;
            Debug.Log($"Start HP Recovery, {startingHp} to {targetIncreaseHp + startingHp}");
            cooldownUI.StartCooldown(duration, OnCoolDown, () => {
                if (currentCount > 0) {
                    potionButton.interactable = true;
                    isCool = false;
                    return;
                }
                cooldownUI.StartCooldown(fillDuration, onComplete: () => {
                    currentCount = 3;
                    SetCount();
                    potionButton.interactable = true;
                    isCool = false;
                });
            });
        }

        
        private void OnCoolDown(float progress) {
            // progress에 따라 targetIncreaseHp 양만큼 회복
            var increaseValue = Mathf.Lerp(0, targetIncreaseHp, 1 - progress);  
            var newHp = startingHp + increaseValue;  // 시작 체력에 회복된 양을 더함

            newHp = Mathf.Min(newHp, maxHp);
            // Debug.Log($"[{progress * 100}%] Target: {targetIncreaseHp}, Increase: {increaseValue}, Hp: {newHp}");
            SetHp(Mathf.RoundToInt(newHp)); 
        }

        private void SetCount() {
            countText.SetText($"x{currentCount}");
        }
    }
}