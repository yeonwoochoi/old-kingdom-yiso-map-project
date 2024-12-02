using System;
using System.Collections;
using System.Collections.Generic;
using Core.Behaviour;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Utils.Beagle;

namespace Tools.SceneLoading.Loader {
    [Serializable]
    public class ProgressEvent : UnityEvent<float> {
    }

    [Serializable]
    public class YisoAdditiveSceneLoadingSettings {
        public enum UnloadMethods {
            None,
            ActiveScene,
            AllScenes
        }

        public string loadingSceneName = "Additive Loading Scene";
        public ThreadPriority threadPriority = ThreadPriority.High;
        public bool secureLoad = true; // 등록된 Scene인지 체크함

        public float beforeEntryFadeDelay = 0f;
        public float entryFadeDuration = 0.5f;
        public float afterEntryFadeDelay = 0.1f;

        public float beforeExitFadeDelay = 0.5f;
        public float exitFadeDuration = 0.2f;

        public bool interpolateProgress = true; // Progress Bar interpolate
        public float progressBarSpeed = 0f;

        public YisoAdditiveSceneLoader.FadeModes fadeMode = YisoAdditiveSceneLoader.FadeModes.FadeInThenOut;
        public UnloadMethods unloadMethod = UnloadMethods.AllScenes;

        /// <summary>
        /// Additive load할때 로드할 씬이 유출되는 것을 방지하는 Scene을 Anti spill이라고 함
        /// 따로 등록 안하면 자동으로 생성하지만 커스텀해서 지정할 수도 있음
        /// </summary>
        public string antiSpillSceneName = "";
    }

    [AddComponentMenu("Yiso/Tools/Scene/AdditiveSceneLoader")]
    public class YisoAdditiveSceneLoader : RunIBehaviour {
        public enum FadeModes {
            FadeInThenOut,
            FadeOutThenIn
        }

        [Title("UI")] public CanvasGroup fader;

        [Title("Audio Listener")] public AudioListener loadingAudioListener;

        [Title("Settings")] public bool debugMode = false;

        [Title("Progress Events")]
        public ProgressEvent updateProgressEvent; // Progress가 Update될때 (with interpolation) 실행됨

        [Header("State Events")] public UnityEvent onLoadStarted;
        public UnityEvent onBeforeEntryFade;
        public UnityEvent onEntryFade;
        public UnityEvent onAfterEntryFade;
        public UnityEvent onUnloadOriginScene;
        public UnityEvent onLoadDestinationScene;
        public UnityEvent onLoadProgressComplete;
        public UnityEvent onInterpolatedLoadProgressComplete;
        public UnityEvent onBeforeExitFade;
        public UnityEvent onExitFade;
        public UnityEvent onDestinationSceneActivation;
        public UnityEvent onUnloadSceneLoader;

        protected static string loadingSceneName;
        protected static string sceneToLoadName;

        protected static Scene[]
            initialScenes; // Unload되어야 할 Scene들 (initial Scenes -> Loading Scene -> Destination Scene)

        protected static List<string> scenesInBuild;
        protected static float beforeEntryFadeDelay;
        protected static float entryFadeDuration;
        protected static float afterEntryFadeDelay;
        protected static float beforeExitFadeDelay;
        protected static float exitFadeDuration;
        protected static bool interpolateProgress;
        protected static float progressBarSpeed;
        protected static FadeModes fadeMode;
        protected static string antiSpillSceneName;
        protected static bool loadingInProgress = false;

        protected YisoSceneLoadingAntiSpill antiSpill = new();
        protected AsyncOperation unloadOriginAsyncOperation;
        protected AsyncOperation loadDestinationAsyncOperation;
        protected AsyncOperation unloadLoadingAsyncOperation;
        protected float loadProgress = 0f;
        protected float interpolatedLoadProgress = 0f;

        protected const float AsyncProgressLimit = 0.9f;

        #region Static

        public static void LoadScene(string sceneToLoadNameParam, YisoAdditiveSceneLoadingSettings settings) {
            LoadScene(sceneToLoadName, settings.loadingSceneName, settings.threadPriority, settings.secureLoad,
                settings.interpolateProgress,
                settings.beforeEntryFadeDelay, settings.entryFadeDuration, settings.afterEntryFadeDelay,
                settings.beforeExitFadeDelay,
                settings.exitFadeDuration, settings.progressBarSpeed, settings.fadeMode, settings.unloadMethod,
                settings.antiSpillSceneName);
        }

