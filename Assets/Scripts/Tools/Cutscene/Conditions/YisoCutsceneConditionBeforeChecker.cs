using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools.Cutscene.Conditions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Condition/CutsceneConditionBeforeChecker")]
    public class YisoCutsceneConditionBeforeChecker : YisoCutsceneCondition {
        public List<YisoCutsceneTrigger> triggers;

        public override bool CanPlay() {
            return triggers.All(trigger => trigger.IsDone);
        }
    }
}