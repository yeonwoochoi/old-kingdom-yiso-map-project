using Core.Behaviour;
using Core.Domain.Locale;
using Core.Service;
using Core.Service.Data;
using TMPro;
using UI.Base;
using UI.Init.Setting.Difficulty;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Init.Setting {
    public class YisoInitDifficultySettingsPanelUI : YisoUIController {
        [SerializeField] private YisoInitDifficultySliderUI sliderUI;
        [SerializeField] private TextMeshProUGUI difficultyValueText;
        [SerializeField] private TextMeshProUGUI difficultyDescriptionText;

        private IYisoDataService dataService;

        protected override void Awake() {
            base.Awake();
            dataService = YisoServiceProvider.Instance.Get<IYisoDataService>();
        }

        protected override void OnEnable() {
            base.OnEnable();
            sliderUI.OnDifficultyChangedEvent += OnSliderDifficultyChanged;
        }

        protected override void Start() {
            var gameData = dataService.GetGameData();
            var value = gameData.settingData.difficulty;
            value = NormalizeDifficulty(value);
            sliderUI.SetDifficulty(value);
        }

        protected override void OnDisable() {
            base.OnDisable();
            sliderUI.OnDifficultyChangedEvent -= OnSliderDifficultyChanged;
        }


        private float ConvertToDifficulty(float value) => value + 0.5f;
        private float NormalizeDifficulty(float value) => value - 0.5f;

        private void SetDescription(float value) {
            var difficulty = CurrentLocale == YisoLocale.Locale.KR ? "쉬움" : "Easy";
            if (value >= 0.9 && value <= 1.1) {
                difficulty = CurrentLocale == YisoLocale.Locale.KR ? "보통" : "Normal";
            }
            else if (value > 1.1) {
                difficulty = CurrentLocale == YisoLocale.Locale.KR ? "어려움" : "Hard";
            }

            difficultyDescriptionText.SetText(difficulty);
        }

        private void OnSliderDifficultyChanged(float value) {
            var convertedValue = ConvertToDifficulty(value);
            difficultyValueText.SetText(convertedValue.ToString("F1"));
            SetDescription(convertedValue);
            var gameData = dataService.GetGameData();
            gameData.settingData.SetDifficulty(convertedValue);
#if UNITY_EDITOR
#else
            dataService.SaveGameData();
#endif
        }
    }
}