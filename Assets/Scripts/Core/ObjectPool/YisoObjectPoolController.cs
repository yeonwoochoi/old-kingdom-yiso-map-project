using System;
using System.Collections.Generic;
using Core.Behaviour;
using UnityEngine;

namespace Core.ObjectPool {
    public class YisoObjectPoolController : RunIBehaviour {
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> prefabLookup = new();
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> instanceLookup = new();
        
        public void WarmPool(GameObject prefab, int size, GameObject root = null) {
            if (prefabLookup.ContainsKey(prefab)) {
                return;
            }
            
            prefabLookup[prefab] = new ObjectPool<GameObject>(() => InstantiatePrefab(prefab, root), size);
        }

        public T SpawnObject<T>(GameObject prefab, GameObject root = null) where T : Component {
            if (!prefabLookup.ContainsKey(prefab)) {
                WarmPool(prefab, 1, root);
            }

            var pool = prefabLookup[prefab];
            var clone = pool.GetItem();
            clone.SetActive(true);
            instanceLookup.Add(clone, pool);
            return clone.GetComponent<T>();
        }

        public void ReleaseObject(GameObject clone) {
            clone.SetActive(false);
            if (instanceLookup.ContainsKey(clone)) {
                instanceLookup[clone].ReleaseItem(clone);
                instanceLookup.Remove(clone);
                return;
            }
            
            Debug.LogWarning($"No pool contains the object: {clone.name}");
        }

        public void ReleaseAllObject(GameObject prefab) {
            if (!instanceLookup.TryGetValue(prefab, out var pool)) return;
            pool.ReleaseAllItem();
        }
        
        public void ReleaseAllObject() {
            foreach (var (clone, item) in instanceLookup) {
                item.ReleaseItem(clone);
            }
            
            instanceLookup.Clear();

            foreach (var (prefab, item) in prefabLookup) {
                item.ReleaseAllItem();
            }
        }
        
        private GameObject InstantiatePrefab(GameObject prefab, GameObject root = null) {
            var go = root == null 
                ? Instantiate(prefab, prefab.transform.position, Quaternion.identity) 
                : Instantiate(prefab, root.transform.position, Quaternion.identity, root.transform);
            var initialized = go.GetComponent<IInstantiatable>();
            initialized?.Init();
            return go;
        }
    }
}