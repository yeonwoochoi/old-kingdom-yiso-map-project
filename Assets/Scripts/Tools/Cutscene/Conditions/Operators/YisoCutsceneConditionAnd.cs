using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools.Cutscene.Conditions.Operators {
    [AddComponentMenu("Yiso/Tools/Cutscene/Condition/CutsceneConditionAnd")]
    public class YisoCutsceneConditionAnd : YisoCutsceneCondition {
        public List<YisoCutsceneCondition> conditions;

        public override bool CanPlay() {
            return conditions.All(condition => condition.CanPlay());
        }
    }
}