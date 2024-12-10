using System;
using System.Collections;
using System.Collections.Generic;
using Character.Core;
using Core.Behaviour;
using Core.Domain.Types;
using Core.Logger;
using Core.Service;
using Core.Service.Bounty;
using Core.Service.Log;
using Core.Service.ObjectPool;
using Manager;
using Sirenix.OdinInspector;
using Tools.Event;
using UnityEngine;
using Utils.Beagle;
using Random = UnityEngine.Random;

namespace Controller.Map {
    public struct SpawnObjectEvent {
        public GameObject spawnObj;

        public SpawnObjectEvent(GameObject spawnObj) {
            this.spawnObj = spawnObj;
        }

        private static SpawnObjectEvent e;

        public static void Trigger(GameObject spawnObj) {
            e.spawnObj = spawnObj;
            YisoEventManager.TriggerEvent(e);
        }
    }
    
    [Serializable]
    public class SpawnablePrefab {
        public GameObject prefab;
        public float spawnInterval = 60f;
        [Range(1, 20)] public int minSpawnCount = 1;
        [Range(1, 20)] public int maxSpawnCount = 2;
        public bool spawnOnStart = true;
        public bool spawnOnce = false;
        public bool eventTrigger = false;
        
        [ReadOnly] public bool isPaused = false;
        [ReadOnly] public float lastSpawnTime;
        [HideInInspector] public bool isSpawned = false;
        private float pauseTime;

        public int PoolSize => Mathf.Max(minSpawnCount, maxSpawnCount) * 2;
        public bool CanSpawn {
            get {
                if (prefab == null) return false;
                if (isPaused) return false;
                if (spawnOnStart && !isSpawned) return true;
                if (spawnOnce && isSpawned) return false;
                return Time.time - lastSpawnTime >= spawnInterval;
            }
        }

        public int GetRandomSpawnCount() {
            var min = Mathf.Min(minSpawnCount, maxSpawnCount);
            var max = Mathf.Max(minSpawnCount, maxSpawnCount);
            return Random.Range(min, max + 1);
        }

        public void Pause() {
            if (isPaused) return;
            isPaused = true;
            pauseTime = Time.time;
        }

        public void Resume() {
            if (!isPaused) return;
            lastSpawnTime += (Time.time - pauseTime);
            isPaused = false;
        }
    }

    /// <summary>
    /// Trigger 영역에 랜덤한 위치에 등록된 프리팹 주기적으로 Spawn
    /// </summary>
    [AddComponentMenu("Yiso/Controller/Map/CharacterSpawner")]
    public class YisoCharacterSpawner : RunIBehaviour, IYisoEventListener<YisoInGameEvent> {
        [Title("Settings")]
        public bool autoInitialize = true;
        public Transform parent;
        public bool canSpawnDuringStage = true;

        [Title("Spawn")]
        public bool canSpawn = true;
        public bool onlySpawnWhenPlayerInArea = true;
        public List<SpawnablePrefab> spawnablePrefabs;
        public Collider2D spawnArea;
        public LayerMask collisionMask;
        public int maxCurrentSpawnCount = 15;
        public bool limitTotalSpawnCount = false;
        [ShowIf("limitTotalSpawnCount")] public int maxTotalSpawnCount = 15;
        
        [Title("Debug")]
        public bool testMode = false;

        private bool initialized = false;
        private List<YisoCharacter> spawnObjects;
        private const int MaxAttempts = 20;
        private int spawnCount = 0;

        private Coroutine spawnCoroutine;
        private bool isPlayerInArea;
        private bool isSpawnReady = false;
        
        public IYisoObjectPoolService PoolService => YisoServiceProvider.Instance.Get<IYisoObjectPoolService>();
        public IYisoBountyService BountyService => YisoServiceProvider.Instance.Get<IYisoBountyService>();
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoCharacterSpawner>();

        protected override void Start() {
            base.Start();
            if (autoInitialize) {
                Initialization();
            }
        }

