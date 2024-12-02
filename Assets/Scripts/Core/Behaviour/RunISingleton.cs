using Core.Manager;
using UnityEngine;

namespace Core.Behaviour {
    public abstract class RunISingleton<T> : MonoBehaviour, RunIUpdateManager.IUpdatable where T : MonoBehaviour {
        protected static T instance = null;
        public static bool HasInstance => instance != null;

        public static T Instance {
            get {
                if (instance != null) {
                    return instance;
                }
                
                instance = FindObjectOfType(typeof(T)) as T;
                if (instance != null) return instance;
                
                instance = new GameObject(typeof(T).ToString(), typeof(T)).AddComponent<T>();
                DontDestroyOnLoad(instance);
                return instance;
            }
        }

        protected virtual void Awake() {
            if (instance != null && instance != this) {
                Destroy(gameObject);
                return;
            }
            instance = this as T;
            DontDestroyOnLoad(instance);
        }

        protected virtual void OnEnable() {
            RunIUpdateManager.Instance.Register(this);
        }

        protected virtual void OnDisable() {
            if (RunIUpdateManager.Instance != null)
                RunIUpdateManager.Instance.UnRegister(this);
        }

        public virtual void OnUpdate() { }

        public virtual void OnFixedUpdate() { }

        public virtual void OnLateUpdate() { }
        
        protected T CreateObject<T>(GameObject prefab, Transform root = null) where T : Component {
            root ??= transform;
            var obj = Instantiate(prefab, root.position, Quaternion.identity, root);
            var instantiable = obj.GetComponent<IInstantiatable>();
            instantiable?.Init();
            return obj.GetComponent<T>();
        }
    }
}