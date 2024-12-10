using System.Collections.Generic;
using Character.Core;
using Core.Behaviour;
using Core.Domain.Data.Checkpoint;
using Core.Service;
using Core.Service.Character;
using Manager;
using UnityEngine;

namespace Spawn {
    [AddComponentMenu("Yiso/Spawn/Checkpoint")]
    public class YisoCharacterCheckPoint : RunIBehaviour {
        public YisoCheckpointSO checkpointSO;
        public Transform spawnPosition;
        public bool forceAssignation = false;
        public YisoCharacter.FacingDirections facingDirection = YisoCharacter.FacingDirections.East;
        public int checkPointOrder;

        protected List<IRespawnable> listeners;

        public int CheckPointId => checkpointSO == null ? -1 : checkpointSO.id;
        public Vector2 SpawnPosition => spawnPosition == null ? transform.position : spawnPosition.transform.position;
        public IYisoCharacterService CharacterService => YisoServiceProvider.Instance.Get<IYisoCharacterService>();

        protected override void Awake() {
            listeners = new List<IRespawnable>();
        }

        public virtual void SpawnPlayer(YisoCharacter player, bool isRespawn) {
            player.RespawnAt(spawnPosition == null ? transform : spawnPosition, facingDirection, isRespawn);

            // 등록된 애들도 같이 스폰 
            foreach (var listener in listeners) {
                listener.OnPlayerRespawn(this, player);
            }
        }

        /// <summary>
        /// Player Spawn될때 같이 이 체크포인트에서 Spawn되고 싶은 애들 등록
        /// 어리 등록하면 되겠지
        /// </summary>
        /// <param name="listener"></param>
        public virtual void AssignObjectToCheckPoint(IRespawnable listener) {
            listeners.Add(listener);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider) {
            TriggerEnter(collider.gameObject);
        }

        private void SaveData() {
            var player = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer();
            player.GameModule.SetCheckpointId(CheckPointId);
        }

        protected virtual void TriggerEnter(GameObject collider) {
            var character = collider.GetComponent<YisoCharacter>();
            if (character == null) return;
            if (character.characterType != YisoCharacter.CharacterTypes.Player) return;
            if (GameManager.HasInstance) {
                GameManager.Instance.CurrentMapController?.SetCurrentCheckpoint(this);
            }
        }
    }
}