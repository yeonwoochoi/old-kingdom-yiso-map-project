using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Domain.Data {
    [Serializable]
    public class YisoPlayerGameData {
        public int checkpointId = -1;
        public YisoPlayerGamePositionData lastPosition = new();
        public int mapId = -1;
        public List<int> petIds = new();

        public void SetPosition(Vector3 position) {
            lastPosition.x = position.x;
            lastPosition.y = position.y;
            lastPosition.z = position.z;
        }

        public Vector3 GetPosition() => new(
            lastPosition.x,
            lastPosition.y,
            lastPosition.z
        );
    }

    [Serializable]
    public class YisoPlayerGamePositionData {
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
    }
}