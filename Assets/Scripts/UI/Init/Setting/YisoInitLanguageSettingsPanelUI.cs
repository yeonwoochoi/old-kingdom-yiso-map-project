using Core.Behaviour;
using Core.Domain.Locale;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Init.Setting {
    public class YisoInitLanguageSettingsPanelUI : RunIBehaviour {
        [SerializeField] private Toggle[] tabs;

        private const int KOR_INDEX = 0;
        private const int EN_INDEX = 1;

        protected override void Start() {
            for (var i = 0; i < tabs.Length; i++)
                tabs[i].onValueChanged.AddListener(OnChangeLanguage(i));
        }

        private UnityAction<bool> OnChangeLanguage(int index) => flag => {
            if (!flag) return;
            var changedLang = index == KOR_INDEX ? YisoLocale.Locale.KR : YisoLocale.Locale.EN;
        };
    }
}