using System;
using System.Collections.Generic;
using Core.Manager;
using MEC;
using UnityEngine;
using Utils.ObjectId;

namespace Core.Behaviour {
    /// <summary>
    /// Abstract base class for components managed by the RunIUpdateManager. It handles
    /// registration with the update manager on enable and unregister on disable,
    /// and provides methods for component caching to improve performance.
    /// </summary>
    public abstract class RunIBehaviour : MonoBehaviour, RunIUpdateManager.IUpdatable {
        private readonly Dictionary<Type, Component> cachedComponents = new();

        /// <summary>
        /// Invoked when the script instance is being awakened.
        /// </summary>
        protected virtual void Awake() { }

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// Registers the component with the update manager.
        /// </summary>
        protected virtual void OnEnable() {
            RunIUpdateManager.Instance.Register(this);
        }

        /// <summary>
        /// Called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        protected virtual void Start() { }

        /// <summary>
        /// Called every fixed framerate frame. Should be used for physics-related updates.
        /// </summary>
        public virtual void OnFixedUpdate() { }

        /// <summary>
        /// Called once per frame. Used for regular updates such as moving objects or detecting inputs.
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// Called after all Update functions have been called. This is useful to order script execution.
        /// For example, a follow camera should always be implemented in LateUpdate because it tracks objects
        /// that might have moved inside Update.
        /// </summary>
        public virtual void OnLateUpdate() { }

        /// <summary>
        /// Called when the behaviour becomes disabled or inactive.
        /// Unregisters the component from the update manager.
        /// </summary>
        protected virtual void OnDisable() {
            if (RunIUpdateManager.Instance != null)
                RunIUpdateManager.Instance.UnRegister(this);
        }

        /// <summary>
        /// Invoked when the object is being destroyed. Can be used to clean up resources or log messages.
        /// </summary>
        protected virtual void OnDestroy() { }

        /// <summary>
        /// Attempts to retrieve a component of type T from the cache, fetching it from the GameObject if not cached.
        /// </summary>
        /// <typeparam name="T">The type of the component to retrieve.</typeparam>
        /// <param name="component">Out parameter that will receive the component if found.</param>
        /// <returns>True if component is found, false otherwise.</returns>
        private new bool TryGetComponent<T>(out T component) where T : Component {
            var type = typeof(T);
            if (!cachedComponents.TryGetValue(type, out var value)) {
                if (base.TryGetComponent<T>(out component))
                    cachedComponents[type] = component;
            }
            else component = (T)value;

            return component != null;
        }

        /// <summary>
        /// Retrieves a component of type T from the cache or adds it to the GameObject if it doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve or add.</typeparam>
        /// <returns>The component of type T.</returns>
        public T GetOrAddComponent<T>() where T : Component {
            if (TryGetComponent<T>(out var component)) return component;
            component = gameObject.AddComponent<T>();
            cachedComponents[typeof(T)] = component;
            return component;
        }

        /// <summary>
        /// Retrieves a component of type T using the internal caching mechanism or throws if not found.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <returns>The component of type T.</returns>
        public T GetNonNullComponent<T>() where T : Component =>
            TryGetComponent(out T component)
                ? component
                : throw new InvalidOperationException($"{typeof(T).Name} not found in {name}");

        /// <summary>
        /// Creates an instance of a prefab and retrieves a component of type T from it.
        /// Optionally allows specifying the parent for the instantiated object.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve from the instantiated prefab.</typeparam>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="root">The parent transform. If null, the current transform is used.</param>
        /// <returns>The component of type T from the instantiated prefab.</returns>
        protected T CreateObject<T>(GameObject prefab, Transform root = null) where T : Component {
            root ??= transform;
            var obj = Instantiate(prefab, root.position, Quaternion.identity, root);
            var instantiable = obj.GetComponent<IInstantiatable>();
            instantiable?.Init();
            return obj.GetComponent<T>();
        }

        public void StartCoroutine(IEnumerator<float> coroutine, string tag = "") {
            if (tag is "" or "") tag = YisoObjectID.GenerateString();
            Timing.RunCoroutine(coroutine.CancelWith(gameObject), tag);
        }

        public void KillCoroutine(string tag) {
            Timing.KillCoroutines(tag);
        }
    }
}