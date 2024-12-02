using System;
using Core.Behaviour;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Tools.Pool {
    [RequireComponent(typeof(Collider2D))]
    [AddComponentMenu("Yiso/Tools/Object Pool/Poolable Object")]
    public class YisoPoolableObject : RunIBehaviour {
        [Title("Events")] public UnityEvent executeOnEnable;
        public UnityEvent executeOnDisable;

        [Title("Poolable Object")] public float lifeTime = 0f;

        public delegate void OnDestroyDelegate();

        public OnDestroyDelegate onDestroy;

        #region Public API

        public virtual void Destroy() {
            onDestroy?.Invoke();
            gameObject.SetActive(false);
        }

        #endregion

        protected override void OnEnable() {
            base.OnEnable();
            if (lifeTime > 0f) {
                Invoke(nameof(Destroy), lifeTime);
            }

            executeOnEnable?.Invoke();
        }

        protected override void OnDisable() {
            base.OnDisable();
            executeOnDisable?.Invoke();
            CancelInvoke();
        }
    }
}