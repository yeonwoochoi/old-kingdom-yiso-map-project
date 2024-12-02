using Core.Behaviour;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup.Quest.Detail {
    public class YisoPopupQuestDetailRewardItemUI : RunIBehaviour {
        [SerializeField] private Image rewardImage;
        [SerializeField] private TextMeshProUGUI rewardText;

        public void ShowReward(Sprite icon, string target, string value) {
            rewardImage.sprite = icon;
            rewardText.SetText($"{target} (+{value})");
        }

        public void ShowReward(Sprite icon, string target) {
            rewardImage.sprite = icon;
            rewardText.SetText(target);
        }
    }
}