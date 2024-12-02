using System;
using System.Collections;
using System.Collections.Generic;
using Core.Domain.Item;
using Core.Domain.Types;
using Core.Service.Game;
using Core.Service.UI.Global;
using DG.Tweening;
using MEC;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Core.Service.Scene {
    public class YisoSceneService : IYisoSceneService {
        private readonly IYisoGlobalUIService globalUIService;

        private event UnityAction<YisoSceneTypes, YisoSceneTypes> OnSceneChangedEvent; 

        private bool uiAttached = false;
        private bool initLoading = false;

        private YisoSceneTypes currentScene = YisoSceneTypes.INIT;

        public YisoSceneTypes GetCurrentScene() {
            return currentScene;
        }

        public void SetCurrentScene(YisoSceneTypes type) {
            currentScene = type;
            OnSceneChangedEvent?.Invoke(YisoSceneTypes.NONE, type);
        }

        public void RegisterOnSceneChanged(UnityAction<YisoSceneTypes, YisoSceneTypes> handler) {
            OnSceneChangedEvent += handler;
        }

        public void UnregisterOnSceneChanged(UnityAction<YisoSceneTypes, YisoSceneTypes> handler) {
            OnSceneChangedEvent -= handler;
        }

        // ONLY CALL ONCE!
        public void LoadUIScene() {
            if (uiAttached) return;
            Timing.RunCoroutine(DOLoadUIScene());
        }

        private IEnumerator<float> DOLoadUIScene() {
            var async = SceneManager.LoadSceneAsync(YisoSceneTypes.UI.ToBuildName(), LoadSceneMode.Additive);
            while (!async.isDone) yield return Timing.WaitForOneFrame;
            uiAttached = true;
        }

        public void LoadScene(YisoSceneTypes type, Action<bool> onFade = null) {
            if (currentScene == type) {
                Debug.LogError($"Current Type is {type.ToString()} Cannot Load Scene!");
                return;
            }
            globalUIService.StartCoroutine(DOLoadScene(type, onFade));
        }

        private IEnumerator DOLoadScene(YisoSceneTypes type, Action<bool> onFade = null) {
            yield return globalUIService.DOFade(true, 0.5f).WaitForCompletion();
            VisibleLoading(true, type);
            onFade?.Invoke(true);
            
            yield return globalUIService.DOFade(false, 0.5f).WaitForCompletion();
            onFade?.Invoke(false);

            var beforeActiveScene = SceneManager.GetActiveScene();
            var beforeSceneType = currentScene;
            var unloadAsyncOperation = SceneManager.UnloadSceneAsync(beforeActiveScene);
            while (!unloadAsyncOperation!.isDone) yield return null;
            
            var sceneName = type.ToBuildName();
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncOperation!.isDone) yield return null;

            SceneManager.SetActiveScene(type.ToScene());


            currentScene = type;
            OnSceneChangedEvent?.Invoke(beforeSceneType, currentScene);
            yield return new WaitForSeconds(3f);

            yield return globalUIService.DOFade(true, 0.5f).WaitForCompletion();
            VisibleLoading(false, type);
            yield return globalUIService.DOFade(false, 0.5f).WaitForCompletion();

            if (!initLoading) initLoading = true;
        }

        private void VisibleLoading(bool flag, YisoSceneTypes targetScene) {
            var isMobile = YisoServiceProvider.Instance.Get<IYisoGameService>().IsMobile();
            if (!isMobile && flag) {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
            if (initLoading) {
                if (flag) globalUIService.StartGameLoading(targetScene);
                else globalUIService.StopGameLoading();
            } else {
                if (flag) globalUIService.StartInitLoading();
                else globalUIService.StopInitLoading();
            }
        }
        
        public bool IsReady() => true;
        public void OnDestroy() { }
        private YisoSceneService(YisoServiceProvider provider) {
            globalUIService = provider.Get<IYisoGlobalUIService>();
        }
        
        internal static YisoSceneService CreateService(YisoServiceProvider provider) => new(provider);
    }
}