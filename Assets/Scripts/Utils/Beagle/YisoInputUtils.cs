using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utils.Beagle {
    public static class YisoInputUtils {
        private static float lastClickTime = 0f;
        private static float doubleClickInterval = 0.25f;
        
        public static bool IsDoubleClicked() {
            if (Time.time - lastClickTime < doubleClickInterval) {
                lastClickTime = 0f;
                return true;
            }

            lastClickTime = Time.time;
            return false;
        }

        public static bool IsPointerOverUI() {
            var pointerData = new PointerEventData(EventSystem.current) {
                position = Input.mousePosition
            };
            
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            return results.Count > 0;
        }
    }
}