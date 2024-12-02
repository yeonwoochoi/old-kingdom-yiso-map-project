using System.Collections.Generic;
using Core.Behaviour;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Tools.Inputs {
    public class YisoMouseClickEventDispatcher : RunIBehaviour, IPointerClickHandler {
        private readonly List<UnityAction> clickCallbacks = new();
        private Collider2D collider2D;

        protected override void Start() {
            base.Start();
            collider2D = gameObject.GetComponent<Collider2D>();
            collider2D.isTrigger = true;
        }

        public void RegisterCallback(UnityAction callback) {
            if (callback != null && !clickCallbacks.Contains(callback)) {
                clickCallbacks.Add(callback);
            }
        }

        public void UnregisterCallback(UnityAction callback) {
            if (callback != null && clickCallbacks.Contains(callback)) {
                clickCallbacks.Remove(callback);
            }
        }

        private void DispatchClickEvent() {
            foreach (var callback in clickCallbacks) {
                callback?.Invoke();
            }
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            DispatchClickEvent();
        }
    }
}