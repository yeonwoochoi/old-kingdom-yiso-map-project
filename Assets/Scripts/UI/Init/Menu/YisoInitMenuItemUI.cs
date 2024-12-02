using Core.Behaviour;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Init.Menu {
    public class YisoInitMenuItemUI : RunIBehaviour {
        [SerializeField] private Toggle toggle;
        [SerializeField] private Image selectedImage;
        [SerializeField] private TextMeshProUGUI menuText;

        public Toggle MenuToggle => toggle;

        public Image SelectedImage => selectedImage;

        public void OnToggle(bool flag) {
            toggle.interactable = !flag;
            selectedImage.enabled = flag;
            if (flag) {
                menuText.fontStyle = FontStyles.Bold;
            } else {
                menuText.fontStyle ^= FontStyles.Bold;
            }
        }
    }
}