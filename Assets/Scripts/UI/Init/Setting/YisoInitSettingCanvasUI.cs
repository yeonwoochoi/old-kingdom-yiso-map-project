using Core.Behaviour;
using UnityEngine;
using Utils.Extensions;

namespace UI.Init.Setting {
    public class YisoInitSettingCanvasUI : RunIBehaviour {
        [SerializeField] private YisoInitSoundSettingsPanelUI soundSettings;
        [SerializeField] private YisoInitLanguageSettingsPanelUI languageSettings;
 
        private CanvasGroup canvasGroup;

        public bool Visible {
            get => canvasGroup.IsVisible();
            set => canvasGroup.Visible(value);
        }

        protected override void Start() {
            base.Start();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnVisibleCanvas(bool flag) {
            Visible = flag;
        }
    }
}