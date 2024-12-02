using System.Collections.Generic;
using UnityEngine;

namespace Utils {
    public static class YieldInstructionCache {
        private class FloatComparer : IEqualityComparer<float> {
            public bool Equals(float x, float y) => Mathf.Approximately(x, y);

            public int GetHashCode(float obj) => obj.GetHashCode();
        }
        
        private static readonly Dictionary<float, WaitForSeconds> TimeInterval = new Dictionary<float, WaitForSeconds>(new FloatComparer());

        public static WaitForSeconds WaitForSeconds(float seconds) {
            if (!TimeInterval.TryGetValue(seconds, out var wfs))
                TimeInterval.Add(seconds, wfs = new WaitForSeconds(seconds));
            return wfs;
        }
    }
}