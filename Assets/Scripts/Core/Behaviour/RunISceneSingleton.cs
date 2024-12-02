using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Behaviour {
    public abstract class RunISceneSingleton<T> : RunISingleton<T> where T : MonoBehaviour {
        private bool initialized = false;

        protected virtual void Start() {
            initialized = true;
        }

        protected override void OnEnable() {
            base.OnEnable();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDisable() {
            base.OnDisable();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (!initialized) return;
            Destroy(instance);
            Destroy(gameObject);
        }
    }
}