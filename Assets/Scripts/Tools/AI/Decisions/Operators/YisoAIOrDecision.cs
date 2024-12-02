using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools.AI.Decisions.Operators {
    [AddComponentMenu("Yiso/Character/AI/Decisions/AIOrDecision")]
    public class YisoAIOrDecision : YisoAIDecision {
        public List<YisoAIDecision> decisions;

        public override void Initialization() {
            base.Initialization();
            foreach (var decision in decisions) {
                decision.Initialization();
            }
        }

        public override void OnEnterState() {
            base.OnEnterState();
            foreach (var decision in decisions) {
                decision.OnEnterState();
            }
        }

        public override void OnExitState() {
            base.OnExitState();
            foreach (var decision in decisions) {
                decision.OnExitState();
            }
        }

        public override bool Decide() {
            if (decisions == null || decisions.Count == 0) {
                return false;
            }

            return decisions.Any(decision => decision.Decide()); // 하나라도 true이면 true return
        }
    }
}