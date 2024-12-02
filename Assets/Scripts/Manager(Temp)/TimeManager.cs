using System.Collections.Generic;
using Core.Behaviour;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Core.Service.UI.HUD;
using Sirenix.OdinInspector;
using Tools.Event;
using Tools.StateMachine;
using UI.HUD.Timer;
using UnityEngine;
using Utils.Beagle;

namespace Manager_Temp_ {
    /// <summary>
    /// Change: 지정된 시간동안 시간 척도를 변경
    /// Reset: 시간 척도를 초기 값을 재설정
    /// Revert: 시간 척도 변경을 중지하고 이전에 설정된 시간 척도로 되돌림
    /// </summary>
    public enum TimeScaleMethods {
        Change,
        Reset,
        Revert
    }

    /// <summary>
    /// Speed: 보간 속도에 따라 시간 척도 변경
    /// Duration: 보간 지속 시간에 따라 시간 척도 변경
    /// NoInterpolation: 보간 없이 즉시 시간 척도 변경
    /// </summary>
    public enum TimeScaleLerpModes {
        Speed,
        Duration,
        NoInterpolation
    }

    /// <summary>
    /// timeScale: 변경할 시간 척도 (ex. 0.5 = 절반 속도, 2 = 2배속)
    /// duration: 시간 척도 변경 유지 시간 (초) (ex. 1이면 시간 척도가 변경된 후 1초간 유지됨)
    /// timeScaleLerp: 시간 척도 변경을 보간할건지 여부
    /// lerpSpeed: 보간 속도
    /// infinite: 시간 척도 변경 무한히 유지 여부
    /// timeScaleLerpMode: Speed, Duration, NoInterpolation 중 선택
    /// timeScaleLerpCurve: 시간 척도 변경에 사용될 보간 Curve 설정 (timeScaleLerpMode == Duration 일떄만)
    /// timeScaleLerpDuration: 시간 척도 변경이 보간될 지속 시간 지정 (ex. 1이면 1초동안 서서히 보간을 통해 시간 척도가 변경됨)
    /// timeScaleLerpOnReset: 시간 척도 초기값으로 재설정할때 보간 적용할지 여부
    /// timeScaleLerpCurveOnReset : 시간 척도를 초기 값으로 재설정할 때 사용할 보간 곡선 (timeScaleLerpOnReset = true)
    /// TimeScaleLerpDurationOnReset: 시간 척도를 초기 값으로 재설정할 때 보간될 지속 시간 (timeScaleLerpOnReset = true)
    /// </summary>
    public struct TimeScaleProperties {
        public float timeScale;
        public float duration;
        public bool timeScaleLerp;
        public float lerpSpeed;
        public bool infinite;
        public TimeScaleLerpModes timeScaleLerpMode;
        public AnimationCurve timeScaleLerpCurve;
        public float timeScaleLerpDuration;
        public bool timeScaleLerpOnReset;
        public AnimationCurve timeScaleLerpCurveOnReset;
        public float timeScaleLerpDurationOnReset;

        public override string ToString() =>
            $"REQUESTED ts={timeScale} time={duration} lerp={timeScaleLerp} speed={lerpSpeed} keep={infinite}";
    }

    public struct YisoTimeScaleEvent {
        private static event Delegate OnEvent;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialization() {
            OnEvent = null;
        }

        public delegate void Delegate(TimeScaleMethods timeScaleMethod, float timeScale, float duration, bool lerp,
            float lerpSpeed, bool infinite, TimeScaleLerpModes timeScaleLerpMode = TimeScaleLerpModes.Speed,
            AnimationCurve timeScaleLerpCurve = null,
            float timeScaleLerpDuration = 0.2f, bool timeScaleLerpOnReset = false,
            AnimationCurve timeScaleLerpCurveOnReset = null, float timeScaleLerpDurationOnReset = 0.2f);

        public static void Register(Delegate callback) {
            OnEvent += callback;
        }

        public static void Unregister(Delegate callback) {
            OnEvent -= callback;
        }

