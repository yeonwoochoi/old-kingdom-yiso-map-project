using UnityEngine;

namespace Utils.Beagle {
    public class YisoLayerUtils {
        public static bool CheckLayerInLayerMask(int layer, LayerMask layerMask) {
            return ((1 << layer) & layerMask) != 0;
        }
    }
}