using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.ObjectPool {
    public class ObjectPool<T> {
        private readonly List<ObjectContainer<T>> containers;
        private readonly Dictionary<T, ObjectContainer<T>> lookups;
        private readonly Func<T> factoryFunc;
        private int lastIndex = 0;

        public int Count => containers.Count;
        public int CountUsedItems => lookups.Count;

        public ObjectPool(Func<T> factoryFunc, int initialize) {
            this.factoryFunc = factoryFunc;
            containers = new();
            lookups = new(initialize);
        }

        private void Warm(int capacity) {
            for (var i = 0; i < capacity; i++) {
                CreateContainer();
            }
        }

        private ObjectContainer<T> CreateContainer() {
            var container = new ObjectContainer<T> {
                Item = factoryFunc()
            };
            containers.Add(container);
            return container;
        }

        public T GetItem() {
            ObjectContainer<T> container = null;
            foreach (var _ in containers) {
                lastIndex++;
                if (lastIndex > containers.Count - 1) lastIndex = 0;
                if (containers[lastIndex].Active) continue;
                container = containers[lastIndex];
                break;
            }

            container ??= CreateContainer();
            container.Consume();
            lookups.Add(container.Item, container);
            return container.Item;
        }

        public void ReleaseAllItem() {
            foreach (var (item, container) in lookups) {
                container.Release();
                lookups.Remove(item);
            }
            
            containers.Clear();
        }

        public void ReleaseItem(object item) {
            ReleaseItem((T) item);
        }

        public void ReleaseItem(T item) {
            if (lookups.TryGetValue(item, out var container)) {
                container.Release();
                lookups.Remove(item);
                return;
            }
            
            Debug.LogWarning($"This object pool does not container the item provided: {item}");
        }
    }

    public class ObjectContainer<T> {
        public T Item { get; set; }
        public bool Active { get; private set; }

        public void Consume() {
            Active = true;
        }

        public void Release() {
            Active = false;
        }
    }
}