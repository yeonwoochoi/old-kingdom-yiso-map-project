using Camera;
using Core.Behaviour;
using Manager_Temp_;
using UnityEngine;

namespace Controller.Map {
    [AddComponentMenu("Yiso/Controller/Map/CameraBoundarySetter")]
    public class YisoCameraBoundarySetter : RunIBehaviour {
        public LayerMask targetMask = LayerManager.PlayerLayerMask;

        protected PolygonCollider2D polygonCollider2D;
        public bool Active { get; protected set; } = false;

        protected override void Start() {
            polygonCollider2D = GetComponent<PolygonCollider2D>();
        }

        protected virtual void OnTriggerEnter2D(Collider2D other) {
            if (polygonCollider2D == null || Active) return;
            if (!LayerManager.CheckIfInLayer(other.gameObject, targetMask)) return;
            Active = true;
            YisoCameraEvent.Trigger(YisoCameraEventTypes.SetConfiner, polygonCollider2D);
        }

        protected virtual void OnTriggerExit2D(Collider2D other) {
            if (polygonCollider2D == null || !Active) return;
            if (!LayerManager.CheckIfInLayer(other.gameObject, targetMask)) return;
            Active = false;
        }
    }
}