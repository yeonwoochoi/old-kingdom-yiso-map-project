using Character.Ability;
using Character.Core;
using UnityEngine;

namespace Tools.AI.Actions {
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionFaceTowardsTarget")]
    public class YisoAIActionFaceTowardsTarget : YisoAIAction {
        public YisoAIBrain.AITargetType aiTargetType = YisoAIBrain.AITargetType.Main;

        protected YisoCharacterOrientation2D orientation2D;
        
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

        public override void Initialization() {
            if (!ShouldInitialize) return;
            base.Initialization();
            orientation2D = gameObject.GetComponentInParent<YisoCharacter>()?.FindAbility<YisoCharacterOrientation2D>();
        }

        public override void PerformAction() {
            FaceTarget();
        }

        protected virtual void FaceTarget() {
            if (Target == null || orientation2D == null) return;
            var distance = Target.position - transform.position;
            var faceDirection = YisoCharacter.FacingDirections.South;
            if (Mathf.Abs(distance.y) > Mathf.Abs(distance.x)) {
                faceDirection = distance.y > 0
                    ? YisoCharacter.FacingDirections.North
                    : YisoCharacter.FacingDirections.South;
            }
            else {
                faceDirection = distance.x > 0
                    ? YisoCharacter.FacingDirections.East
                    : YisoCharacter.FacingDirections.West;
            }

            orientation2D.Face(faceDirection);
        }
    }
}