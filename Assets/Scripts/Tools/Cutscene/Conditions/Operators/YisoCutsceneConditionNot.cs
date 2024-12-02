using UnityEngine;

namespace Tools.Cutscene.Conditions.Operators {
    [AddComponentMenu("Yiso/Tools/Cutscene/Condition/CutsceneConditionNot")]
    public class YisoCutsceneConditionNot : YisoCutsceneCondition {
        public YisoCutsceneCondition condition;

        public override bool CanPlay() {
            return !condition.CanPlay();
        }
    }
}