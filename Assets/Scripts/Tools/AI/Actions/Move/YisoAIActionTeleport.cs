using System.Collections;
using Manager;
using UnityEngine;
using Utils.Beagle;

namespace Tools.AI.Actions.Move {
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionTeleport")]
    public class YisoAIActionTeleport : YisoAIAction {
        public YisoAIBrain.AITargetType aiTargetType = YisoAIBrain.AITargetType.Sub;
        public bool teleportToPlayer = true;
        public float spawnRadius = 4f;

        private Coroutine teleportCo;
        private const int MaxAttempts = 100;

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

        public override void PerformAction() {
        }

        public override void OnEnterState() {
            teleportCo = StartCoroutine(TeleportCo());
        }

        public override void OnExitState() {
            if (teleportCo != null) {
                StopCoroutine(teleportCo);
            }
        }

        protected virtual IEnumerator TeleportCo() {
            if (Target == null && !teleportToPlayer) yield break;
            var origin = teleportToPlayer ? GameManager.Instance.Player.transform.position : Target.transform.position;
            var teleportCount = 0;

            while (teleportCount <= MaxAttempts) {
                var positionInRadius = YisoPhysicsUtils.GetRandomPositionInRadius(origin, 0.5f, spawnRadius);
                if (IsPositionOccupied(positionInRadius)) {
                    teleportCount++;
                    yield return null;
                    continue;
                }
                
                brain.owner.transform.position = positionInRadius;
                yield break;
            }
        }

        protected virtual bool IsPositionOccupied(Vector2 position) {
            return Physics2D.OverlapCircle(position, 0.5f,
                LayerManager.ObstaclesLayerMask | LayerManager.EnemiesLayerMask | LayerManager.PlayerLayerMask |
                LayerManager.MapLayerMask);
        }
    }
}