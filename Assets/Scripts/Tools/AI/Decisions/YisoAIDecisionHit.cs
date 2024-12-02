using Character.Health;
using UnityEngine;

namespace Tools.AI.Decisions {
    /// <summary>
    /// This decision returns true if the Character got hit this frame, or after the specified number of hits has been reached.
    /// </summary>
    [AddComponentMenu("Yiso/Character/AI/Decisions/AIDecisionHit")]
    public class YisoAIDecisionHit : YisoAIDecision {
        public YisoAIBrain.AITargetType aiTargetType = YisoAIBrain.AITargetType.Main;
        public int numberOfHits = 1;
        public bool memorizeAttacker = true;

        protected int hitCounter = 0;
        protected YisoHealth health;

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
            health = brain.gameObject.GetComponentInParent<YisoHealth>();
            hitCounter = 0;
        }

        public override void OnEnterState() {
            base.OnEnterState();
            hitCounter = 0;
        }

        public override void OnExitState() {
            base.OnExitState();
            hitCounter = 0;
        }

        public override bool Decide() {
            return hitCounter >= numberOfHits;
        }

        protected virtual void OnHit(GameObject attacker) {
            if (memorizeAttacker) {
                Target = attacker.transform;
            }

            hitCounter++;
        }

        protected override void OnEnable() {
            base.OnEnable();
            if (health == null) {
                health = brain.gameObject.GetComponentInParent<YisoHealth>();
            }

            if (health != null) {
                health.onHit += OnHit;
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (health != null) {
                health.onHit -= OnHit;
            }
        }
    }
}