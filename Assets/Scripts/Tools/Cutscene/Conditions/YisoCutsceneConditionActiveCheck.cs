using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools.Cutscene.Conditions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Condition/CutsceneConditionActiveCheck")]
    public class YisoCutsceneConditionActiveCheck : YisoCutsceneCondition {
        public bool isActive = false;
        public bool useAndCondition = true; // And 조건인지 Or 조건인지 설정 (true: And, false: Or)
        public List<GameObject> characters;

        public override bool CanPlay() {
            return useAndCondition
                ? characters.All(character => character.activeInHierarchy == isActive)
                : characters.Any(character => character.activeInHierarchy == isActive);
        }
    }
}