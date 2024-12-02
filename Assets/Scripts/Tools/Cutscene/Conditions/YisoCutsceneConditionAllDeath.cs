using System.Collections.Generic;
using System.Linq;
using Character.Health;
using UnityEngine;

namespace Tools.Cutscene.Conditions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Condition/CutsceneConditionAllDeath")]
    public class YisoCutsceneConditionAllDeath : YisoCutsceneCondition {
        public List<YisoHealth> targets;

        public override bool CanPlay() {
            return targets.Where(target => target.gameObject.activeInHierarchy).All(target => !target.IsAlive);
        }
    }
}