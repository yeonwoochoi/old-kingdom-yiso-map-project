using UnityEngine;

namespace Core.Service.ObjectPool {
    public interface IYisoObjectPoolService : IYisoService {
        void WarmPool(GameObject prefab, int size, GameObject root = null);
        T SpawnObject<T>(GameObject prefab, GameObject root = null) where T : Component;
        void ReleaseObject(GameObject clone);
        public void ReleaseAllObject(GameObject prefab);
    }
}