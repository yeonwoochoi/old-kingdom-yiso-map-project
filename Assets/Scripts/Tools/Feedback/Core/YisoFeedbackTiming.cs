using System;
using Sirenix.OdinInspector;
using Tools.Feedback.Sequencing;
using UnityEngine;

namespace Tools.Feedback.Core {
    public enum TimescaleModes {
        Scaled, Unscaled
    }
    
    [Serializable]
    public class YisoFeedbackTiming {
        /// <summary>
        /// YisoFeedbacks 실행 방향에 따라 피드백 실행 여부를 결정
        /// Always: 항상 실행됨
        /// OnlyWhenForwards: Forward 방향으로 Feedback list가 실행될때만 실행됨
        /// OnlyWhenBackwards: Backward 방향으로 Feedback list가 실행될때만 실행됨
        /// </summary>
        public enum YisoFeedbacksDirectionConditions { Always, OnlyWhenForwards, OnlyWhenBackwards }
        
        /// <summary>
        /// FollowFeedbacksDirection: MMFeedbacks 실행 방향을 따름
        /// OppositeFeedbacksDirection: MMFeedbacks 실행 방향과 반대로 실행
        /// Always Normal: 항상 정방향으로 실행
        /// Always Rewind: 항상 역방향으로 실행
        /// </summary>
        public enum PlayDirections { FollowFeedbacksDirection, OppositeFeedbacksDirection, AlwaysNormal, AlwaysRewind }

        [Header("Timescale")]
        public TimescaleModes timescaleMode = TimescaleModes.Scaled;

        [Header("Exceptions")]
        public bool excludeFromHoldingPauses = false; // true이면 holding pause를 무시하고 이전 feedback들 안 기다리고 바로 실행됨. (holding Pause를 무력화시킨다 보면됨)
        public bool contributeToTotalDuration = true; // YisoFeedbacks에서 total duration 게산할때 이 feedback 포함할건지

        [Header("Delays")]
        public float initialDelay = 0f;
        public float coolDownDuration = 0f;
        
        [Header("Stop")]
        public bool interruptsOnStop = true;

        [Header("Repeat")]
        public int numberOfRepeats = 0;
        public bool repeatForever = false;
        public float delayBetweenRepeats = 1f;

        [Header("Play Direction")]
        public YisoFeedbacksDirectionConditions feedbacksDirectionCondition = YisoFeedbacksDirectionConditions.Always;
        public PlayDirections playDirection = PlayDirections.FollowFeedbacksDirection;

        [Header("Intensity")]
        public bool constantIntensity = false; // YisoFeedbacks의 intensity 와 독립적으로. 일정한 intensity
        public bool useIntensityInterval = false; // intensity가 아래 min, max 값 사이값인 경우에만 실행할건지
        [ShowIf("useIntensityInterval")] public float intensityIntervalMin = 0f;
        [ShowIf("useIntensityInterval")] public float intensityIntervalMax = 0f;

        [Header("Sequence")]
        public YisoSequence sequence;
        public int trackID = 0;
        public bool quantized = false;
        [ShowIf("quantized")] public int targetBPM = 120;
        
        public virtual bool UseScriptDrivenTimescale { get; set; }
        public virtual float ScriptDrivenDeltaTime { get; set; }
        public virtual float ScriptDrivenTime { get; set; }
        
    }
}