        public override void OnUpdate() {
            if (!initialized) return;
            if (isSpawnReady) {
                foreach (var spawnablePrefab in spawnablePrefabs) {
                    spawnablePrefab.Resume();
                }
            }
            else {
                foreach (var spawnablePrefab in spawnablePrefabs) {
                    spawnablePrefab.Pause();
                }
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            if (autoInitialize) {
                Initialization();
            }
            BountyService.RegisterOnBountyEvent(StopSpawnOnBountyClear);
            this.YisoEventStartListening();
        }

        protected override void OnDisable() {
            if (spawnCoroutine == null) return;
            StopAllCoroutines();
            BountyService.UnregisterOnBountyEvent(StopSpawnOnBountyClear);
            spawnCoroutine = null;
            this.YisoEventStopListening();
        }

        public virtual void Initialization() {
            if (initialized) return;
            if (spawnArea == null) spawnArea = GetComponent<Collider2D>();
            spawnArea.isTrigger = true;
            
            foreach (var spawnablePrefab in spawnablePrefabs) {
                PoolService.WarmPool(spawnablePrefab.prefab, spawnablePrefab.PoolSize, parent.gameObject);
                spawnablePrefab.lastSpawnTime = Time.time;
            }

            spawnObjects = new List<YisoCharacter>();
            spawnCoroutine ??= StartCoroutine(SpawnCoroutine());
            if (testMode) isSpawnReady = true;
            else if (!canSpawnDuringStage) isSpawnReady = true;
            initialized = true;
        }
        
        private IEnumerator SpawnCoroutine() {
            while (true) {
                CleanInactiveObjects();
                foreach (var spawnablePrefab in spawnablePrefabs) {
                    if (!CanSpawn(spawnablePrefab)) continue;
                    var count = spawnablePrefab.GetRandomSpawnCount();
                    for (var i = 0; i < count; i++) {
                        if (TryGetValidRandomPointInBounds(spawnArea, out var spawnPosition)) {
                            SpawnCharacter(spawnablePrefab, spawnPosition);
                            yield return null;
                        }
                        else {
                            LogService.Warn($"[YisoCharacterSpawner] Spawn failed after {MaxAttempts} attempts. Unable to find a valid position.");
                        }
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }

        private void SpawnCharacter(SpawnablePrefab spawnablePrefab, Vector2 position) {
            var spawnCharacter = PoolService.SpawnObject<YisoCharacter>(spawnablePrefab.prefab);
            spawnCharacter.activateOnInstantiate = true;
            spawnCharacter.playSpawnAnimation = true;
            spawnCharacter.RespawnAt(position, YisoCharacter.FacingDirections.South, true);
            spawnablePrefab.isSpawned = true;
            spawnablePrefab.lastSpawnTime = Time.time;
            if (spawnablePrefab.eventTrigger) SpawnObjectEvent.Trigger(spawnCharacter.gameObject);
            if (!spawnObjects.Contains(spawnCharacter)) spawnObjects.Add(spawnCharacter);
            spawnCount++;
        }

        private void CleanInactiveObjects() {
            spawnObjects.RemoveAll(character => character == null || !character.gameObject.activeInHierarchy || character.conditionState.CurrentState == YisoCharacterStates.CharacterConditions.Dead);
        }
        
        private bool CanSpawn(SpawnablePrefab spawnablePrefab) {
            if (!canSpawn) return false;
            if (!testMode && canSpawnDuringStage && !isSpawnReady) return false;
            if (onlySpawnWhenPlayerInArea && !isPlayerInArea) return false;
            if (spawnablePrefab.spawnOnce) {
                return spawnablePrefab.CanSpawn;
            }

            var exceedsTotalSpawnLimit = limitTotalSpawnCount && spawnCount >= maxTotalSpawnCount;
            var exceedsCurrentSpawnLimit = spawnObjects.Count >= maxCurrentSpawnCount;

            return spawnablePrefab.CanSpawn && !exceedsTotalSpawnLimit && !exceedsCurrentSpawnLimit;
        }

        private bool TryGetValidRandomPointInBounds(Collider2D collider2D, out Vector2 randomPoint) {
            var success = false;
            var bounds = collider2D.bounds;

            switch (collider2D) {
                case PolygonCollider2D polygonCollider2D:
                    bounds = polygonCollider2D.bounds;
                    randomPoint = bounds.center;

                    for (var i = 0; i < MaxAttempts; i++) {
                        // Generate a random point within the bounds
                        var x = Random.Range(bounds.min.x, bounds.max.x);
                        var y = Random.Range(bounds.min.y, bounds.max.y);
                        randomPoint = new Vector2(x, y);

                        // Check if the point is inside the PolygonCollider2D
                        if (!polygonCollider2D.OverlapPoint(randomPoint)) continue;
                        // Ensure there's no collision with objects in the collisionMask
                        if (Physics2D.OverlapCircle(randomPoint, 0.5f, collisionMask)) continue;
                        success = true;
                        break;
                    }

                    return success;
                case BoxCollider2D boxCollider2D:
                    bounds = boxCollider2D.bounds;
                    randomPoint = bounds.center;

                    for (var i = 0; i < MaxAttempts; i++) {
                        var x = Random.Range(bounds.min.x, bounds.max.x);
                        var y = Random.Range(bounds.min.y, bounds.max.y);
                        randomPoint = new Vector2(x, y);

                        if (Physics2D.OverlapCircle(randomPoint, 1f, collisionMask)) continue;
                        success = true;
                        break;
                    }
            
                    return success;
                case CircleCollider2D circleCollider2D:
                    randomPoint = circleCollider2D.bounds.center;
                    Vector2 circleCenter = circleCollider2D.transform.position + (Vector3)circleCollider2D.offset;
                    var radius = circleCollider2D.radius * circleCollider2D.transform.lossyScale.x; // Adjust for scale

                    for (var i = 0; i < MaxAttempts; i++) {
                        var angle = Random.Range(0f, Mathf.PI * 2); // Random angle in radians
                        var distance = Mathf.Sqrt(Random.Range(0f, 1f)) * radius; // Random distance with uniform distribution
                        var x = circleCenter.x + distance * Mathf.Cos(angle);
                        var y = circleCenter.y + distance * Mathf.Sin(angle);
                        randomPoint = new Vector2(x, y);
                        
                        if (Physics2D.OverlapCircle(randomPoint, 1f, collisionMask)) continue;
                        success = true;
                        break;
                    }

                    return success;
                default:
                    randomPoint = collider2D.bounds.center;
                    return false;
            }
        }

        protected virtual void StopSpawnOnBountyClear(YisoBountyEventArgs eventArgs) {
            if (!GameManager.HasInstance || GameManager.Instance.CurrentGameMode != GameManager.GameMode.Bounty) return;
            switch (eventArgs) {
                case YisoBountyStatusChangeEventArgs statusChangeEventArgs:
                    if (statusChangeEventArgs.To == YisoBountyStatus.COMPLETE) {
                        canSpawn = false;
                    }
                    break;
            }
        }
        
        protected virtual void ReleasePool() {
            if (!initialized) return;
            if (parent != null) parent.gameObject.ReleaseAllChildObjects();
            initialized = false;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            ReleasePool();
        }

        protected void OnTriggerEnter2D(Collider2D other) {
            if (!LayerManager.CheckIfInLayer(other.gameObject, LayerManager.PlayerLayerMask)) return;
            isPlayerInArea = true;
        }

        protected void OnTriggerExit2D(Collider2D other) {
            if (!LayerManager.CheckIfInLayer(other.gameObject, LayerManager.PlayerLayerMask)) return;
            isPlayerInArea = false;
        }

        public void OnEvent(YisoInGameEvent e) {
            if (!canSpawnDuringStage) return;
            switch (e.eventType) {
                case YisoInGameEventTypes.StageClear:
                case YisoInGameEventTypes.Pause:
                case YisoInGameEventTypes.PlayerDeath:
                    isSpawnReady = false;
                    break;
                case YisoInGameEventTypes.StageStart:
                case YisoInGameEventTypes.UnPause:
                case YisoInGameEventTypes.RespawnComplete:
                    isSpawnReady = true;
                    break;
                
            }
        }
    }
}