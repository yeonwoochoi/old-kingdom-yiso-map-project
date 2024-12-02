using Core.Behaviour;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Init.Setting {
    public class YisoInitSoundSettingsPanelUI : RunIBehaviour {
        [SerializeField] private Slider backgroundMusicSlider;
        [SerializeField] private TextMeshProUGUI backgroundMusicValueText;
        [SerializeField] private Slider effectSoundSlider;
        [SerializeField] private TextMeshProUGUI effectSoundValueText;

        protected override void Start() {
            base.Start();
            backgroundMusicSlider.onValueChanged.AddListener(OnSliderValueChanged(backgroundMusicValueText));
            effectSoundSlider.onValueChanged.AddListener(OnSliderValueChanged(effectSoundValueText));
        }

        private UnityAction<float> OnSliderValueChanged(TextMeshProUGUI text) => value => {
            var intValue = Mathf.RoundToInt(value * 100);
            text.SetText(intValue.ToString());
        };
    }
}