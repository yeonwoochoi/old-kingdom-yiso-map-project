using Core.Behaviour;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.SceneLoading.Helpers {
    [AddComponentMenu("Yiso/Tools/Scene/Helper/SceneLoadingImageProgress")]
    public class YisoSceneLoadingImageProgress : RunIBehaviour {
        protected Image image;

        protected override void Awake() {
            image = gameObject.GetComponent<Image>();
        }

        public virtual void SetProgress(float progress) {
            image.fillAmount = progress;
        }
    }
}