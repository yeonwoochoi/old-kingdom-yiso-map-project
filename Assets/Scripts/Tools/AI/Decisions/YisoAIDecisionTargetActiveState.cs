using Character.Core;
using UnityEngine;
using Utils;
using Utils.Beagle;

namespace Tools.AI.Decisions {
    /// <summary>
    /// This decision will return true if the Brain's current target is active state (not dead, not frozen), false otherwise
    /// </summary>
    [AddComponentMenu("Yiso/Character/AI/Decisions/AIDecisionTargetIsActiveState")]
    public class YisoAIDecisionTargetActiveState : YisoAIDecision {
        public YisoAIBrain.AITargetType aiTargetType = YisoAIBrain.AITargetType.Main;
        protected YisoCharacter targetCharacter;
        
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

        public override bool Decide() {
            return CheckIfTargetIsActive();
        }

        protected virtual bool CheckIfTargetIsActive() {
            if (Target == null) return false;

            targetCharacter = Target.gameObject.YisoGetComponentNoAlloc<YisoCharacter>();
            if (targetCharacter == null) return false;
            return targetCharacter.conditionState.CurrentState is not YisoCharacterStates.CharacterConditions.Dead and
                not YisoCharacterStates.CharacterConditions.Frozen;
        }
    }
}