        public static void Trigger(TimeScaleMethods timeScaleMethod, float timeScale, float duration, bool lerp,
            float lerpSpeed, bool infinite, TimeScaleLerpModes timeScaleLerpMode = TimeScaleLerpModes.Speed,
            AnimationCurve timeScaleLerpCurve = null,
            float timeScaleLerpDuration = 0.2f, bool timeScaleLerpOnReset = false,
            AnimationCurve timeScaleLerpCurveOnReset = null, float timeScaleLerpDurationOnReset = 0.2f) {
            OnEvent?.Invoke(timeScaleMethod, timeScale, duration, lerp, lerpSpeed, infinite, timeScaleLerpMode,
                timeScaleLerpCurve, timeScaleLerpDuration, timeScaleLerpOnReset, timeScaleLerpCurveOnReset,
                timeScaleLerpDurationOnReset);
        }
    }

    public struct YisoFreezeFrameEvent {
        private static event Delegate OnEvent;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialization() {
            OnEvent = null;
        }

        public static void Register(Delegate callback) {
            OnEvent += callback;
        }

        public static void Unregister(Delegate callback) {
            OnEvent -= callback;
        }

        public delegate void Delegate(float duration);

        static public void Trigger(float duration) {
            OnEvent?.Invoke(duration);
        }
    }

    public class TimeManager : RunISingleton<TimeManager>, IYisoEventListener<YisoInGameEvent> {
        public enum TimerState {
            Stopped,
            Running,
            Paused
        }
        
        [Title("Debug")] [ReadOnly] public float currentTimeScale = 1f;
        [ReadOnly] public float targetTimeScale = 1f;

        protected readonly float normalTimeScale = 1f;

        protected Stack<TimeScaleProperties> timeScaleProperties;
        protected TimeScaleProperties currentProperty;
        protected TimeScaleProperties resetProperty;
        protected float initialFixedDeltaTime = 0f;
        protected float initialMaximumDeltaTime = 0f;
        protected bool lerpingBackToNormal = false; // normal Time Scale로 돌아가고 있는 중인지 여부
        protected float timeScaleLastTime = float.NegativeInfinity; // 같은 값이면 Update하지 않으려고 체크하기 위한 변수
        protected float startedAt;

        public YisoStateMachine<TimerState> timerState;
        protected float timerDuration = 0f;
        protected float timerElapsedTime = 0f;
        
        protected IYisoHUDUIService HUDUIService => YisoServiceProvider.Instance.Get<IYisoHUDUIService>();
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<TimeManager>();
        public float TimerProgressRate => timerDuration == 0f ? 0f : Mathf.Min(timerElapsedTime / timerDuration, 1f);
        public bool IsTimerRunning { get; protected set; } = false;

        protected override void Awake() {
            base.Awake();
            timeScaleProperties = new Stack<TimeScaleProperties>();
        }

        public virtual void Start() {
            Initialization();
        }

        protected virtual void Initialization() {
            targetTimeScale = normalTimeScale;
            initialFixedDeltaTime = Time.fixedDeltaTime;
            initialMaximumDeltaTime = Time.maximumDeltaTime;
            timerState = new YisoStateMachine<TimerState>(gameObject,  true);
        }

        public override void OnUpdate() {
            while (timeScaleProperties.Count > 0) {
                currentProperty = timeScaleProperties.Peek();
                targetTimeScale = currentProperty.timeScale;
                currentProperty.duration -= Time.unscaledDeltaTime;

                // Peek 요소를 사용해 duration값을 변경했지만, 스택의 요소와 참조관계에 있으므로
                // 실제 스택 element를 변경하려면 Pop-Push 과정을 거쳐야 함
                timeScaleProperties.Pop();
                timeScaleProperties.Push(currentProperty);

                if (currentProperty.duration > 0f || currentProperty.infinite) {
                    break; // Infinite거나 아직 duration이 남았으면 현재 상태 유지해야하니까
                }
                else {
                    Revert(); // current property pop! (다음꺼 가야지)
                }
            }

            // 끝났으면 원래 상태로
            if (timeScaleProperties.Count == 0) {
                targetTimeScale = normalTimeScale;
            }

            // Apply Time Scale (메인 핵심 부분)
            if (currentProperty.timeScaleLerp) {
                switch (currentProperty.timeScaleLerpMode) {
                    case TimeScaleLerpModes.Speed:
                        if (currentProperty.lerpSpeed <= 0) currentProperty.lerpSpeed = 1;
                        ApplyTimeScale(Mathf.Lerp(Time.timeScale, targetTimeScale,
                            Time.unscaledDeltaTime * currentProperty.lerpSpeed));
                        break;
                    case TimeScaleLerpModes.Duration:
                        var timeSinceStart = Time.unscaledTime - startedAt;
                        var progress = YisoMathUtils.Remap(timeSinceStart, 0f, currentProperty.timeScaleLerpDuration,
                            0f, 1f);
                        var delta = currentProperty.timeScaleLerpCurve.Evaluate(progress);
                        ApplyTimeScale(Mathf.Lerp(Time.timeScale, targetTimeScale, delta));
                        if (timeSinceStart > currentProperty.timeScaleLerpDuration) {
                            ApplyTimeScale(targetTimeScale);
                            if (lerpingBackToNormal) {
                                lerpingBackToNormal = false;
                                timeScaleProperties.Pop();
                            }
                        }

                        break;
                }
            }
            else {
                ApplyTimeScale(targetTimeScale);
            }
        }

