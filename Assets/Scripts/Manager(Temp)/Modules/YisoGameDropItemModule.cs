using System;
using Core.Service;
using Core.Service.ObjectPool;
using Core.Service.Scene;
using Items.Pickable;
using Sirenix.OdinInspector;
using Tools.Event;
using UnityEngine;
using Utils.Beagle;

namespace Manager_Temp_.Modules {
    public class YisoGameDropItemModule : YisoGameBaseModule, IYisoEventListener<YisoInGameEvent> {
        private readonly Settings settings;
        private bool poolInitialized = false;
        private GameObject dropMoneyHolder;
        private GameObject dropItemHolder;

        public IYisoObjectPoolService PoolService => YisoServiceProvider.Instance.Get<IYisoObjectPoolService>();
        public IYisoSceneService SceneService => YisoServiceProvider.Instance.Get<IYisoSceneService>();
        public GameObject DropItemPrefab => settings.dropItemPrefab;
        public GameObject DropMoneyPrefab => settings.dropMoneyPrefab;

        public YisoGameDropItemModule(GameManager manager, Settings settings) : base(manager) {
            this.settings = settings;
            InitializePool();
        }

        public GameObject SpawnMoneyObject(out YisoCoinPickableObject component) {
            component = PoolService.SpawnObject<YisoCoinPickableObject>(settings.dropMoneyPrefab);
            return component.gameObject;
        }

        public GameObject SpawnItemObject(out YisoItemPickableObject component) {
            component = PoolService.SpawnObject<YisoItemPickableObject>(settings.dropItemPrefab);
            return component.gameObject;
        }

        public void OnEvent(YisoInGameEvent e) {
            switch (e.eventType) {
                case YisoInGameEventTypes.MoveNextStage:
                    ReleasePool();
                    break;
            }
        }

        public override void OnEnabled() {
            base.OnEnabled();
            this.YisoEventStartListening();
        }

        public override void OnDisabled() {
            base.OnDisabled();
            this.YisoEventStopListening();
        }

        private void InitializePool() {
            if (poolInitialized) return;
            var parentMoneyObjectName = $"[Pooler] Drop Money {settings.dropMoneyPrefab.name}";
            var parentItemObjectName = $"[Pooler] Drop Item {settings.dropItemPrefab.name}";
            dropMoneyHolder = GameObject.Find(parentMoneyObjectName);
            dropItemHolder = GameObject.Find(parentItemObjectName);
            if (dropMoneyHolder == null) dropMoneyHolder = new GameObject(parentMoneyObjectName);
            if (dropItemHolder == null) dropItemHolder = new GameObject(parentItemObjectName);
            // SceneManager.MoveGameObjectToScene(dropMoneyHolder, SceneService.GetCurrentScene().ToScene());
            // SceneManager.MoveGameObjectToScene(dropItemHolder, SceneService.GetCurrentScene().ToScene());
            PoolService.WarmPool(settings.dropMoneyPrefab, 30, dropMoneyHolder);
            PoolService.WarmPool(settings.dropItemPrefab, 30, dropItemHolder);
            poolInitialized = true;
        }

        private void ReleasePool() {
            if (!poolInitialized) return;
            if (dropMoneyHolder != null) dropMoneyHolder.ReleaseAllChildObjects();
            if (dropItemHolder != null) dropItemHolder.ReleaseAllChildObjects();
            // PoolService.ReleaseAllObject(settings.dropItemPrefab);
            // PoolService.ReleaseAllObject(settings.dropMoneyPrefab);
            poolInitialized = false;
        }


        [Serializable]
        public class Settings {
            [Title("Item")] public GameObject dropItemPrefab;
            [Title("Money")] public GameObject dropMoneyPrefab;
        }
    }
}