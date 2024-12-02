using UnityEngine;

namespace Tools.AI.Decisions.Operators {
    [AddComponentMenu("Yiso/Character/AI/Decisions/AINotDecision")]
    public class YisoAINotDecision : YisoAIDecision {
        public YisoAIDecision decision;

        public override bool Decide() {
            return !decision.Decide();
        }
    }
}