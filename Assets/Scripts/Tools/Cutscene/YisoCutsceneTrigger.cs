using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character.Core;
using Core.Behaviour;
using Core.Service;
using Core.Service.UI;
using Cutscene.Scripts.Control.Cutscene;
using Cutscene.Scripts.Control.Cutscene.Trigger;
using DG.Tweening;
using Manager;
using Sirenix.OdinInspector;
using Tools.Event;
using Tools.StateMachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Tools.Cutscene {
    [AddComponentMenu("Yiso/Tools/Cutscene/Cutscene Trigger")]
    public class YisoCutsceneTrigger : RunIBehaviour, IYisoEventListener<YisoCutsceneStateChangeEvent> {
        [Serializable]
        public struct CutsceneInfo {
            public CutscenePlayMode cutscenePlayType;

            [ShowIf("cutscenePlayType", CutscenePlayMode.Timeline)]
            public PlayableAsset timeline;

            [ShowIf("cutscenePlayType", CutscenePlayMode.AdditiveScene)]
            public string sceneName;

            [ShowIf("cutscenePlayType", CutscenePlayMode.Timeline)]
            public float initialTime;

            [ShowIf("cutscenePlayType", CutscenePlayMode.Timeline)]
            public DirectorWrapMode extrapolationMode;

            [ShowIf("cutscenePlayType", CutscenePlayMode.Timeline)]
            public Canvas canvas;
        }

        public enum CutscenePlayMode {
            Timeline,
            AdditiveScene
        }

        public enum CutsceneState {
            Idle,
            Play,
            Pause,
            Stop
        }

        [Title("Setting")] public CutsceneInfo cutsceneInfo;
        public bool autoPlay = true;
        [ShowIf("@!autoPlay")] public bool canRepeat = false;
        [ShowIf("autoPlay")] public float delay = 0f;

        [Title("Stage")] [Range(1, 100)] public int stage = 1;

        [Title("Condition")] public List<YisoCutsceneCondition> startConditions;

        [Title("Action")] public List<YisoCutsceneAction> startActions;
        public List<YisoCutsceneAction> pauseActions;
        public List<YisoCutsceneAction> stopActions;

        [Title("Player")] public bool hideCharacterDuringCutscene = true;

        [ShowIf("hideCharacterDuringCutscene")]
        public bool hidePetsDuringCutscene = true;

        public bool freezeCharacterDuringCutscene = true;

        [ShowIf("freezeCharacterDuringCutscene")]
        public float freezeDelay = 0f;

        [ShowIf("freezeCharacterDuringCutscene")]
        public float unfreezeDelay = 1f;

        [Title("Timer")]
        public bool pauseTimerDuringCutscene = true;

        public bool IsDone { get; protected set; } = false;

        protected bool initialized = false;
        protected bool isPlayed = false; // 해당 cutscene은 이미 플레이되었는지 여부 => 한번만 trigger 되게끔
        protected bool isTriggered = false; // 해당 cutscene은 이미 Trigger되었는지 여부 => Auto Play Delay 때문에
        protected PlayableDirector director;
        protected YisoStateMachine<CutsceneState> currentState;
        protected bool isHudVisible = true;
        protected bool isCanvasVisible = true;

        protected List<UnityAction> startCallbacks;
        protected List<UnityAction> pauseCallbacks;
        protected List<UnityAction> stopCallbacks;

        #region Initialization

        protected override void Awake() {
            Initialization();
        }

        protected override void OnEnable() {
            base.OnEnable();

            if (!initialized) {
                Initialization();
            }

            if (cutsceneInfo.cutscenePlayType == CutscenePlayMode.AdditiveScene) {
                this.YisoEventStartListening();
            }
            else {
                director.paused += ChangePauseState;
                director.stopped += ChangeStopState;
            }
        }

        protected override void OnDisable() {
            base.OnDisable();

            if (cutsceneInfo.cutscenePlayType == CutscenePlayMode.AdditiveScene) {
                this.YisoEventStopListening();
            }
            else {
                director.paused -= ChangePauseState;
                director.stopped -= ChangeStopState;
            }
        }

        protected virtual void Initialization() {
            if (initialized) return;
            isPlayed = false;
            isTriggered = false;
            IsDone = false;
            isHudVisible = true;

            if (cutsceneInfo.cutscenePlayType == CutscenePlayMode.Timeline) {
                InactivateCanvasUI();
                director = GetOrAddComponent<PlayableDirector>();
                director.playOnAwake = false;
                director.extrapolationMode = cutsceneInfo.extrapolationMode;
                director.initialTime = cutsceneInfo.initialTime;
                director.playableAsset = cutsceneInfo.timeline;
            }

            currentState = new YisoStateMachine<CutsceneState>(gameObject, true);
            currentState.ChangeState(CutsceneState.Idle);

            // Initialize Callback
            startCallbacks = new List<UnityAction>();
            pauseCallbacks = new List<UnityAction>();
            stopCallbacks = new List<UnityAction>();

            // Initialize Actions & Conditions
            if (startActions != null) {
                foreach (var startAction in startActions) {
                    startAction.Initialization();
                }
            }

            if (pauseActions != null) {
                foreach (var pauseAction in pauseActions) {
                    pauseAction.Initialization();
                }
            }

            if (stopActions != null) {
                foreach (var stopAction in stopActions) {
                    stopAction.Initialization();
                }
            }

            if (startConditions != null) {
                foreach (var startCondition in startConditions) {
                    startCondition.Initialization();
                }
            }

            initialized = true;
        }

        #endregion

        #region Update

        public override void OnUpdate() {
            if (!initialized) return;
            if (autoPlay) {
                if (isTriggered) return;
                if (!CanPlay()) return;
                isTriggered = true;
                StartCoroutine(AutoPlayCo());
            }
        }

        #endregion

        #region Core

        protected virtual IEnumerator AutoPlayCo() {
            if (!initialized) yield break;
            if (delay > 0f) {
                yield return new WaitForSeconds(delay);
            }

            Play();
        }

        public virtual void Play() {
            if (!initialized) return;
            if (!CanPlay()) return;

            isPlayed = true;

            if (cutsceneInfo.cutscenePlayType == CutscenePlayMode.Timeline) {
                var cutsceneTriggers = cutsceneInfo.canvas.GetComponentsInChildren<YisoBaseCutsceneTrigger>();
                var skipButtons = cutsceneInfo.canvas.GetComponentsInChildren<YisoCutsceneSkipButtonController>();
                foreach (var trigger in cutsceneTriggers) {
                    trigger.director = director;
                }

                foreach (var skipButton in skipButtons) {
                    skipButton.director = director;
                }
            }

            ChangePlayState();

            switch (cutsceneInfo.cutscenePlayType) {
                case CutscenePlayMode.Timeline:
                    director.Play();
                    break;
                case CutscenePlayMode.AdditiveScene:
                    StartCoroutine(LoadSceneAsync());
                    break;
            }
        }

        public virtual void Pause() {
            if (!initialized) return;
            if (currentState.CurrentState is not CutsceneState.Play or CutsceneState.Pause) return;
            director.Pause();
            ChangePauseState();
            // TODO: Pause Popup 연동
        }

        public virtual void Resume() {
            if (!initialized) return;
            if (currentState.CurrentState is not CutsceneState.Pause) return;
            director.Resume();
            ChangePlayState();
            // TODO: Resume Popup 연동
        }

        public virtual void Stop() {
            if (!initialized) return;
            if (currentState.CurrentState is not (CutsceneState.Play or CutsceneState.Pause)) return;
            director.Stop();
            ChangeStopState();
        }

        protected virtual IEnumerator LoadSceneAsync() {
            if (!GameManager.HasInstance) yield break;
            yield return GameManager.Instance.Fade(true, 0.1f).WaitForCompletion();
            
            yield return StartCoroutine(GameManager.Instance.LoadCutsceneAdditive(cutsceneInfo.sceneName));

            yield return GameManager.Instance.Fade(false, 0.1f).WaitForCompletion();
            
        }

        #endregion

        #region State

        protected virtual void ChangePlayState(PlayableDirector playableDirector = null) {
            OnPlayState();
            currentState.ChangeState(CutsceneState.Play);
        }

        protected virtual void ChangePauseState(PlayableDirector playableDirector = null) {
            currentState.ChangeState(CutsceneState.Pause);
            OnPauseState();
        }

        protected virtual void ChangeStopState(PlayableDirector playableDirector = null) {
            currentState.ChangeState(CutsceneState.Stop);
            OnStopState();
        }

        protected virtual void OnPlayState() {
            InactivateHUD();
            ActivateCanvasUI();
            PlayAllStartActions();
            InvokeStartCallback();
            SetCharacterBeforeCutscene();
            PauseTimer();
        }

        protected virtual void OnPauseState() {
            PlayAllPauseActions();
            InvokePauseCallback();
        }

        protected virtual void OnStopState() {
            ActivateHUD();
            InactivateCanvasUI();
            PlayAllStopActions();
            InvokeStopCallback();
            SetCharacterAfterCutscene();
            ResumeTimer();
            IsDone = true;
        }

        #endregion

        #region Action

        protected virtual void PlayAllStartActions() {
            if (startActions == null || startActions.Count == 0) return;
            startActions.Sort((x, y) => x.priority.CompareTo(y.priority));
            foreach (var action in startActions) {
                action.PerformAction();
            }
        }

        protected virtual void PlayAllPauseActions() {
            if (pauseActions == null || pauseActions.Count == 0) return;
            pauseActions.Sort((x, y) => x.priority.CompareTo(y.priority));
            foreach (var action in pauseActions) {
                action.PerformAction();
            }
        }

        protected virtual void PlayAllStopActions() {
            if (stopActions == null || stopActions.Count == 0) return;
            stopActions.Sort((x, y) => x.priority.CompareTo(y.priority));
            foreach (var action in stopActions) {
                action.PerformAction();
            }
        }

        #endregion

        #region Player State

        protected virtual void SetCharacterBeforeCutscene() {
            if (freezeCharacterDuringCutscene)
                GameManager.Instance.FreezeCharacter(freezeDelay, YisoCharacterStates.FreezePriority.CutscenePlayer);
            if (hideCharacterDuringCutscene) GameManager.Instance.HidePlayer(hidePetsDuringCutscene);
        }

        protected virtual void SetCharacterAfterCutscene() {
            if (hideCharacterDuringCutscene) GameManager.Instance.ShowPlayer(hidePetsDuringCutscene);
            GameManager.Instance.UnFreezeCharacter(freezeCharacterDuringCutscene ? unfreezeDelay : 0f,
                YisoCharacterStates.FreezePriority.CutscenePlayer);
        }

        #endregion

        #region HUD & UI

        protected virtual void ActivateHUD() {
            if (isHudVisible) return;
            YisoServiceProvider.Instance.Get<IYisoUIService>().ActiveOnlyHudUI(true);
            isHudVisible = true;
        }

        protected virtual void InactivateHUD() {
            if (!isHudVisible) return;
            YisoServiceProvider.Instance.Get<IYisoUIService>().ActiveOnlyHudUI(false);
            isHudVisible = false;
        }

        protected virtual void ActivateCanvasUI() {
            if (cutsceneInfo.cutscenePlayType != CutscenePlayMode.Timeline) return;
            if (isCanvasVisible) return;
            cutsceneInfo.canvas.gameObject.SetActive(true);
            isCanvasVisible = true;
        }

        protected virtual void InactivateCanvasUI() {
            if (cutsceneInfo.cutscenePlayType != CutscenePlayMode.Timeline) return;
            if (!isCanvasVisible) return;
            cutsceneInfo.canvas.gameObject.SetActive(false);
            isCanvasVisible = false;
        }

        #endregion

        #region Check

        public virtual bool CanPlay() {
            if (!initialized) return false;
            if (isPlayed && !canRepeat) return false;
            if (currentState.CurrentState is CutsceneState.Play or CutsceneState.Pause) return false;
            if (!CheckAllStartConditions()) return false;
            return true;
        }

        protected virtual bool CheckAllStartConditions() {
            if (!GameManager.HasInstance) return false;
            if (GameManager.Instance.CurrentStageId != stage) return false;
            if (GameManager.Instance.Player == null) return false;
            if (startConditions == null || startConditions.Count == 0) return true;
            startConditions.Sort((x, y) => x.priority.CompareTo(y.priority));
            var result = true;
            foreach (var condition in startConditions.Where(condition => !condition.CanPlay())) {
                result = false;
            }

            return result;
        }

        #endregion

        #region Event

        /// <summary>
        /// Additive Scene인 경우에만 해당
        /// </summary>
        /// <param name="stateChangeEventArgs"></param>
        public void OnEvent(YisoCutsceneStateChangeEvent stateChangeEventArgs) {
            if (stateChangeEventArgs.cutsceneName != cutsceneInfo.sceneName) return;
            switch (stateChangeEventArgs.cutsceneState) {
                case CutsceneState.Pause:
                    ChangePauseState();
                    break;
                case CutsceneState.Stop:
                    ChangeStopState();
                    break;
            }
        }

        #endregion

        #region Callback

        public virtual void AddStartCallback(UnityAction callback) {
            if (startCallbacks == null) {
                StartCoroutine(WaitForInitializationAndExecuteCo(() => startCallbacks?.Add(callback)));
                return;
            }

            startCallbacks.Add(callback);
        }

        public virtual void AddPauseCallback(UnityAction callback) {
            if (pauseCallbacks == null) {
                StartCoroutine(WaitForInitializationAndExecuteCo(() => pauseCallbacks?.Add(callback)));
                return;
            }

            pauseCallbacks.Add(callback);
        }

        public virtual void AddStopCallback(UnityAction callback) {
            if (stopCallbacks == null) {
                StartCoroutine(WaitForInitializationAndExecuteCo(() => stopCallbacks?.Add(callback)));
                return;
            }

            stopCallbacks.Add(callback);
        }

        protected virtual void InvokeStartCallback() {
            foreach (var startCallback in startCallbacks) {
                startCallback?.Invoke();
            }
        }

        protected virtual void InvokePauseCallback() {
            foreach (var pauseCallback in pauseCallbacks) {
                pauseCallback?.Invoke();
            }
        }

        protected virtual void InvokeStopCallback() {
            foreach (var stopCallback in stopCallbacks) {
                stopCallback?.Invoke();
            }
        }

        #endregion
        
        #region Timer

        protected virtual void PauseTimer() {
            if (!pauseTimerDuringCutscene) return;
            if (TimeManager.HasInstance && TimeManager.Instance.IsTimerRunning) {
                TimeManager.Instance.PauseTimer();
            }
        }

        protected virtual void ResumeTimer() {
            if (!pauseTimerDuringCutscene) return;
            if (TimeManager.HasInstance && TimeManager.Instance.IsTimerRunning) {
                TimeManager.Instance.ResumeTimer();
            }
        }

        #endregion

        #region Utils

        protected virtual IEnumerator WaitForInitializationAndExecuteCo(UnityAction action) {
            while (!initialized) {
                yield return null;
            }

            action?.Invoke();
        }

        #endregion
    }
}