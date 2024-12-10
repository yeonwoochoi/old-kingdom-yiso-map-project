using System;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.AI.Decisions {
    /// <summary>
    /// 특정 거리 이내에 Target있으면 True Return
    /// </summary>
    [AddComponentMenu("Yiso/Character/AI/Decisions/AIDecisionDistanceToTarget")]
    public class YisoAIDecisionDistanceToTarget : YisoAIDecision {
        public enum ComparisonModes {
            StrictlyLowerThan,
            LowerThan,
            Equals,
            GreaterThan,
            StrictlyGreaterThan,
            Between
        }

        public ComparisonModes comparisonMode = ComparisonModes.LowerThan;
        public float distance;

        [ShowIf("comparisonMode", ComparisonModes.Between)]
        public float minDistance = 2f;

        public bool ignoreTarget = false;
        [ShowIf("ignoreTarget")] public LayerMask ignoreTargetLayer;
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

        public override bool Decide() {
            return EvaluateDistance();
        }

        protected virtual bool EvaluateDistance() {
            if (Target == null) return false;
            if (ignoreTarget) {
                if (LayerManager.CheckIfInLayer(Target.gameObject, ignoreTargetLayer)) {
                    return false;
                }
            }

            var tempDistance = Vector3.Distance(transform.position, Target.position);

            switch (comparisonMode) {
                case ComparisonModes.StrictlyLowerThan:
                    return tempDistance < distance;
                case ComparisonModes.LowerThan:
                    return tempDistance <= distance;
                case ComparisonModes.Equals:
                    return Math.Abs(tempDistance - distance) < 0.0001f;
                case ComparisonModes.GreaterThan:
                    return tempDistance >= distance;
                case ComparisonModes.StrictlyGreaterThan:
                    return tempDistance > distance;
                case ComparisonModes.Between:
                    return tempDistance > Mathf.Min(minDistance, distance)
                           && tempDistance < Mathf.Max(minDistance, distance);
                default:
                    return false;
            }
        }
    }
}