using System.Collections;
using Core.Behaviour;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Layer {
    [AddComponentMenu("Yiso/Tools/Layer/SetLayerForChildren")]
    public class YisoSetLayerForChildren : RunIBehaviour {
        public bool runOnStart = false;
        [ShowIf("runOnStart")] public LayerMask layer;
        [ShowIf("runOnStart")] public float delay;
        
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoSetLayerForChildren>();

        protected override void Start() {
            if (runOnStart) {
                SetLayer(layer, delay);
            }
        }
        
        public void SetLayer(LayerMask newLayer, float delay = 0f) {
            var layer = GetFirstLayerIndex(newLayer);
            if (layer < 0 || layer > 31) {
                LogService.Error("[YisoSetLayerForChildren] Invalid layer mask.");
                return;
            }

            if (delay > 0f) {
                StartCoroutine(SetLayerRecursivelyCo(gameObject, layer, delay));
            }
            else {
                SetLayerRecursively(gameObject, layer);
            }
        }

        private int GetFirstLayerIndex(LayerMask mask) {
            var bitmask = mask.value;
            var layer = 0;

            while (bitmask > 0) {
                if ((bitmask & 1) != 0) {
                    return layer;
                }

                bitmask >>= 1;
                layer++;
            }

            return -1;
        }

        private void SetLayerRecursively(GameObject obj, int newLayer) {
            obj.layer = newLayer;

            foreach (Transform child in obj.transform) {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        private IEnumerator SetLayerRecursivelyCo(GameObject obj, int newLayer, float delay) {
            yield return new WaitForSeconds(delay);
            SetLayerRecursively(obj, newLayer);
        }
    }
}