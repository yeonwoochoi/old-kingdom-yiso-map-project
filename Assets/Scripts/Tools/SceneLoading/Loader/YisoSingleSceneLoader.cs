using System.Collections;
using Core.Behaviour;
using Sirenix.OdinInspector;
using Tools.Event;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils.Beagle;

namespace Tools.SceneLoading.Loader {
    [AddComponentMenu("Yiso/Tools/Scene/SingleSceneLoader")]
    public class YisoSingleSceneLoader : RunIBehaviour {
        public enum LoadingStatus {
            LoadStarted,
            BeforeEntryFade,
            EntryFade,
            AfterEntryFade,
            UnloadOriginScene,
            LoadDestinationScene,
            LoadProgressComplete,
            InterpolatedLoadProgressComplete,
            BeforeExitFade,
            ExitFade,
            DestinationSceneActivation,
            UnloadSceneLoader,
            LoadTransitionComplete
        }

        public struct LoadingSceneEvent {
            public LoadingStatus status;
            public string sceneName;

            public LoadingSceneEvent(string sceneName, LoadingStatus status) {
                this.status = status;
                this.sceneName = sceneName;
            }

            static LoadingSceneEvent e;

            public static void Trigger(string sceneName, LoadingStatus status) {
                e.sceneName = sceneName;
                e.status = status;
                YisoEventManager.TriggerEvent(e);
            }
        }

        [Title("Binding")] public static string loadingScreenSceneName = "Loading Scene";

        [Title("UI")] public CanvasGroup fader;
        public CanvasGroup loadingProgressBar;
        public CanvasGroup loadingAnimation;
        public CanvasGroup loadingCompleteAnimation;

        [Title("Time")] public float startFadeDuration = 0.5f; // the duration (in seconds) of the initial fade in
        public float progressBarSpeed = 2f; // the speed of the progress bar
        public float exitFadeDuration = 0.5f; // the duration (in seconds) of the load complete fade out
        public float loadingCompleteDelay = 0.5f; // Delay Before leaving scene when complete

        protected AsyncOperation asyncOperation;
        protected static string sceneToLoad = "";
        protected Image progressBarImage;
        protected float fillTarget = 0f;

        public static void LoadScene(string sceneToLoadName) {
            sceneToLoad = sceneToLoadName;
            Application.backgroundLoadingPriority =
                ThreadPriority
                    .High; // 가장 높은 우선 순위로 설정하며, 비동기 로딩 작업이 가능한 한 빠르게 수행됨. 이 설정은 로딩 시간을 최소화하지만, 메인 게임의 성능에 부정적인 영향을 미칠 수 있음
            if (loadingScreenSceneName != null) {
                LoadingSceneEvent.Trigger(sceneToLoad, LoadingStatus.LoadStarted);
                SceneManager.LoadScene(loadingScreenSceneName);
            }
        }

        public static void LoadScene(string sceneToLoadName, string loadingSceneName) {
            sceneToLoad = sceneToLoadName;
            Application.backgroundLoadingPriority = ThreadPriority.High;
            SceneManager.LoadScene(loadingSceneName);
        }

        protected override void Start() {
            progressBarImage = loadingProgressBar.GetComponent<Image>();
            if (!string.IsNullOrEmpty(sceneToLoad)) {
                StartCoroutine(LoadAsync());
            }
        }

        public override void OnUpdate() {
            Time.timeScale = 1f;
            progressBarImage.fillAmount = YisoMathUtils.Approach(progressBarImage.fillAmount,
                fillTarget, Time.deltaTime * progressBarSpeed);
        }

        protected virtual IEnumerator LoadAsync() {
            LoadingStart();

            // Fade Out
            yield return StartCoroutine(YisoFadeUtils.FadeCanvasGroup(fader, startFadeDuration, 0f));

            // Start Loading Scene
            asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
            asyncOperation.allowSceneActivation = false; // 로딩 작업 완료할 때까지 sceneToLoad를 비활성화

            // Loading 중..
            while (asyncOperation.progress < 0.9f) {
                fillTarget = asyncOperation.progress;
                yield return null;
            }

            // 90% 넘으면 100%로 Setting
            fillTarget = 1f;

            // progressBar가 fillTarget까지 채워질떄까지 기다림
            while (progressBarImage.fillAmount != fillTarget) {
                yield return null;
            }

            // Loading complete
            LoadingComplete();
            yield return new WaitForSeconds(loadingCompleteDelay);

            // Fade In
            yield return StartCoroutine(YisoFadeUtils.FadeCanvasGroup(fader, exitFadeDuration, 1f));

            // Switch to new scene
            asyncOperation.allowSceneActivation = true;
            LoadingSceneEvent.Trigger(sceneToLoad, LoadingStatus.LoadTransitionComplete);
        }

        protected virtual void LoadingStart() {
            loadingCompleteAnimation.alpha = 0;
            progressBarImage.fillAmount = 0f;
        }

        protected virtual void LoadingComplete() {
            LoadingSceneEvent.Trigger(sceneToLoad, LoadingStatus.InterpolatedLoadProgressComplete);
            loadingCompleteAnimation.gameObject.SetActive(true);
            StartCoroutine(YisoFadeUtils.FadeCanvasGroup(loadingProgressBar, 0.1f, 0f));
            StartCoroutine(YisoFadeUtils.FadeCanvasGroup(loadingAnimation, 0.1f, 0f));
            StartCoroutine(YisoFadeUtils.FadeCanvasGroup(loadingCompleteAnimation, 0.1f, 1f));
        }
    }
}