        public static void LoadScene(string sceneToLoadNameParam,
            string loadingSceneNameParam = "Additive Loading Scene",
            ThreadPriority threadPriorityParam = ThreadPriority.High, bool secureLoadParam = true,
            bool interpolateProgressParam = true,
            float beforeEntryFadeDelayParam = 0f,
            float entryFadeDurationParam = 0.25f,
            float afterEntryFadeDelayParam = 0.1f,
            float beforeExitFadeDelayParam = 0.25f,
            float exitFadeDurationParam = 0.2f,
            float progressBarSpeedParam = 5f,
            FadeModes fadeModeParam = FadeModes.FadeInThenOut,
            YisoAdditiveSceneLoadingSettings.UnloadMethods unloadMethodParam =
                YisoAdditiveSceneLoadingSettings.UnloadMethods.AllScenes,
            string antiSpillSceneNameParam = "") {
            if (loadingInProgress) {
                Debug.LogError(
                    "YisoLoadingSceneManagerAdditive : a request to load a new scene was emitted while a scene load was already in progress");
                return;
            }

            if (secureLoadParam) {
                scenesInBuild = YisoSceneUtils.GetScenesInBuild();
                if (!scenesInBuild.Contains(sceneToLoadNameParam)) {
                    Debug.LogError(
                        $"YisoLoadingSceneManagerAdditive : impossible to load the '{sceneToLoadNameParam}' scene, there is no such scene in the project's build settings.");
                    return;
                }

                if (!scenesInBuild.Contains(loadingSceneNameParam)) {
                    Debug.LogError(
                        $"YisoLoadingSceneManagerAdditive : impossible to load the '{loadingSceneNameParam}' scene, there is no such scene in the project's build settings.");
                    return;
                }
            }

            loadingInProgress = true;
            initialScenes = GetScenesToUnload(unloadMethodParam);

            Application.backgroundLoadingPriority = threadPriorityParam;
            sceneToLoadName = sceneToLoadNameParam;
            loadingSceneName = loadingSceneNameParam;
            beforeEntryFadeDelay = beforeEntryFadeDelayParam;
            entryFadeDuration = entryFadeDurationParam;
            afterEntryFadeDelay = afterEntryFadeDelayParam;
            beforeExitFadeDelay = beforeExitFadeDelayParam;
            exitFadeDuration = exitFadeDurationParam;
            fadeMode = fadeModeParam;
            interpolateProgress = interpolateProgressParam;
            progressBarSpeed = progressBarSpeedParam;
            antiSpillSceneName = antiSpillSceneNameParam;

            SceneManager.LoadScene(loadingSceneName, LoadSceneMode.Additive);
        }


        private static Scene[] GetScenesToUnload(YisoAdditiveSceneLoadingSettings.UnloadMethods unloaded) {
            switch (unloaded) {
                case YisoAdditiveSceneLoadingSettings.UnloadMethods.None:
                    initialScenes = Array.Empty<Scene>();
                    break;
                case YisoAdditiveSceneLoadingSettings.UnloadMethods.ActiveScene:
                    initialScenes = new Scene[1] {SceneManager.GetActiveScene()};
                    break;
                case YisoAdditiveSceneLoadingSettings.UnloadMethods.AllScenes:
                    initialScenes = YisoSceneUtils.GetLoadedScenes();
                    break;
            }

            return initialScenes;
        }

        #endregion

        #region Initialization

        protected override void Awake() {
            Initialization();
        }

        protected virtual void Initialization() {
            LoadingSceneDebug("YisoAdditiveSceneLoader : Initialization");
            if (debugMode) {
                foreach (var scene in initialScenes) {
                    LoadingSceneDebug($"YisoAdditiveSceneLoader : Initial scene : {scene.name}");
                }
            }

            Time.timeScale = 1f;
            if (sceneToLoadName == "" || loadingSceneName == "") return;
            StartCoroutine(LoadSequence());
        }

        protected override void OnDestroy() {
            loadingInProgress = false;
        }

        #endregion

        #region Update

        public override void OnUpdate() {
            UpdateProgress();
        }

        /// <summary>
        /// Sends progress value via UnityEvents
        /// </summary>
        protected virtual void UpdateProgress() {
            if (interpolateProgress) {
                interpolatedLoadProgress = YisoMathUtils.Approach(interpolatedLoadProgress, loadProgress,
                    Time.unscaledDeltaTime * progressBarSpeed);
                updateProgressEvent?.Invoke(interpolatedLoadProgress);
            }
            else {
                updateProgressEvent?.Invoke(loadProgress);
            }
        }

        #endregion

        #region Sequence

        /// <summary>
        /// Loads the scene to load asynchronously.
        /// </summary>
        protected virtual IEnumerator LoadSequence() {
            antiSpill?.PrepareAntiFill(sceneToLoadName, antiSpillSceneName);
            InitiateLoad();
            yield return ProcessDelayBeforeEntryFade();
            yield return EntryFade();
            yield return ProcessDelayAfterEntryFade();
            yield return UnloadOriginScenes();
            yield return LoadDestinationScene();
            yield return ProcessDelayBeforeExitFade();
            yield return DestinationSceneActivation();
            yield return ExitFade();
            yield return UnloadSceneLoader();
        }

