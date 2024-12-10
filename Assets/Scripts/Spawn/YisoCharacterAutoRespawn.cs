using System.Collections.Generic;
using Character.Core;
using Character.Health;
using Core.Behaviour;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;

namespace Spawn {
    /// <summary>
    /// 해당 Interface 상속받은 class를 가진 GameObject는 player가 respawn할때 같이 Respawn됨
    /// </summary>
    public interface IRespawnable {
        void OnPlayerRespawn(YisoCharacterCheckPoint checkPoint, YisoCharacter player);
    }

    /// <summary>
    /// player가 respawn할때 같이 Respawn됨 + 등록된 checkpoint에서
    /// 어리 무딘과 같이 따라다니는 Pet들
    /// </summary>
    [AddComponentMenu("Yiso/Spawn/Auto Respawn")]
    public class YisoCharacterAutoRespawn : RunIBehaviour, IRespawnable {
        [Title("CheckPoints")] public bool alwaysRespawnInAllCheckPoints = false; // 모든 checkpoint에 respawn되게끔

        [ShowIf("@!alwaysRespawnInAllCheckPoints")]
        public List<YisoCharacterCheckPoint> associatedCheckPoints; // 해당 checkpoint에만 respawn됨.

        [Title("Respawn Position")] public float radius = 5f;

        public LayerMask collisionMask =
            LayerManager.ObstaclesLayerMask | LayerManager.MapLayerMask | LayerManager.PlayerLayerMask;

        public int maxAttempts = 30;

        protected YisoCharacter character;
        protected YisoHealth health;

        protected override void Awake() {
            character = gameObject.GetComponentInChildren<YisoCharacter>();
            health = gameObject.GetComponentInChildren<YisoHealth>();
        }

        public void OnPlayerRespawn(YisoCharacterCheckPoint checkPoint, YisoCharacter player) {
            if (alwaysRespawnInAllCheckPoints) {
                Revive(player);
            }
            else {
                if (!associatedCheckPoints.Contains(checkPoint)) return;
                Revive(player);
            }
        }

        public virtual void Kill() {
        }

        protected virtual void Revive(YisoCharacter player) {
            if (character != null) {
                character.RespawnAt(FindValidSpawnPosition(player.transform.position),
                    YisoCharacter.FacingDirections.South, true);
            }
            else if (health != null) {
                health.Revive();
            }
        }

        protected virtual Vector2 FindValidSpawnPosition(Vector2 playerPosition) {
            return YisoPhysicsUtils.FindValidPositionInCircle(playerPosition, radius, collisionMask, maxAttempts);
        }
    }
}