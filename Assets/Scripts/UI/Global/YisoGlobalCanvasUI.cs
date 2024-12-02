using System;
using Core.Behaviour;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Data;
using Core.Service.UI;
using Core.Service.UI.Global;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UI.Global.Fade;
using UI.Global.FloatingText;
using UI.Global.Loading;
using UI.Global.Saving;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Global {
    public class YisoGlobalCanvasUI : RunISingleton<YisoGlobalCanvasUI> {
        [SerializeField] private YisoGlobalLoadingUI loadingUI;
        [SerializeField] private YisoGlobalLoadingUI gameLoadingUI;
        [SerializeField] private YisoGlobalFadeUI fadeUId;
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private YisoGlobalSavingContentUI savingUI;
        [SerializeField] private YisoGlobalFloatingTextUI floatingTextUI;
        
        public bool IsUIActive { get; private set; }

        private void Start() {
            YisoServiceProvider.Instance.Get<IYisoGlobalUIService>().SetGlobalUI(this);
            YisoServiceProvider.Instance.Get<IYisoUIService>().SetGlobalCanvasUI(this);
            var data = YisoServiceProvider.Instance.Get<IYisoDataService>().GetGameData();
            versionText.SetText($"version {data.versionData}");
        }
        
        public TweenerCore<float, float, FloatOptions> DOFade(bool flag, float duration) {
            return fadeUId.DOFade(flag, duration);
        }
        
        public void StartGameLoading(YisoSceneTypes type) {
            if (type == YisoSceneTypes.STORY) {
                gameLoadingUI.StartStageLoading();
            } else {
                gameLoadingUI.StartLoading();
            }
            
            IsUIActive = true;
        }

        public void StopGameLoading() {
            gameLoadingUI.StopLoading();
            IsUIActive = false;
        }

        public void StartIniLoading() {
            loadingUI.StartLoading();
            IsUIActive = true;
        }

        public void StopInitLoading() {
            loadingUI.StopLoading();
            IsUIActive = false;
        }

        public void FloatingText(string text, Color color) {
            floatingTextUI.AddFloating(text, color);
        }

        public void ShowSaving(UnityAction onShowed = null) {
            savingUI.Show(onShowed);
        }
    }
}