        protected virtual void InitiateLoad() {
            loadProgress = 0f;
            interpolatedLoadProgress = 0f;
            Time.timeScale = 1f;
            SetAudioListener(false);
            LoadingSceneDebug("YisoAdditiveSceneLoader : Initiate Load");
            YisoSingleSceneLoader.LoadingSceneEvent.Trigger(sceneToLoadName,
                YisoSingleSceneLoader.LoadingStatus.LoadStarted);
            onLoadStarted?.Invoke();
        }

        protected virtual IEnumerator ProcessDelayBeforeEntryFade() {
            if (beforeEntryFadeDelay > 0f) {
                LoadingSceneDebug(
                    $"YisoAdditiveSceneLoader : delay before entry fade, duration : {beforeEntryFadeDelay}");
                YisoSingleSceneLoader.LoadingSceneEvent.Trigger(sceneToLoadName,
                    YisoSingleSceneLoader.LoadingStatus.BeforeEntryFade);
                onBeforeEntryFade?.Invoke();

                yield return YisoCoroutineUtils.WaitForUnscaled(beforeEntryFadeDelay);
            }
        }

        protected virtual IEnumerator EntryFade() {
            if (entryFadeDuration > 0f) {
                LoadingSceneDebug($"YisoAdditiveSceneLoader : entry fade, duration : {entryFadeDuration}");
                YisoSingleSceneLoader.LoadingSceneEvent.Trigger(sceneToLoadName,
                    YisoSingleSceneLoader.LoadingStatus.EntryFade);
                onEntryFade?.Invoke();

                if (fadeMode == FadeModes.FadeOutThenIn) {
                    yield return null;
                    yield return
                        StartCoroutine(YisoFadeUtils.FadeCanvasGroup(fader, entryFadeDuration, 0f)); // Fade out
                }
                else {
                    yield return null;
                    yield return StartCoroutine(YisoFadeUtils.FadeCanvasGroup(fader, entryFadeDuration, 1f)); // Fade in
                }

                yield return YisoCoroutineUtils.WaitForUnscaled(entryFadeDuration);
            }
        }

        protected virtual IEnumerator ProcessDelayAfterEntryFade() {
            if (afterEntryFadeDelay > 0f) {
                LoadingSceneDebug(
                    $"YisoAdditiveSceneLoader : delay after entry fade, duration : {afterEntryFadeDelay}");
                YisoSingleSceneLoader.LoadingSceneEvent.Trigger(sceneToLoadName,
                    YisoSingleSceneLoader.LoadingStatus.AfterEntryFade);
                onAfterEntryFade?.Invoke();

                yield return YisoCoroutineUtils.WaitForUnscaled(afterEntryFadeDelay);
            }
        }

        protected virtual IEnumerator UnloadOriginScenes() {
            foreach (var scene in initialScenes) {
                LoadingSceneDebug($"YisoAdditiveSceneLoader : unload scene : {scene.name}");
                YisoSingleSceneLoader.LoadingSceneEvent.Trigger(sceneToLoadName,
                    YisoSingleSceneLoader.LoadingStatus.UnloadOriginScene);
                onUnloadOriginScene?.Invoke();

                if (!scene.IsValid() || !scene.isLoaded) {
                    Debug.LogWarning($"YisoAdditiveSceneLoader : invalid scene : {scene.name}");
                    continue;
                }

                unloadOriginAsyncOperation = SceneManager.UnloadSceneAsync(scene);
                SetAudioListener(true);
                while (unloadOriginAsyncOperation.progress < AsyncProgressLimit) {
                    yield return null;
                }
            }
        }

