using Manager_Temp_;
using UnityEngine;

namespace Tools.Cutscene.Conditions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Condition/CutsceneConditionTimerChecker")]
    public class YisoCutsceneConditionTimerChecker: YisoCutsceneCondition {
        [Tooltip("Checks if the timer has elapsed")]
        public bool checkTimerElapsed = false;
        
        public override bool CanPlay() {
            if (!TimeManager.HasInstance) return false;
            if (checkTimerElapsed) return TimeManager.Instance.TimerProgressRate >= 1f;
            return TimeManager.Instance.TimerProgressRate is < 1f and >= 0f;
        }
    }
}