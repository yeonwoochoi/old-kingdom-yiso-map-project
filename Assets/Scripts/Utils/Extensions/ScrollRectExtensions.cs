using UnityEngine;
using UnityEngine.UI;

namespace Utils.Extensions {
    public static class ScrollRectExtensions {
        private static Vector3[] corners = new Vector3[4];

        private static Bounds TransformBoundsTo(this RectTransform source, Transform target) {
            var bounds = new Bounds();

            if (source != null) {
                source.GetWorldCorners(corners);
                
                var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                var matrix = target.worldToLocalMatrix;
                for (var i = 0; i < 4; i++) {
                    var v = matrix.MultiplyPoint3x4(corners[i]);
                    vMin = Vector3.Min(v, vMin);
                    vMax = Vector3.Max(v, vMax);
                }

                bounds = new Bounds(vMin, Vector3.zero);
                bounds.Encapsulate(vMax);
            }
            
            return bounds;
        }

        private static float NormalizeScrollDistance(this ScrollRect scrollRect, int axis, float distance) {
            var viewPort = scrollRect.viewport;
            var viewRect = viewPort != null ? viewPort : scrollRect.GetComponent<RectTransform>();
            var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

            var content = scrollRect.content;
            var contentBounds = content != null ? content.TransformBoundsTo(viewRect) : new Bounds();

            var hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
            return distance / hiddenLength;
        }

        public static float ScrollToCenter(this ScrollRect scrollRect, RectTransform target,
            RectTransform.Axis axis = RectTransform.Axis.Horizontal) {
            var view = scrollRect.viewport ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();
            var viewRect = view.rect;
            var elementBounds = target.TransformBoundsTo(view);

            if (axis == RectTransform.Axis.Vertical) {
                var offset = viewRect.center.y - elementBounds.center.y;
                var scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
                return scrollPos;
            }
            else {
                var offset = viewRect.center.x - elementBounds.center.x;
                var scrollPos = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(0, offset);
                return scrollPos;
            }
        }
    }
}