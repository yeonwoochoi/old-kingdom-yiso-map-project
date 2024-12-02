using System;
using Tools.Vision;
using UnityEngine;

namespace Tools.AI.Decisions {
    [AddComponentMenu("Yiso/Character/AI/Decisions/AIDecisionDetectTargetConeOfVision")]
    public class YisoAIDecisionDetectTargetConeOfVision : YisoAIDecision {
        public YisoAIBrain.AITargetType aiTargetType = YisoAIBrain.AITargetType.Main;
        public YisoConeOfVision targetConeOfVision;

        private Transform Target {
            get {
                return aiTargetType switch {
                    YisoAIBrain.AITargetType.Main => brain.mainTarget,
                    YisoAIBrain.AITargetType.Sub => brain.subTarget,
                    YisoAIBrain.AITargetType.SpawnPosition => brain.spawnPositionTarget,
                    _ => null
                };
            }
            set {
                switch (aiTargetType) {
                    case YisoAIBrain.AITargetType.Main:
                        brain.mainTarget = value;
                        break;
                    case YisoAIBrain.AITargetType.Sub:
                        brain.subTarget = value;
                        break;
                }
            }
        }

        public override void Initialization() {
            base.Initialization();
            if (targetConeOfVision == null) {
                targetConeOfVision = gameObject.GetComponentInParent<YisoConeOfVision>();
            }
        }

        public override bool Decide() {
            return DetectTarget();
        }

        protected virtual bool DetectTarget() {
            if (targetConeOfVision.visibleTargets.Count == 0) {
                Target = null;
                return false;
            }
            else {
                Target = targetConeOfVision.visibleTargets[0];
                return true;
            }
        }
    }
}