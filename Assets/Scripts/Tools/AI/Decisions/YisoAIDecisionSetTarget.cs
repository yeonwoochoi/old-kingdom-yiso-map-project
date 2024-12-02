using UnityEngine;

namespace Tools.AI.Decisions {
    [AddComponentMenu("Yiso/Character/AI/Decisions/AIDecisionSetTarget")]
    public class YisoAIDecisionSetTarget : YisoAIDecision {
        public YisoAIBrain.AITargetType targetType;
        public Transform target;

        public override bool Decide() {
            if (target == null) {
                return false;
            }
            if (targetType == YisoAIBrain.AITargetType.Main) {
                brain.mainTarget = target;
            }
            else {
                brain.subTarget = target;
            }
            return true;
        }
    }
}