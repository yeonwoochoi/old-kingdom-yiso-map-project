using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.AI.Decisions {
    /// <summary>
    /// 일정시간이 지나면 다음 State으로 Transition
    /// </summary>
    [AddComponentMenu("Yiso/Character/AI/Decisions/AIDecisionTimeInState")]
    public class YisoAIDecisionTimeInState : YisoAIDecision {
        [Title("Time")] public float minDuration = 2f;
        public float maxDuration = 2f;

        protected float duration;

        public override void Initialization() {
            base.Initialization();
            RandomizeTime();
        }

        public override void OnEnterState() {
            base.OnEnterState();
            RandomizeTime();
        }

        public override bool Decide() {
            return EvaluateTime();
        }

        protected virtual bool EvaluateTime() {
            if (brain == null) return false;
            return brain.timeInThisState >= duration;
        }

        protected virtual void RandomizeTime() {
            duration = Random.Range(minDuration, maxDuration);
        }
    }
}