using System.Collections.Generic;
using Character.Health;
using Character.Health.Damage;
using Core.Behaviour;
using UnityEditor;
using UnityEngine;
using Utils.Beagle;

namespace Tools.Collider {
    /// <summary>
    /// 해당 범위에 있는 Target을 Detecting해주는 Class
    /// Attack할때 Enable -> Disable 하면서 사용하면 됨.
    /// 쏠 때만 enable해서 Target 값 받고 disable시키면서 targetObject 초기화시킴
    /// </summary>
    [AddComponentMenu("Yiso/Tools/Collider/Detect Area")]
    public class YisoDetectArea : RunIBehaviour {
        public LayerMask TargetLayerMask { get; set; }
        public GameObject TargetObject { get; private set; }

        protected bool initialized = false;
        protected YisoDamageOnTouch.TriggerAndCollisionMask triggerFilter = YisoDamageOnTouch.AllowedTriggerCallbacks;
        protected GameObject owner;
        protected List<GameObject> ignoredGameObjects;
        protected YisoHealth colliderHealth;
        protected ArcCollider2D arcCollider2D;
        protected PolygonCollider2D polygonCollider2D;
        protected Color gizmoColor;

        #region Initialization

        protected override void Awake() {
            initialized = false;
            arcCollider2D = GetComponent<ArcCollider2D>();
            polygonCollider2D = GetComponent<PolygonCollider2D>();
            if (polygonCollider2D != null) polygonCollider2D.isTrigger = true;
            gizmoColor = Color.red;
            gizmoColor.a = 0.15f;
        }

        public void SetOwner(GameObject owner) {
            this.owner = owner;
        }

        public void SetRange(float range) {
            if (polygonCollider2D == null) {
                polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
                polygonCollider2D.isTrigger = true;
            }

            if (arcCollider2D == null) {
                arcCollider2D = gameObject.AddComponent<ArcCollider2D>();
                arcCollider2D.TotalAngle = 90f;
                arcCollider2D.OffsetRotation = 315f;
                arcCollider2D.PizzaSlice = true;
                arcCollider2D.Thickness = 1f;
                arcCollider2D.Smoothness = 10;
            }

            arcCollider2D.Radius = range;
            initialized = true;
        }

        #endregion

        #region Core (Trigger)

        public virtual void OnTriggerEnter2D(Collider2D collider) {
            if (!CheckDetectingAvailability(collider.gameObject)) return;
            colliderHealth = collider.gameObject.YisoGetComponentNoAlloc<YisoHealth>();
            if (colliderHealth == null) return;
            if (colliderHealth.IsAlive) {
                Detect(collider.gameObject);
            }
        }

        public virtual void OnTriggerStay2D(Collider2D collider) {
            if (!CheckDetectingAvailability(collider.gameObject)) return;
            colliderHealth = collider.gameObject.YisoGetComponentNoAlloc<YisoHealth>();
            if (colliderHealth == null) return;
            if (colliderHealth.IsAlive) {
                Detect(collider.gameObject);
            }
        }

        protected virtual bool CheckDetectingAvailability(GameObject collider) {
            if (!initialized) return false;
            if (!isActiveAndEnabled) return false;
            if ((triggerFilter & YisoDamageOnTouch.TriggerAndCollisionMask.OnTriggerStay2D) == 0) return false;
            if (ignoredGameObjects.Contains(collider)) return false;
            if (!YisoLayerUtils.CheckLayerInLayerMask(collider.layer, TargetLayerMask)) return false;
            if (Time.time == 0f) return false;
            return true;
        }

        protected virtual void Detect(GameObject detected) {
            TargetObject = detected;
        }

        protected virtual void Clear() {
            TargetObject = null;
        }

        #endregion

        #region Public API

        public void Activate(bool activate = true) {
            if (!activate) Clear();
            polygonCollider2D.enabled = activate;
        }

        public virtual void StartIgnoreObject(GameObject newIgnoredGameObject) {
            ignoredGameObjects ??= new List<GameObject>();
            ignoredGameObjects.Add(newIgnoredGameObject);
        }

        public virtual void StopIgnoreObject(GameObject ignoredGameObject) {
            ignoredGameObjects?.Remove(ignoredGameObject);
        }

        public virtual void ClearIgnoreList() {
            ignoredGameObjects ??= new List<GameObject>();
            ignoredGameObjects.Clear();
        }

        #endregion

        #region Gizmos

#if UNITY_EDITOR
        public void OnDrawGizmos() {
            if (!isActiveAndEnabled) return;
            Handles.color = gizmoColor;

            if (polygonCollider2D != null) {
                if (polygonCollider2D.enabled) {
                    YisoDebugUtils.DrawGizmoArc(owner.transform, 90f, arcCollider2D.Radius, false);
                }
                else {
                    YisoDebugUtils.DrawGizmoArc(owner.transform, 90f, arcCollider2D.Radius, true);
                }
            }
        }
#endif

        #endregion
    }
}