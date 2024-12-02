using System;
using System.Collections;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using Tools.Feedback.Core.Channel;
using Tools.Feedback.Feedbacks;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tools.Feedback.Core {
    [Serializable]
    public abstract class YisoFeedback {
        #region Properties

        public const string RandomnessGroupName = "Feedback Randomness";
        public const string RangeGroupName = "Feedback Range";

        [YisoFeedbackInspectorGroup("Feedback Settings", true, 0, false, true)] [Tooltip("피드백이 활성 상태 여부")]
        public bool active = true;

        [HideInInspector] public int uniqueID;

        [Tooltip("Inspector에서 표시될 피드백의 이름")] public string label = "YisoFeedback";

        [Tooltip("채널 설정 방식. 같은 채널 ID인 경우에 이 피드백이 전달됨.")]
        public YisoChannelModes channelMode = YisoChannelModes.Int;

        [Tooltip("채널 설정 방식: Int (Channel ID)")] [ShowIf("channelMode", YisoChannelModes.Int)]
        public int channel = 0;

        [Tooltip("채널 설정 방식: SO (Channel SO)")] [ShowIf("channelMode", YisoChannelModes.Int)]
        public YisoChannel channelDefinition = null;

        [Tooltip("이 피드백이 일어날 확률 (0에서 100 사이)")] [Range(0, 100)]
        public float chance = 100f;

        [Tooltip("Inspector 창에서 피드백이 어떤 색으로 표시될지 설정")]
        public Color displayColor = Color.black;

        [Tooltip("YisoFeedbackTiming 객체를 통해 타이밍 관련 설정")]
        public YisoFeedbackTiming timing;

        [Tooltip("YisoFeedbackTargetAcquisition 객체를 통해 자동으로 타겟을 설정")]
        public YisoFeedbackTargetAcquisition automatedTargetAcquisition;

        [Tooltip("Output의 무작위성을 활성화하거나 비활성화 (true이면 randomMultiplier 적용)")]
        [YisoFeedbackInspectorGroup(RandomnessGroupName, true, 58, false, true)]
        public bool randomizeOutput = false;

        [Tooltip("Output의 결과값에 곱해질 무작위 범위 (Min, Max)")] [ShowIf("randomizeOutput"), YisoFeedbackVector("Min,", "Max")]
        public Vector2 randomMultiplier = new Vector2(0.8f, 1f);

        [Tooltip("반경 기반으로 Shaker가 반응할지 설정 (true일 경우 지정된 반경 내의 Shaker만 반응)")]
        [YisoFeedbackInspectorGroup(RangeGroupName, true, 47, false, true)]
        public bool useRange = false;

        [Tooltip("피드백 반응의 최대 반경 거리")] public float rangeDistance = 5f;

        [Tooltip("RangeFalloff를 통해 거리 기반으로 Shake 강도를 감소시킬지 여부")]
        public bool useRangeFalloff = false;

        [Tooltip("Shake 강도의 거리 기반 감소를 정의하는 애니메이션 곡선 (x: 거리 비율, y: 강도)")]
        public AnimationCurve rangeFalloff = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

        [Tooltip("RangeFalloff 곡선의 y축 값을 0과 1로 재매핑하기 위한 설정")] [YisoFeedbackVector("Zero", "One")]
        public Vector2 remapRangeFalloff = new Vector2(0f, 1f);

        [HideInInspector] public YisoFeedBacks owner;

        // Pause시 여기 등록된 IEnumerator 실행됨
        public virtual IEnumerator Pause => null;

        // 이전 피드백이 모두 실행될 때까지 기다렸다가 실행할지 여부
        public virtual bool HoldingPause => false;

        // 현재 피드백 "도중에" 멈추고, 이전 피드백을 다시 시작
        public virtual bool LooperPause => false;

        // 부모 YisoFeedbacks에서 Resume 호출해야 다시 재개됨
        public virtual bool ScriptDrivenPause { get; set; }

        // 0보다 클 경우 Script에서 Resume 안해도 해당 시간 이후 자동 실행됨
        public virtual float ScriptDrivenPauseAutoResume { get; set; }

        // 현재 피드백 "완전히 끝난 후" 다시 처음부터 반복적으로 실행됨
        public virtual bool LooperStart => false;

        // Inspector에 Channel 그룹 표시할지 말지 여부
        public virtual bool HasChannel => false;

        // Randomness group을 inspector에 표시할지 말지 여부
        public virtual bool HasRandomness => false;

        // false 값이면 ForceInitialState() 호출해도 아무 일 없음 
        public virtual bool CanForceInitialValue => false;

        // ForceInitialValue가 두 프레임에 걸쳐 적용될지 여부를 나타냅니다. (작업이 복잡하거나 용량이 큰 경우)
        public virtual bool ForceInitialValueDelayed => false;

        // 자동으로 모드에 맞게 (게임 오브젝트, 부모, 자식, 또는 참조 홀더) 대상 객체를 찾을 수 있는지 여부
        public virtual bool HasAutomatedTargetAcquisition => false;

        // 강제 참조 모드에서 사용할 참조 홀더를 설정하는 프로퍼티 (보통은 자동으로 설정됨)
        public virtual YisoFeedbackReferenceHolder ForcedReferenceHolder { get; set; }

        // Inspector에 Range 그룹 표시할지 말지 여부
        public virtual bool HasRange => false;

        public virtual bool HasCustomInspectors => false;

#if UNITY_EDITOR
        // Inspector에 해당 피드백 색깔 재정의 할때
        public virtual Color FeedbackColor => Color.white;
#endif

        // CoolDown 여부
        public virtual bool InCooldown => (timing.coolDownDuration > 0f) && (FeedbackTime - lastPlayTimestamp < timing.coolDownDuration);

        // Feedback 재생 여부
        public virtual bool IsPlaying { get; set; }

        public virtual float ComputeIntensity(float intensity, Vector3 position) {
            var result = timing.constantIntensity ? 1f : intensity;
            result *= ComputedRandomMultiplier;
            result *= owner.ComputeRangeIntensityMultiplier(position);
            return result;
        }
        
        public virtual float ComputedRandomMultiplier => randomizeOutput ? Random.Range(randomMultiplier.x, randomMultiplier.y) : 1f;

        public virtual TimescaleModes ComputedTimescaleMode => owner.forceTimescaleMode ? owner.forcedTimescaleMode : timing.timescaleMode;

        public virtual bool InScaledTimescaleMode => owner.forceTimescaleMode ? owner.forcedTimescaleMode == TimescaleModes.Scaled : timing.timescaleMode == TimescaleModes.Scaled;

        public virtual float FeedbackTime {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return (float) EditorApplication.timeSinceStartup;
                }
#endif
                if (timing.UseScriptDrivenTimescale) {
                    return timing.ScriptDrivenTime;
                }

                if (owner.forceTimescaleMode) {
                    return owner.forcedTimescaleMode == TimescaleModes.Scaled ? Time.time : Time.unscaledTime;
                }

                return timing.timescaleMode == TimescaleModes.Scaled ? Time.time : Time.unscaledTime;
            }
        }

        public virtual string RequiredTargetText => "";
        
        protected float lastPlayTimestamp = -1f;

        #endregion
    }
}