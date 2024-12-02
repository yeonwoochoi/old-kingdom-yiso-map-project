using Core.Behaviour;
using UnityEngine;

namespace Items {
    public class YisoLootDetector : RunIBehaviour {
        public delegate void OnTriggerDelegate(Collider2D collider);

        public OnTriggerDelegate onTriggerEnter2D;
        public OnTriggerDelegate onTriggerExit2D;

        protected bool initialized = false;
        protected BoxCollider2D collider2D;

        public void Initialization(OnTriggerDelegate onTriggerEnter, OnTriggerDelegate onTriggerExit) {
            onTriggerEnter2D = onTriggerEnter;
            onTriggerExit2D = onTriggerExit;

            collider2D = gameObject.GetComponent<BoxCollider2D>();
            if (collider2D == null) {
                collider2D = gameObject.AddComponent<BoxCollider2D>();
                collider2D.isTrigger = true;
                collider2D.offset = new Vector2(0f, 0.25f);
                collider2D.size = new Vector2(0.85f, 1.5f);
            }

            initialized = true;
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider) {
            if (!initialized) return;
            onTriggerEnter2D?.Invoke(collider);
        }

        protected virtual void OnTriggerExit2D(Collider2D collider) {
            if (!initialized) return;
            onTriggerExit2D?.Invoke(collider);
        }
    }
}