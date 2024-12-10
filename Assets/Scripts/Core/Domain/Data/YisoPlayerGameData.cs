using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Domain.Data {
    [Serializable]
    public class YisoPlayerGameData {
        public int checkpointId = -1;
        public Vector3 lastPosition = Vector3.zero;
        public int mapId = -1;
        public List<int> petIds = new();
    }
}