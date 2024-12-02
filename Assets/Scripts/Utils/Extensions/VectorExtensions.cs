using UnityEngine;

namespace Utils.Extensions {
    public static class VectorExtensions {
        public static float DeepDistance(this Vector3 a, Vector3 b) => Vector3.Distance(a, b);

        public static float SwallowDistance(this Vector3 a, Vector3 b) => (a - b).sqrMagnitude;
        
        public static float ToAngle(this Vector3 direction) {
            direction = direction.normalized;
            var n = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;
            return n;
        }

        public static Vector2[] GenerateRandomCirclePosition(int count, int thetaValue, float radiusX, float radiusY) {
            var results = new Vector2[count];

            for (var i = 0; i < count; i++) {
                var theta = Randomizer.Next(thetaValue);
                var randomRadius = Randomizer.Next(radiusX, radiusY);
                var radian = (Mathf.PI / 180) * theta;
                var x = randomRadius * Mathf.Cos(radian);
                var y = randomRadius * Mathf.Sin(radian);
                results[i] = new Vector2(x, y);
            }

            return results;
        }
    }
}