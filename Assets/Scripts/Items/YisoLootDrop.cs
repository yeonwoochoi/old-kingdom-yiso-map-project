using System.Collections;
using Character.Health;
using Core.Behaviour;
using Core.Domain.Drop;
using Core.Domain.Item;
using Core.Service;
using Core.Service.Temp;
using Items.Pickable;
using Manager_Temp_;
using Manager_Temp_.Modules;
using Sirenix.OdinInspector;
using Spawn;
using Tools.Feedback.Core;
using UnityEngine;

namespace Items {
    public abstract class YisoLootDrop : RunIBehaviour {
        [Title("Conditions")] public bool spawnLootOnDeath = true;
        public bool spawnLootOnDamage = false; // 픽파켓 같은 스킬에 써먹을 수 있음

        [Title("Pooling")] public bool poolLoot = true;

        [Title("Spawn")] public bool canSpawn = true; // false면 Spawn 안함
        public float delay = 0f;
        public YisoObjectSpawnInAreaProperties spawnInAreaProperties;
        public bool limitedLootQuantity = true;
        [ShowIf("limitedLootQuantity")] public int maximumQuantityLimit = 30;
        [ReadOnly] public int remainingQuantity = 30;

        [Title("Collisions")] public bool avoidObstacles = false;
        public float avoidRadius = 0.25f;
        public int maxAvoidAttempts = 20;

        [Title("Animations")] public bool playAnimationOnHit = false;
        public bool playAnimationOnDeath = false;

        [Title("Feedback")] public YisoFeedBacks lootFeedback;

        protected readonly LayerMask avoidObstacleLayerMask =
            LayerManager.ObstaclesLayerMask | LayerManager.MapLayerMask;

        protected bool initialized = false;
        protected YisoHealth health;
        protected YisoGameDropItemModule dropModule;

        protected int hitAnimationParameter;
        protected int deathAnimationParameter;

        protected override void Awake() {
            health = gameObject.GetComponentInParent<YisoHealth>();
            remainingQuantity = maximumQuantityLimit;
        }

        protected override void Start() {
            Initialization();
        }

        protected virtual void Initialization() {
            dropModule = YisoServiceProvider.Instance.Get<IYisoTempService>().GetGameManager().GameModules
                .DropItemModule;
            initialized = true;
        }

        /// <summary>
        /// Looting Items
        /// </summary>
        public virtual void SpawnLoot() {
            if (!canSpawn || !initialized) return;
            StartCoroutine(SpawnLootCo());
        }

        /// <summary>
        /// Looting Items Coroutine
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator SpawnLootCo() {
            yield return new WaitForSeconds(delay);
            var randomQuantity = GetDropItemQuantity();
            for (var i = 0; i < randomQuantity; i++) {
                SpawnOneLoot();
            }

            lootFeedback?.PlayFeedbacks();
        }

        /// <summary>
        /// Spawn One Loot
        /// SpawnLootCo에서 호출
        /// </summary>
        protected virtual void SpawnOneLoot() {
        }

        protected virtual void SpawnOneItem(YisoDropItem dropItem) {
            SpawnOneItem(dropItem.Item);
        }

        protected virtual void SpawnOneItem(YisoItem dropItem) {
            YisoItemPickableObject itemPickableObject = null;
            GameObject itemObj = null;
            if (poolLoot) {
                itemObj = dropModule.SpawnItemObject(out itemPickableObject);
                itemPickableObject.IsPooled = true;
            }
            else {
                itemObj = Instantiate(dropModule.DropItemPrefab);
                itemPickableObject = itemObj.GetComponent<YisoItemPickableObject>();
                itemPickableObject.IsPooled = false;
            }

            itemPickableObject.SetItem(dropItem);
            SetSpawnedObjectPosition(itemObj);
        }

        protected virtual void SpawnMoney(double money) {
            YisoCoinPickableObject moneyPickableObject = null;
            GameObject moneyObj = null;
            if (poolLoot) {
                moneyObj = dropModule.SpawnMoneyObject(out moneyPickableObject);
                moneyPickableObject.IsPooled = true;
            }
            else {
                moneyObj = Instantiate(dropModule.DropMoneyPrefab);
                moneyPickableObject = moneyObj.GetComponent<YisoCoinPickableObject>();
                moneyPickableObject.IsPooled = false;
            }

            moneyPickableObject.SetMoney(money);
            SetSpawnedObjectPosition(moneyObj);
        }

        /// <summary>
        /// SpawnInAreaProperty에 맞게 Spawn된 object를 적당한 위치에 위치시킴
        /// </summary>
        /// <param name="spawnedObject"></param>
        protected virtual void SetSpawnedObjectPosition(GameObject spawnedObject) {
            if (avoidObstacles) {
                var placementOK = false;
                var amountOfAttempts = 0;
                while (!placementOK && amountOfAttempts < maxAvoidAttempts) {
                    YisoObjectSpawnInArea.ApplySpawnInAreaProperties(spawnedObject, spawnInAreaProperties,
                        transform.position);
                    var raycastOrigin = spawnedObject.transform.position;
                    // 오른쪽에서 왼쪽방향으로 스캔
                    var raycastHit2D = Physics2D.BoxCast(raycastOrigin + Vector3.right * avoidRadius,
                        avoidRadius * Vector2.one, 0f, Vector2.left, avoidRadius, avoidObstacleLayerMask);
                    if (raycastHit2D.collider == null) {
                        placementOK = true;
                    }
                    else {
                        amountOfAttempts++;
                    }
                }
            }
            else {
                YisoObjectSpawnInArea.ApplySpawnInAreaProperties(spawnedObject, spawnInAreaProperties,
                    transform.position);
            }

            if (spawnedObject != null) spawnedObject.SetActive(true);
            if (limitedLootQuantity) remainingQuantity--;
        }

        /// <summary>
        /// 몇개 아이템 떨굴건지
        /// Simple Loot, Multiple Loot에서만 상속받아서 사용
        /// </summary>
        /// <returns></returns>
        protected virtual int GetDropItemQuantity() {
            return 1;
        }

        protected virtual void OnHit(GameObject attacker) {
            if (spawnLootOnDamage) SpawnLoot();
        }

        protected virtual void OnDeath() {
            if (spawnLootOnDeath) SpawnLoot();
        }

        protected override void OnEnable() {
            base.OnEnable();
            if (health != null) {
                health.onHit += OnHit;
                health.onDeath += OnDeath;
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (health != null) {
                health.onHit -= OnHit;
                health.onDeath -= OnDeath;
            }
        }

        [Button]
        public void Loot() {
            SpawnLoot();
        }
    }
}