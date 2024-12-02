using System;
using Core.Behaviour;
using Core.Service;
using Core.Service.Character;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.HUD {
    public class YisoPlayerHUDBasicInfoUI : RunIBehaviour {
        [SerializeField, Title("Components")] private Image hpBar;
        [SerializeField] private TextMeshProUGUI hpBarText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image expBar;
        [SerializeField] private TextMeshProUGUI expText;

        private IYisoCharacterService characterService;

        protected override void Awake() {
            base.Awake();
            characterService = YisoServiceProvider.Instance.Get<IYisoCharacterService>();
        }

        protected override void OnEnable() {
            base.OnEnable();
            characterService.GetPlayer().StatModule.OnHpChangedEvent += OnHPChanged;
        }

        protected override void Start() {
            this.ObserveEveryValueChanged(t => t.hpBar.fillAmount)
                .Subscribe(OnHpFillAmountChanged)
                .AddTo(this);

            this.ObserveEveryValueChanged(t => t.expBar.fillAmount)
                .Subscribe(OnExpFillAmountChanged)
                .AddTo(this);
        }

        protected override void OnDisable() {
            base.OnDisable();
            characterService.GetPlayer().StatModule.OnHpChangedEvent -= OnHPChanged;
        }
        
        private void OnLevelChanged(int value) {
            value = Mathf.Min(100, value);
            value = Mathf.Max(1, value);
            var text = $"Lv. {value}";
            levelText.SetText(text);
        }

        private void OnHPChanged(float value) {
            value = Mathf.Min(1, value);
            value = Mathf.Max(0, value);
            hpBar.DOFillAmount(value, 0.2f);
        }

        private void OnExpChanged(double value) {
            expBar.DOFillAmount((float) value, 0.2f);
        }

        private void OnExpFillAmountChanged(float value) {
            var text = (value * 100).ToPercentage();
            expText.SetText(text);
        }
        
        private void OnHpFillAmountChanged(float value) {
            var text = (value * 100).ToPercentage();
            hpBarText.SetText(text);
        }
    }
}