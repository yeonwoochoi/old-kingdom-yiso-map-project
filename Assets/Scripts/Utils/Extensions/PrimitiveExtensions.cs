using System;
using UnityEngine;

namespace Utils.Extensions {
    public static class PrimitiveExtensions {
        public static float ToNormalized(this int value) => value * 0.01f;

        public static double ToProb(this int value) => value * 0.01d;

        public static int MultiplyProb(this int currentValue, int applyValue) {
            var currentProb = currentValue.ToProb();
            var applyProb = applyValue.ToProb();

            var appliedProb = 1 - ((1 - currentProb) * (1 - applyProb));
            return (int) (appliedProb * 100d);
        }

        public static float GetAngleFromVectorFloat(this Vector3 dir) {
            dir = dir.normalized;
            var n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;
            return n;
        }

        public static float FlagToFloat(this bool flag) => flag ? 1f : 0f;

        public static int GetAngleFromVector(this Vector3 dir) => Mathf.RoundToInt(GetAngleFromVectorFloat(dir));

        public static int GetDigitNumber(this int value) => Mathf.FloorToInt(Mathf.Log10(value) + 1);

        public static int GetDigitNumber(this float value) => ((int)value).GetDigitNumber();

        public static int GetDigitNumber(this double value) => ((int)value).GetDigitNumber();

        public static string ToTimerFormat(this float time) {
            var minutes = Mathf.FloorToInt(time / 60);
            var seconds = Mathf.FloorToInt(time % 60);
            return minutes >= 100 ? $"{minutes:D3}:{seconds:D2}" : $"{minutes:D2}:{seconds:D2}";
        }
    }
}