        protected virtual IEnumerator LoadDestinationScene() {
            // Destination Scene Loading 시작
            LoadingSceneDebug("YisoAdditiveSceneLoader : load destination scene");
            YisoSingleSceneLoader.LoadingSceneEvent.Trigger(sceneToLoadName,
                YisoSingleSceneLoader.LoadingStatus.LoadDestinationScene);
            onLoadDestinationScene?.Invoke();

            loadDestinationAsyncOperation = SceneManager.LoadSceneAsync(sceneToLoadName);
            loadDestinationAsyncOperation.completed += OnLoadOperationComplete;

            loadDestinationAsyncOperation.allowSceneActivation = false;

            while (loadDestinationAsyncOperation.progress < AsyncProgressLimit) {
                loadProgress = loadDestinationAsyncOperation.progress;
                yield return null;
            }

            // 실제 Loading은 끝
            LoadingSceneDebug("YisoAdditiveSceneLoader : load progress complete");
            YisoSingleSceneLoader.LoadingSceneEvent.Trigger(sceneToLoadName,
                YisoSingleSceneLoader.LoadingStatus.LoadProgressComplete);
            onLoadProgressComplete?.Invoke();

            loadProgress = 1f;

            if (interpolateProgress) {
                while (interpolatedLoadProgress < 1f) {
                    yield return null;
                }
            }

            // Interpolate loading 끝 (보간하는 거니까 실제 로딩보다 살짝 늦지)
            LoadingSceneDebug("YisoAdditiveSceneLoader : interpolated load complete");
            YisoSingleSceneLoader.LoadingSceneEvent.Trigger(sceneToLoadName,
                YisoSingleSceneLoader.LoadingStatus.InterpolatedLoadProgressComplete);
            onInterpolatedLoadProgressComplete?.Invoke();
        }

        protected virtual IEnumerator ProcessDelayBeforeExitFade() {
            if (beforeExitFadeDelay > 0f) {
                LoadingSceneDebug(
                    $"YisoAdditiveSceneLoader : delay before exit fade, duration : {beforeExitFadeDelay}");
                YisoSingleSceneLoader.LoadingSceneEvent.Trigger(sceneToLoadName,
                    YisoSingleSceneLoader.LoadingStatus.BeforeExitFade);
                onBeforeExitFade?.Invoke();

                yield return YisoCoroutineUtils.WaitForUnscaled(beforeExitFadeDelay);
            }
        }

        protected virtual IEnumerator ExitFade() {
            SetAudioListener(false);
            if (exitFadeDuration > 0f) {
                LoadingSceneDebug($"YisoAdditiveSceneLoader : exit fade, duration : {exitFadeDuration}");
                YisoSingleSceneLoader.LoadingSceneEvent.Trigger(sceneToLoadName,
                    YisoSingleSceneLoader.LoadingStatus.ExitFade);
                onExitFade?.Invoke();

                if (fadeMode == FadeModes.FadeOutThenIn) {
                    yield return StartCoroutine(YisoFadeUtils.FadeCanvasGroup(fader, entryFadeDuration, 1f)); // Fade in
                }
                else {
                    yield return
                        StartCoroutine(YisoFadeUtils.FadeCanvasGroup(fader, entryFadeDuration, 0f)); // Fade out
                }

                yield return YisoCoroutineUtils.WaitForUnscaled(exitFadeDuration);
            }
        }

        protected virtual IEnumerator DestinationSceneActivation() {
            yield return YisoCoroutineUtils.WaitForFrames(1);
            loadDestinationAsyncOperation.allowSceneActivation = true;
            while (loadDestinationAsyncOperation.progress < 1.0f) {
                yield return null;
            }

            LoadingSceneDebug($"YisoAdditiveSceneLoader : activating destination scene");
            YisoSingleSceneLoader.LoadingSceneEvent.Trigger(sceneToLoadName,
                YisoSingleSceneLoader.LoadingStatus.DestinationSceneActivation);
            onDestinationSceneActivation?.Invoke();
        }

        protected virtual IEnumerator UnloadSceneLoader() {
            LoadingSceneDebug($"YisoAdditiveSceneLoader : unloading scene loader");
            YisoSingleSceneLoader.LoadingSceneEvent.Trigger(sceneToLoadName,
                YisoSingleSceneLoader.LoadingStatus.UnloadSceneLoader);
            onUnloadSceneLoader?.Invoke();

            yield return null;
            unloadLoadingAsyncOperation = SceneManager.UnloadSceneAsync(loadingSceneName);
            while (unloadLoadingAsyncOperation.progress < AsyncProgressLimit) {
                yield return null;
            }
        }

        /// <summary>
        /// Destination Scene Loading이 완료되었을때 호출되는 Callback
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnLoadOperationComplete(AsyncOperation obj) {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoadName));
            LoadingSceneDebug($"YisoAdditiveSceneLoader : set active scene to {sceneToLoadName}");
        }

        #endregion

        #region Audio

        protected virtual void SetAudioListener(bool state) {
            if (loadingAudioListener != null) {
                loadingAudioListener.gameObject.SetActive(state);
            }
        }

        #endregion

        #region Debug

        protected virtual void LoadingSceneDebug(string message) {
            if (!debugMode) {
                return;
            }

            string output = "";
            output += "<color=#82d3f9>[" + Time.frameCount + "]</color> ";
            output += "<color=#f9a682>[" + YisoDebugUtils.FloatToTimeString(Time.time, false, true, true, true) +
                      "]</color> ";
            output += message;
            Debug.Log(output);
        }

        #endregion
    }
}