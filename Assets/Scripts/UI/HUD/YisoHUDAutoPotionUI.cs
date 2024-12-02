using System.Collections.Generic;
using Core.Behaviour;
using MEC;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.HUD {
    public class YisoHUDAutoPotionUI : YisoPlayerUIController {
        [SerializeField, Title("Duration")] private float cooldownDuration = 4f;
        [SerializeField] private float refillDuration = 5f;
        [SerializeField, Title("Count")] private int maxCount = 3;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField, Title("Cooldown")] private CanvasGroup cooldownCanvas;
        [SerializeField] private Image cooldownProgressImage;
        [SerializeField] private TextMeshProUGUI cooldownText;

        private Button potionButton;
        private int currentCount;
        private int targetIncreaseHp;
        private int startingHp;
        
        private bool isCool;

        protected override void OnEnable() {
            base.OnEnable();
            player.StatModule.OnHpChangedEvent += OnHpChanged;
        }

        protected override void Start() {
            potionButton = GetComponent<Button>();
            potionButton.onClick.AddListener(OnClickButton);
            currentCount = maxCount;
            SetCount();
        }

        protected override void OnDisable() {
            base.OnDisable();
            player.StatModule.OnHpChangedEvent -= OnHpChanged;
        }

        private void OnHpChanged(float progress) {
            if (isCool) return;
            potionButton.interactable = progress < 0.98;
        }

        private void OnClickButton() {
            StartCoroutine(DOCool());
        }

        private IEnumerator<float> DOCool() {
            currentCount--;
            SetCount();

            startingHp = GetHp();
            targetIncreaseHp = GetMaxHp() - GetHp();
            isCool = true;
            potionButton.interactable = false;
            
            cooldownCanvas.Visible(true);

            yield return Timing.WaitUntilDone(Timing.RunCoroutine(DOCool(cooldownDuration, OnCooldown)));

            if (currentCount > 0) {
                ClearCooldown();
                isCool = false;
                potionButton.interactable = true;
                yield break;
            }

            for (var i = 0; i < maxCount; i++) {
                yield return Timing.WaitUntilDone(Timing.RunCoroutine(DOCool(refillDuration)));
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

        private void OnCooldown(float progress) {
            var increaseValue = Mathf.Lerp(0, targetIncreaseHp, 1 - progress);
            var newHp = startingHp + increaseValue;
            newHp = Mathf.Min(newHp, GetMaxHp());
            player.StatModule.SetHp(Mathf.RoundToInt(newHp));
        }

        private void ClearCooldown() {
            cooldownCanvas.Visible(false);
            cooldownProgressImage.fillAmount = 1f;
            cooldownText.SetText("");
        }

        private void SetCount() {
            countText.SetText($"x{currentCount}");
        }

        private int GetHp() => player.StatModule.Hp;
        private int GetMaxHp() => player.StatModule.MaxHp;
    }
}