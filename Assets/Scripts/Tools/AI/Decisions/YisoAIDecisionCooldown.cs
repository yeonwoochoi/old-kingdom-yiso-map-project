using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.AI.Decisions {
    [AddComponentMenu("Yiso/Character/AI/Decisions/AIDecisionCooldown")]
    public class YisoAIDecisionCooldown : YisoAIDecision {
        [Title("State")] public string stateName;

        [Title("Time")] public float minCoolTime = 2f;
        public float maxCoolTime = 2f;

        private float coolTime;
        private YisoAIState targetState;

        public override void Initialization() {
            base.Initialization();
            RandomizeTime();
        }

        public override void OnEnterState() {
            base.OnEnterState();
            RandomizeTime();
        }

        public override bool Decide() {
            if (string.IsNullOrEmpty(stateName)) return false;
            targetState ??= brain.FindState(stateName);
            if (targetState == null) return false;
            return Mathf.Abs(Time.time - targetState.lastStateExecutionTime) >= coolTime;
        }

        protected virtual void RandomizeTime() {
            coolTime = Random.Range(minCoolTime, maxCoolTime);
        }
    }
}