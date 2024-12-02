using UnityEngine;

namespace Tools.AI.Decisions {
    /// <summary>
    /// This decision will return true if the Brain's current target is null, false otherwise
    /// </summary>
    [AddComponentMenu("Yiso/Character/AI/Decisions/AIDecisionTargetIsNull")]
    public class YisoAIDecisionTargetIsNull : YisoAIDecision {
        public YisoAIBrain.AITargetType aiTargetType = YisoAIBrain.AITargetType.Main;
        
        private Transform Target {
            get {
                return aiTargetType switch {
                    YisoAIBrain.AITargetType.Main => brain.mainTarget,
                    YisoAIBrain.AITargetType.Sub => brain.subTarget,
                    YisoAIBrain.AITargetType.SpawnPosition => brain.spawnPositionTarget,
                    _ => null
                };
            }
        }

        /// <summary>
        /// On Decide we check whether the Target is null
        /// </summary>
        /// <returns></returns>
        public override bool Decide() {
            return CheckIfTargetIsNull();
        }

        /// <summary>
        /// Returns true if the Brain's Target is null
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckIfTargetIsNull() {
            return Target == null;
        }
    }
}