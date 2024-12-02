using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Core.Service {
    public class YisoServiceProvider {
        private static YisoServiceProvider instance = null;

        public bool Initialized { get; set; } = false;

        private YisoServiceProvider() { }

        private readonly Dictionary<string, IYisoService> services = new();


        public static YisoServiceProvider Instance {
            get {
                if (instance != null) return instance;
                instance = new YisoServiceProvider();
                return instance;
            }
        }

        public T Get<T>() where T : IYisoService {
            var key = typeof(T).Name;
            if (!services.TryGetValue(key, out var service))
                throw new InvalidOperationException($"{key} not registered with {GetType().Name}");
            return (T) service;
        }

        public void Register<T>(Type keyType, T service) where T : IYisoService {
            var key = keyType.Name;
            if (services.ContainsKey(key)) return;
            services.TryAdd(key, service);
        }

        public void Unregister<T>() where T : IYisoService {
            var key = typeof(T).Name;
            services.Remove(key);
        }

        public void OnDestroy() {
            foreach (var service in services.Values)
                service.OnDestroy();
        }

        public bool IsReady() => services.Values.All(service => service.IsReady());
    }
}