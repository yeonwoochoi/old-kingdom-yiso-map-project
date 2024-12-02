using System;
using System.Collections;
using Core.Domain.Types;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UI.Global;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Service.UI.Global {
    public interface IYisoGlobalUIService : IYisoService {
        public void SetGlobalUI(YisoGlobalCanvasUI globalUI);
        public void StartCoroutine(IEnumerator coroutine);
        public TweenerCore<float, float, FloatOptions> DOFade(bool flag, float duration);
        public void StartInitLoading();
        public void StopInitLoading();
        public void StartGameLoading(YisoSceneTypes targetScene);
        public void StopGameLoading();
        public void ShowSaving(UnityAction onShowed = null);
        public void FloatingText(string text, Color color);
    }
}