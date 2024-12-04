using System;
using Controller.Effect;
using Core.Service;
using Core.Service.ObjectPool;
using Core.Service.Scene;
using Sirenix.OdinInspector;
using Tools.Event;
using UnityEngine;
using Utils;
using Utils.Beagle;

namespace Manager_Temp_.Modules {
    public struct DamageEffectEvent {
        public Vector3 position;
        public float damage;
        public bool isCritical;

        private static DamageEffectEvent e;

        public static void Trigger(Vector3 position, float damage, bool isCritical) {
            e.position = position;
            e.damage = damage;
            e.isCritical = isCritical;
            YisoEventManager.TriggerEvent(e);
        }
    }

    public class YisoGameEffectModule : YisoGameBaseModule, IYisoEventListener<DamageEffectEvent>,
        IYisoEventListener<YisoInGameEvent> {
        private readonly Settings settings;
        private bool poolInitialized = false;
        private GameObject damageEffectHolder;

        public IYisoObjectPoolService PoolService => YisoServiceProvider.Instance.Get<IYisoObjectPoolService>();
        public IYisoSceneService SceneService => YisoServiceProvider.Instance.Get<IYisoSceneService>();

        public YisoGameEffectModule(GameManager manager, Settings settings) : base(manager) {
            this.settings = settings;
            InitializePool();
        }

        public override void OnEnabled() {
            base.OnEnabled();
            this.YisoEventStartListening<DamageEffectEvent>();
            this.YisoEventStartListening<YisoInGameEvent>();
        }

        public override void OnDisabled() {
            base.OnDisabled();
            this.YisoEventStopListening<DamageEffectEvent>();
            this.YisoEventStopListening<YisoInGameEvent>();
        }

        public void OnEvent(DamageEffectEvent e) {
            ShowDamage(e.position, e.damage, e.isCritical);
        }

        public void OnEvent(YisoInGameEvent e) {
            switch (e.eventType) {
                case YisoInGameEventTypes.MoveNextStage:
                    ReleasePool();
                    break;
            }
        }

        private void InitializePool() {
            if (poolInitialized) return;
            var parentObjectName = $"[Pooler] Damage Effect {settings.damageEffectPrefab.name}";
            damageEffectHolder = GameObject.Find(parentObjectName);
            if (damageEffectHolder == null) damageEffectHolder = new GameObject(parentObjectName);
            // SceneManager.MoveGameObjectToScene(damageEffectHolder, SceneService.GetCurrentScene().ToScene());
            PoolService.WarmPool(settings.damageEffectPrefab, 30, damageEffectHolder);
            poolInitialized = true;
        }

        private void ReleasePool() {
            if (!poolInitialized) return;
            // PoolService.ReleaseAllObject(settings.damageEffectPrefab);
            if (damageEffectHolder != null) damageEffectHolder.ReleaseAllChildObjects();
            poolInitialized = false;
        }

        private void ShowDamage(Vector3 position, double damage, bool critical) {
            var newPos = position.NextVector3(to: 0.5f);
            var effect =
                PoolService.SpawnObject<YisoDamageEffectController>(settings.damageEffectPrefab, damageEffectHolder);
            var obj = effect.gameObject;

            // Set Layer
            var meshRenderer = obj.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerID = LayerManager.UISortingLayer;
            meshRenderer.sortingOrder = 3;

            obj.transform.position = newPos;
            effect.Setup(damage, critical, () => { PoolService.ReleaseObject(obj); });
        }

        [Serializable]
        public class Settings {
            [Title("Damage Effect")] public GameObject damageEffectPrefab;
        }
    }
}