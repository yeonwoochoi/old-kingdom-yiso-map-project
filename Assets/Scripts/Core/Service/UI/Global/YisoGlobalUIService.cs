using System;
using System.Collections;
using Core.Domain.Types;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UI.Global;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Service.UI.Global {
    public class YisoGlobalUIService : IYisoGlobalUIService {
        private YisoGlobalCanvasUI globalUI;
        
        public void SetGlobalUI(YisoGlobalCanvasUI globalUI) {
            this.globalUI = globalUI;
        }

        public void StartCoroutine(IEnumerator coroutine) {
            globalUI.StartCoroutine(coroutine);
        }

        public TweenerCore<float, float, FloatOptions> DOFade(bool flag, float duration) {
            return globalUI.DOFade(flag, duration);
        }

        public void StartGameLoading(YisoSceneTypes targetScene) {
            globalUI.StartGameLoading(targetScene);
        }

        public void StopGameLoading() {
            globalUI.StopGameLoading();
        }

        public void ShowSaving(UnityAction onShowed = null) {
            globalUI.ShowSaving(onShowed);
        }

        public void StartInitLoading() {
            globalUI.StartIniLoading();
        }

        public void StopInitLoading() {
            globalUI.StopInitLoading();
        }

        public void FloatingText(string text, Color color) {
            globalUI.FloatingText(text, color);
        }
        
        public bool IsReady() => globalUI != null;
        public void OnDestroy() { }
        private YisoGlobalUIService() { }
        
        internal static YisoGlobalUIService CreateService() => new();
    }
}