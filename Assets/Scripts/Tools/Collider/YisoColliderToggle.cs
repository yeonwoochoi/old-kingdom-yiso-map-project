using System.Linq;
using Core.Behaviour;
using UnityEngine;
using UnityEngine.Events;

namespace Tools.Collider {
    [AddComponentMenu("Yiso/Tools/Collider/Collider Toggle")]
    public class YisoColliderToggle : RunIBehaviour {
        public Collider2D targetCollider; // 비활성화할 대상 Collider
        
        private bool isInside = false; // 현재 Collider 내부에 있는지 상태
        private Collider2D collider2D;

        public UnityEvent enterEvent;
        public UnityEvent exitEvent;

        protected override void Start() {
            collider2D = GetComponent<Collider2D>();
        }

        public override void OnUpdate() {
            if (targetCollider == null || collider2D == null) return;
            var targetPosition = targetCollider.transform.position;
            if (collider2D.bounds.Contains(targetPosition)) {
                if (isInside) return;
                targetCollider.enabled = false;
                isInside = true;
                enterEvent?.Invoke();
            }
            else {
                if (!isInside) return;
                targetCollider.enabled = true;
                isInside = false;
                exitEvent?.Invoke();
            }
        }
    }
}