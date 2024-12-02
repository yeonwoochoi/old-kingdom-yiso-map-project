using System.Collections.Generic;
using Core.Service;
using Core.Service.ObjectPool;
using UnityEngine;

namespace Utils.Beagle {
    public static class YisoGameObjectUtils {
        private static List<Component> componentCache = new List<Component>();

        public static T Instantiate<T>(T original) where T : Object {
            return Object.Instantiate(original);
        }

        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object {
            return Object.Instantiate(original, position, rotation);
        }

        public static T Instantiate<T>(T original, Transform parent) where T : Object {
            return Object.Instantiate(original, parent);
        }

        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform parent)
            where T : Object {
            return Object.Instantiate(original, position, rotation, parent);
        }

        public static T YisoGetComponentNoAlloc<T>(this GameObject @this) where T : Component {
            @this.GetComponents(typeof(T), componentCache);
            var component = componentCache.Count > 0 ? componentCache[0] : null;
            componentCache.Clear();
            return component as T;
        }

        /// <summary>
        /// 부모, 자식 오브젝트까지 싹다 뒤지는거. 없으면 Add
        /// </summary>
        /// <param name="this"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T YisoGetComponentAroundOrAdd<T>(this GameObject @this) where T : Component {
            var component = @this.GetComponentInChildren<T>(true);
            if (component == null) component = @this.GetComponentInParent<T>();
            if (component == null) component = @this.AddComponent<T>();
            return component;
        }

        public static void ReleaseAllChildObjects(this GameObject @this) {
            if (@this == null) return;
            for (var i = 0; i < @this.transform.childCount; i++) {
                var child = @this.transform.GetChild(i);
                if (child.gameObject.activeInHierarchy) {
                    YisoServiceProvider.Instance.Get<IYisoObjectPoolService>().ReleaseObject(child.gameObject);
                }
            }
        }

        public static void CopyColliderToGameObject(GameObject original, GameObject target, bool isTrigger = true) {
            var originalCollider = original.GetComponent<Collider2D>();
            if (originalCollider == null) return;
            switch (originalCollider) {
                case BoxCollider2D boxCollider2D: {
                    var newCollider = target.AddComponent<BoxCollider2D>();
                    CopyColliderProperties(boxCollider2D, newCollider);
                    newCollider.isTrigger = isTrigger;
                    break;
                }
                case CircleCollider2D circleCollider2D: {
                    var newCollider = target.AddComponent<CircleCollider2D>();
                    CopyColliderProperties(circleCollider2D, newCollider);
                    newCollider.isTrigger = isTrigger;
                    break;
                }
                case PolygonCollider2D polygonCollider2D: {
                    var newCollider = target.AddComponent<PolygonCollider2D>();
                    CopyColliderProperties(polygonCollider2D, newCollider);
                    newCollider.isTrigger = isTrigger;
                    break;
                }
            }
        }

        private static void CopyColliderProperties(BoxCollider2D original, BoxCollider2D copy) {
            copy.size = original.size;
            copy.offset = original.offset;
        }

        private static void CopyColliderProperties(CircleCollider2D original, CircleCollider2D copy) {
            copy.radius = original.radius;
            copy.offset = original.offset;
        }

        private static void CopyColliderProperties(PolygonCollider2D original, PolygonCollider2D copy) {
            copy.points = original.points;
            copy.offset = original.offset;
        }
    }
}