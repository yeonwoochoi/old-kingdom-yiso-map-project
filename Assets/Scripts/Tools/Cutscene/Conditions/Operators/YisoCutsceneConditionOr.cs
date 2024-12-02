using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools.Cutscene.Conditions.Operators {
    [AddComponentMenu("Yiso/Tools/Cutscene/Condition/CutsceneConditionOr")]
    public class YisoCutsceneConditionOr : YisoCutsceneCondition {
        public List<YisoCutsceneCondition> conditions;

        public override bool CanPlay() {
            return conditions.Any(condition => condition.CanPlay());
        }
    }
}