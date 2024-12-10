using System.Collections.Generic;
using Core.Behaviour;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Manager;
using Sirenix.OdinInspector;
using Tools.Environment;
using Tools.Event;
using UnityEngine;

namespace Controller.Map {
    public struct YisoNavigationZoneEnterEvent {
        public YisoNavigationZoneController zone;

        public YisoNavigationZoneEnterEvent(YisoNavigationZoneController zone) {
            this.zone = zone;
        }

        static YisoNavigationZoneEnterEvent e;

        public static void Trigger(YisoNavigationZoneController zone) {
            e.zone = zone;
            YisoEventManager.TriggerEvent(e);
        }
    }

    [AddComponentMenu("Yiso/Controller/Map/NavigationZoneController")]
    public class YisoNavigationZoneController : RunIBehaviour {
        [Title("Settings")] public LayerMask targetLayer = LayerManager.PlayerLayerMask;

        [Title("Portals")] public List<YisoPortal> portals;

        [Title("Path Finding")] public YisoMapController.MapBounds mapBounds;
        
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoNavigationZoneController>();

        public string ZoneName { get; set; } = "Zone";
        protected Collider2D collider2D;

        protected override void Awake() {
            if (portals != null && portals.Count > 0) {
                foreach (var portal in portals) {
                    portal.ParentZone = this;
                }
            }

            collider2D = GetComponent<Collider2D>();
            if (collider2D == null) {
                LogService.Warn("[Navigation Zone] : need collider 2d");
            }
            else {
                collider2D.isTrigger = true;
            }
        }

        public bool Contains(Vector3 position) {
            return collider2D.bounds.Contains(position);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other) {
            if (!LayerManager.CheckIfInLayer(other.gameObject, targetLayer)) return;
            YisoNavigationZoneEnterEvent.Trigger(this);
            mapBounds?.Scan();
        }
    }
}