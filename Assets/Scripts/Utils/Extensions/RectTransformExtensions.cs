using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Utils.Extensions {
    public static class RectTransformExtensions {
        public static void SetLeft(this RectTransform transform, float left) {
            transform.offsetMin = new Vector2(left, transform.offsetMin.y);
        }

        public static TweenerCore<float, float, FloatOptions> DOLeft(this RectTransform transform, float endValue,
            float duration) {
            var t = DOTween.To(
                () => transform.offsetMin.x,
                value => transform.offsetMin = new Vector2(value, transform.offsetMin.y),
                endValue,
                duration);
            t.SetTarget(transform);
            return t;
        }

        public static void SetRight(this RectTransform transform, float right) {
            transform.offsetMax = new Vector2(-right, transform.offsetMax.y);
        }
        
        public static TweenerCore<float, float, FloatOptions> DORight(this RectTransform transform, float endValue,
            float duration) {
            var t = DOTween.To(
                () => transform.offsetMax.x,
                value => transform.offsetMax = new Vector2(-value, transform.offsetMax.y),
                endValue,
                duration);
            t.SetTarget(transform);
            return t;
        }

        public static void SetTop(this RectTransform transform, float top) {
            transform.offsetMax = new Vector2(transform.offsetMax.x, -top);
        }
        
        public static TweenerCore<float, float, FloatOptions> DOTop(this RectTransform transform, float endValue,
            float duration) {
            var t = DOTween.To(
                () => -transform.offsetMax.y,
                value => transform.offsetMax = new Vector2(transform.offsetMax.x, -value),
                endValue,
                duration);
            t.SetTarget(transform);
            return t;
        }


        public static void SetBottom(this RectTransform transform, float bottom) {
            transform.offsetMin = new Vector2(transform.offsetMin.x, bottom);
        }
        
        public static TweenerCore<float, float, FloatOptions> DOBottom(this RectTransform transform, float endValue,
            float duration) {
            var t = DOTween.To(
                () => transform.offsetMin.y,
                value => transform.offsetMin = new Vector2(transform.offsetMin.x, value),
                endValue,
                duration);
            t.SetTarget(transform);
            return t;
        }

        public static void SetLeftRightTopBottom(this RectTransform transform, float left, float right, float top,
            float bottom) {
            transform.SetLeft(left);
            transform.SetRight(right);
            transform.SetTop(top);
            transform.SetBottom(bottom);
        }
    }
}