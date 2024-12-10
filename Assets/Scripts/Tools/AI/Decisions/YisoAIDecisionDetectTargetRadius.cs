using System.Collections.Generic;
using Character.Ability;
using Character.Core;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;

namespace Tools.AI.Decisions {
    /// <summary>
    /// 일정 Radius 안에 Target이 있으면 True Return
    /// </summary>
    [AddComponentMenu("Yiso/Character/AI/Decisions/AIDecisionDetectTargetRadius")]
    public class YisoAIDecisionDetectTargetRadius : YisoAIDecision {
        public enum ObstaclesDetectionModes {
            Boxcast,
            Raycast
        }

        [Title("Basic")] public ObstaclesDetectionModes obstaclesDetectionMode = ObstaclesDetectionModes.Raycast;
        public float radius = 3f;
        public Vector3 detectionOriginOffset = Vector3.zero; // Circle 중심 Offset

        [Title("Target")] public YisoAIBrain.AITargetType aiTargetType = YisoAIBrain.AITargetType.Main;
        public LayerMask targetLayer;
        public float targetCheckFrequency = 1f;
        public int overlapMaximum = 10;

        [Title("Obstacle")] public bool obstacleDetection = true;
        public LayerMask obstacleMask = LayerManager.ObstaclesLayerMask | LayerManager.MapLayerMask;

        protected Collider2D collider;
        protected YisoCharacter character;
        protected YisoCharacterOrientation2D orientation2D;

        protected bool initialized = false;
        protected float lastTargetCheckTimestamp = 0f;
        protected bool lastReturnValue = false;
        protected Vector2 facingDirection;
        protected Vector2 raycastOrigin;
        protected Vector2 boxcastDirection;
        protected Collider2D[] results;
        protected List<Transform> potentialTargets;
        protected RaycastHit2D hit;

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
            potentialTargets = new List<Transform>();
            character = gameObject.GetComponentInParent<YisoCharacter>();
            orientation2D = character?.FindAbility<YisoCharacterOrientation2D>();
            collider = gameObject.GetComponentInParent<Collider2D>();
            initialized = true;
            results = new Collider2D[overlapMaximum];
        }

        public override bool Decide() {
            return DetectTarget();
        }

        protected virtual bool DetectTarget() {
            if (Time.time - lastTargetCheckTimestamp < targetCheckFrequency) return lastReturnValue;
            potentialTargets.Clear();
            lastTargetCheckTimestamp = Time.time;

            if (orientation2D != null) {
                facingDirection = orientation2D.DirectionFineValue;
                raycastOrigin.x = transform.position.x + facingDirection.x * detectionOriginOffset.x / 2;
                raycastOrigin.y = transform.position.y + facingDirection.y * detectionOriginOffset.y / 2;
            }
            else {
                raycastOrigin = transform.position + detectionOriginOffset;
            }

            // 해당 Radius 안에 있는 모든 Collider 수를 return
            var numberOfResults = Physics2D.OverlapCircleNonAlloc(raycastOrigin, radius, results, targetLayer);
            if (numberOfResults == 0) {
                lastReturnValue = false;
                return false;
            }

            var min = Mathf.Min(overlapMaximum, numberOfResults);
            for (int i = 0; i < min; i++) {
                if (results[i] == null) continue;
                if (results[i].gameObject == brain.owner.gameObject ||
                    results[i].transform.IsChildOf(transform)) continue;

                potentialTargets.Add(results[i].gameObject.transform);
            }

            if (potentialTargets.Count == 0) {
                lastReturnValue = false;
                return false;
            }

            // we sort our targets by distance
            potentialTargets.Sort(delegate(Transform a, Transform b) {
                if (a == null || b == null) return 0;
                return Vector2.Distance(this.transform.position, a.transform.position)
                    .CompareTo(Vector2.Distance(this.transform.position, b.transform.position));
            });

            if (!obstacleDetection && potentialTargets[0] != null) {
                Target = potentialTargets[0].gameObject.transform;
                lastReturnValue = true;
                return true;
            }

            // we return the first unobscured target
            foreach (var target in potentialTargets) {
                boxcastDirection = target.gameObject.YisoGetComponentNoAlloc<Collider2D>().bounds.center -
                                   collider.bounds.center;

                hit = obstaclesDetectionMode == ObstaclesDetectionModes.Boxcast
                    ? Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, boxcastDirection.normalized,
                        boxcastDirection.magnitude, obstacleMask)
                    : YisoDebugUtils.RayCast(collider.bounds.center, boxcastDirection, boxcastDirection.magnitude,
                        obstacleMask, Color.yellow, true);

                if (!hit) {
                    Target = target;
                    lastReturnValue = true;
                    return true;
                }
            }

            lastReturnValue = false;
            return false;
        }

        protected virtual void OnDrawGizmosSelected() {
            raycastOrigin.x = transform.position.x + facingDirection.x * detectionOriginOffset.x / 2;
            raycastOrigin.y = transform.position.y + detectionOriginOffset.y;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(raycastOrigin, radius);
        }
    }
}