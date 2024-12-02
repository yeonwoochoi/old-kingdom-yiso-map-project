using Core.Behaviour;
using Core.Service;
using Core.Service.ObjectPool;
using UnityEngine;

namespace Test {
    public class PoolTest : RunIBehaviour {
        [SerializeField] private GameObject poolObjectPrefab;
        [SerializeField] private GameObject parent = null;
        [SerializeField] private int poolSize = 20;

        private IYisoObjectPoolService poolService;

        protected override void Awake() {
            base.Awake();
            poolService = YisoServiceProvider.Instance.Get<IYisoObjectPoolService>();
        }

        protected override void Start() {
            base.Start();
            poolService.WarmPool(poolObjectPrefab, poolSize, parent);

            poolService.SpawnObject<Component>(poolObjectPrefab, parent);
        }
    }
}