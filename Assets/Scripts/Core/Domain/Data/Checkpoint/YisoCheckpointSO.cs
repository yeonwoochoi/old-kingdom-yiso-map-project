using UnityEngine;

namespace Core.Domain.Data.Checkpoint {
    [CreateAssetMenu(fileName = "Checkpoint", menuName = "Yiso/Data/Checkpoint")]
    public class YisoCheckpointSO : ScriptableObject {
        public int id;
        public int mapId;
        public Vector3 playerPosition;
    }
}