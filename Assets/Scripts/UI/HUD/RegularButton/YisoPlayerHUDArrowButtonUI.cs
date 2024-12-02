using Core.Behaviour;
using TMPro;
using UnityEngine;
using Utils.Extensions;

namespace UI.HUD.RegularButton {
    public class YisoPlayerHUDArrowButtonUI : RunIBehaviour {
        [SerializeField] private CanvasGroup activeCanvas;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private CanvasGroup buttonCanvas;
        
        public void VisibleButton(bool flag) {
            buttonCanvas.Visible(flag.FlagToFloat());
        }

        public void SetArrowCount(int count) {
            activeCanvas.Visible(count > 0);
            
            if (count > 999) {
                countText.SetText("999");
                return;
            }
            
            countText.SetText(count.ToString());
        }
    }
}