        #region Core

        /// <summary>
        /// Modifies the time scale and time attributes to match the new time scale
        /// </summary>
        /// <param name="newValue"></param>
        protected virtual void ApplyTimeScale(float newValue) {
            if (newValue == timeScaleLastTime) return;
            Time.timeScale = newValue;
            if (newValue != 0) Time.fixedDeltaTime = initialFixedDeltaTime * newValue;
            Time.maximumDeltaTime = initialMaximumDeltaTime * newValue;

            currentTimeScale = Time.timeScale;
            timeScaleLastTime = currentTimeScale;
        }

        /// <summary>
        /// 변경 중인 시간 척도 모두 날리고 바로 즉시 설정
        /// </summary>
        /// <param name="newTimeScale"></param>
        protected virtual void SetTimeScale(float newTimeScale) {
            timeScaleProperties.Clear();
            ApplyTimeScale(newTimeScale);
        }

        /// <summary>
        /// 변경할 Property Stack에 집어넣어
        /// </summary>
        /// <param name="properties"></param>
        protected virtual void SetTimeScale(TimeScaleProperties properties) {
            if (properties.timeScaleLerp && properties.timeScaleLerpMode == TimeScaleLerpModes.Duration) {
                properties.duration = Mathf.Max(properties.duration, properties.timeScaleLerpDuration);
                properties.duration = Mathf.Max(properties.duration, properties.timeScaleLerpDurationOnReset);
            }

            startedAt = Time.unscaledTime;
            timeScaleProperties.Push(properties);
        }

        protected virtual void ResetTimeScale() {
            SetTimeScale(normalTimeScale);
        }

        protected virtual void Revert() {
            if (timeScaleProperties.Count > 0) {
                resetProperty = timeScaleProperties.Peek();
                timeScaleProperties.Pop();
            }

            if (timeScaleProperties.Count == 0) {
                if (resetProperty.timeScaleLerp && resetProperty.timeScaleLerpMode == TimeScaleLerpModes.Duration &&
                    resetProperty.timeScaleLerpOnReset) {
                    lerpingBackToNormal = true;
                    YisoTimeScaleEvent.Trigger(TimeScaleMethods.Change, normalTimeScale,
                        resetProperty.timeScaleLerpDuration, resetProperty.timeScaleLerp,
                        resetProperty.lerpSpeed, true, TimeScaleLerpModes.Duration,
                        resetProperty.timeScaleLerpCurveOnReset, resetProperty.timeScaleLerpDurationOnReset);
                }
                else {
                    ResetTimeScale();
                }
            }
        }

        #endregion

        #region Event

