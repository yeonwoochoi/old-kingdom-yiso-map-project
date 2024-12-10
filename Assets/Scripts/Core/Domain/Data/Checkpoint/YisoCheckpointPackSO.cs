using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Domain.Data.Checkpoint {
    [CreateAssetMenu(fileName = "CheckpointPack", menuName = "Yiso/Data/Checkpoint Pack")]
    public class YisoCheckpointPackSO : ScriptableObject {
        public List<YisoCheckpointSO> checkpointSos;

        public Dictionary<int, YisoCheckpointSO> ToDictionary() =>
            checkpointSos.ToDictionary(so => so.id, so => so);
    }
}