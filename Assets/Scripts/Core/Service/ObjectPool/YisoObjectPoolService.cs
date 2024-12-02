using System;
using Core.Domain.Types;
using Core.ObjectPool;
using Core.Service.Scene;
using UnityEngine;

namespace Core.Service.ObjectPool {
    public class YisoObjectPoolService : IYisoObjectPoolService {
        private readonly YisoObjectPoolController poolController;
        
        public void WarmPool(GameObject prefab, int size, GameObject root = null) =>
            poolController.WarmPool(prefab, size, root);

        public T SpawnObject<T>(GameObject prefab, GameObject root = null) where T : Component =>
            poolController.SpawnObject<T>(prefab, root);

        public void ReleaseObject(GameObject clone) => poolController.ReleaseObject(clone);

        public void ReleaseAllObject() => poolController.ReleaseAllObject();

        public void ReleaseAllObject(GameObject prefab) => poolController.ReleaseAllObject(prefab);

        private void OnSceneChanged(YisoSceneTypes beforeScene, YisoSceneTypes afterScene) {
            poolController.ReleaseAllObject();
        }

        private YisoObjectPoolService() { }

        ~YisoObjectPoolService() {
            YisoServiceProvider.Instance.Get<IYisoSceneService>().UnregisterOnSceneChanged(OnSceneChanged);
        }
        
        private YisoObjectPoolService(YisoObjectPoolController poolController) {
            this.poolController = poolController;
            YisoServiceProvider.Instance.Get<IYisoSceneService>().RegisterOnSceneChanged(OnSceneChanged);
        }
        
        public bool IsReady() => true;
        public void OnDestroy() { }
        internal static YisoObjectPoolService CreateService(YisoObjectPoolController poolController) => new (poolController);

        [Serializable]
        public class Settings {
            public GameObject objectPoolPrefab;
        }
    }
}