        /// <summary>
        /// Catches TimeScaleEvents and acts on them
        /// </summary>
        /// <param name="timeScaleMethod"></param>
        /// <param name="timeScale"></param>
        /// <param name="duration"></param>
        /// <param name="lerp"></param>
        /// <param name="lerpSpeed"></param>
        /// <param name="infinite"></param>
        /// <param name="timeScaleLerpMode"></param>
        /// <param name="timeScaleLerpCurve"></param>
        /// <param name="timeScaleLerpDuration"></param>
        /// <param name="timeScaleLerpOnReset"></param>
        /// <param name="timeScaleLerpCurveOnReset"></param>
        /// <param name="timeScaleLerpDurationOnReset"></param>
        public virtual void OnTimeScaleEvent(TimeScaleMethods timeScaleMethod, float timeScale, float duration,
            bool lerp, float lerpSpeed, bool infinite,
            TimeScaleLerpModes timeScaleLerpMode = TimeScaleLerpModes.Speed, AnimationCurve timeScaleLerpCurve = null,
            float timeScaleLerpDuration = 0.2f,
            bool timeScaleLerpOnReset = false, AnimationCurve timeScaleLerpCurveOnReset = null,
            float timeScaleLerpDurationOnReset = 0.2f) {
            var timeScaleProperty = new TimeScaleProperties {
                timeScale = timeScale,
                duration = duration,
                timeScaleLerp = lerp,
                lerpSpeed = lerpSpeed,
                infinite = infinite,
                timeScaleLerpOnReset = timeScaleLerpOnReset,
                timeScaleLerpCurveOnReset = timeScaleLerpCurveOnReset,
                timeScaleLerpDurationOnReset = timeScaleLerpDurationOnReset,
                timeScaleLerpMode = timeScaleLerpMode,
                timeScaleLerpCurve = timeScaleLerpCurve,
                timeScaleLerpDuration = timeScaleLerpDuration
            };

            switch (timeScaleMethod) {
                case TimeScaleMethods.Reset:
                    ResetTimeScale();
                    break;
                case TimeScaleMethods.Change:
                    SetTimeScale(timeScaleProperty);
                    break;
                case TimeScaleMethods.Revert:
                    Revert();
                    break;
            }
        }

        /// <summary>
        /// When getting a freeze frame event we stop the time
        /// </summary>
        public virtual void OnFreezeFrameEvent(float duration) {
            var properties = new TimeScaleProperties {
                duration = duration,
                timeScaleLerp = false,
                lerpSpeed = 0f,
                timeScale = 0f
            };
            SetTimeScale(properties);
        }

        /// <summary>
        /// On enable, starts listening for FreezeFrame events
        /// </summary>
        protected override void OnEnable() {
            base.OnEnable();
            YisoFreezeFrameEvent.Register(OnFreezeFrameEvent);
            YisoTimeScaleEvent.Register(OnTimeScaleEvent);
            this.YisoEventStartListening();
        }

        /// <summary>
        /// On disable, stops listening for FreezeFrame events
        /// </summary>
        protected override void OnDisable() {
            base.OnDisable();
            YisoFreezeFrameEvent.Unregister(OnFreezeFrameEvent);
            YisoTimeScaleEvent.Unregister(OnTimeScaleEvent);
            this.YisoEventStopListening();
        }

        #endregion
        
        #region Timer
        
        public void StartTimer(float time, bool forceStart = false) {
            if (IsTimerRunning) {
                if (forceStart) {
                    StopTimer();
                }
                else {
                    LogService.Warn("[TimeManager] Timer is already running.");
                    return;
                }
            }
            
            IsTimerRunning = true;
            timerDuration = time;
            timerElapsedTime = 0f;
            
            var args = YisoPlayerHUDTimerEventArgs.Builder(time)
                .AddOnStart(OnStartTimer)
                .AddOnProgress(OnProgressTimer)
                .AddOnComplete(OnCompleteTimer)
                .HideWhenDone(true)
                .Build();
            HUDUIService.RaiseTimer(YisoPlayerHUDTimerUI.Actions.START, args);
        }

        public void PauseTimer() {
            if (!IsTimerRunning) return;
            HUDUIService.RaiseTimer(YisoPlayerHUDTimerUI.Actions.PAUSE);
            timerState.ChangeState(TimerState.Paused);
        }
        
        public void ResumeTimer() {
            if (!IsTimerRunning) return;
            HUDUIService.RaiseTimer(YisoPlayerHUDTimerUI.Actions.RESUME);
            timerState.ChangeState(TimerState.Running);
        }

        public void StopTimer() {
            if (!IsTimerRunning) return;
            HUDUIService.RaiseTimer(YisoPlayerHUDTimerUI.Actions.STOP);
            IsTimerRunning = false;
        }

        private void OnStartTimer() {
            timerState.ChangeState(TimerState.Running);
        }

        private void OnProgressTimer(float elapsedTime) {
            timerElapsedTime = elapsedTime;
        }

        private void OnCompleteTimer() {
            timerState.ChangeState(TimerState.Stopped);
            timerElapsedTime = timerDuration;
        }

        #endregion
        
        public void OnEvent(YisoInGameEvent inGameEvent) {
            switch (inGameEvent.eventType) {
                case YisoInGameEventTypes.PlayerDeath:
                    PauseTimer();
                    break;
                case YisoInGameEventTypes.RespawnStarted:
                    ResumeTimer();
                    break;
            }
        }
    }
}