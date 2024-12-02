using System.Globalization;
using Core.Behaviour;
using UnityEngine;
using UnityEngine.UI;
using Utils.Beagle;

namespace Tools.SceneLoading.Helpers {
    [AddComponentMenu("Yiso/Tools/Scene/Helper/SceneLoadingTextProgress")]
    public class YisoSceneLoadingTextProgress : RunIBehaviour {
        public float remapMin = 0f;
        public float remapMax = 100f;
        public int numberOfDecimals = 0;
        public string prefix = "";
        public string suffix = "%";

        protected Text text;

        protected override void Awake() {
            text = gameObject.GetComponent<Text>();
        }

        public virtual void SetProgress(float newValue) {
            var remappedValue = YisoMathUtils.Remap(newValue, 0f, 1f, remapMin, remapMax);
            var displayValue = YisoMathUtils.RoundToDecimal(remappedValue, numberOfDecimals);
            text.text = $"{prefix}{displayValue.ToString(CultureInfo.InvariantCulture)}{suffix}";
        }
    }
}