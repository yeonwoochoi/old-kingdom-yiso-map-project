using Character.Ability;
using Character.Core;
using UnityEngine;

namespace Tools.AI.Actions.Attack {
    public abstract class YisoAIActionSkill : YisoAIAction {
        public YisoAIBrain.AITargetType aiTargetType = YisoAIBrain.AITargetType.Main;

        protected YisoCharacter character;
        protected YisoCharacterOrientation2D orientation2D;

        protected Transform Target {
            get {
                return aiTargetType switch {
                    YisoAIBrain.AITargetType.Main => brain.mainTarget,
                    YisoAIBrain.AITargetType.Sub => brain.subTarget,
                    YisoAIBrain.AITargetType.SpawnPosition => brain.spawnPositionTarget,
                    _ => null
                };
            }
        }

        public override void Initialization() {
            base.Initialization();
            character = gameObject.GetComponentInParent<YisoCharacter>();
            orientation2D = character?.FindAbility<YisoCharacterOrientation2D>();
        }
    }
}