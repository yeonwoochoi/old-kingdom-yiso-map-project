using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Test {
    public class TestText : MonoBehaviour {
        public TextMeshProUGUI text;

        [Button]
        public void Shake() {
            text.rectTransform.DOShakeAnchorPos(0.5f, new Vector2(5, 0), 10, 90, false, true);
        }
    }
}