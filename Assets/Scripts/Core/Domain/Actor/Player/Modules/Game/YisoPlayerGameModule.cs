using System.Collections.Generic;
using Core.Domain.Actor.Player.Modules.Base;
using Core.Domain.Data;
using Core.Domain.Data.Checkpoint;
using Core.Service.Domain;
using Core.Service.Server;
using UnityEngine;

namespace Core.Domain.Actor.Player.Modules.Game {
    public class YisoPlayerGameModule : YisoPlayerBaseModule {
        public int CheckpointId { get; private set; }
        public Vector3 LastPosition { get; private set; }
        public int MapId { get; private set; }
        
        public List<int> PetIds { get; private set; }
        public YisoPlayerGameModule(YisoPlayer player) : base(player) { }

        public void SetCheckpointId(int id) {
            CheckpointId = id;
        }

        public void SetLastPosition(Vector3 position) {
            LastPosition = position;
        }

        public void SetMapId(int mapId) {
            MapId = mapId;
        }

        public bool TryGetCheckpointSO(out YisoCheckpointSO so) =>
            GetService<IYisoDomainService>().TryGetCheckpointSO(CheckpointId, out so);

        public void LoadData(YisoPlayerData data) {
            CheckpointId = data.gameData.checkpointId;
            LastPosition = data.gameData.lastPosition;
            MapId = data.gameData.mapId;
            PetIds = data.gameData.petIds;
        }

        public void SaveData(ref YisoPlayerData data) {
            data.gameData.checkpointId = CheckpointId;
            data.gameData.mapId = MapId;
            data.gameData.lastPosition = LastPosition;
        }
        
        public void ResetData() {
            CheckpointId = -1;
            LastPosition = Vector3.zero;
            MapId = -1;
        }
    }
}