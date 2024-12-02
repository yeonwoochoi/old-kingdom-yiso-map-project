using UnityEngine;

namespace Utils.Extensions {
    public static class DistanceExtensions {
        public static float GetDistance(this Vector3 position, Vector3 target) =>
            target.SwallowDistance(position);

        public static float GetDistance<T>(this T component, Vector3 target) where T : Component => 
            component.transform.position.GetDistance(target);

        public static float GetDistance<T>(this T component, T target) where T : Component =>
            component.GetDistance(target.transform.position);

        public static bool IsInRangedDistance(this Vector3 position, Vector3 target, float @from, float @to) {
            var distance = position.GetDistance(target);
            return distance >= @from && distance <= @to;
        }

        public static bool IsInRangedDistance<T>(this T component, Vector3 target, float @from, float @to)
            where T : Component =>
            IsInRangedDistance(component.transform.position, target, @from, @to);

        public static bool IsInRangedDistance<T>(this T component, T target, float @from, float @to)
            where T : Component =>
            IsInRangedDistance(component, target.transform.position, @from, @to);
        
        
    }
}