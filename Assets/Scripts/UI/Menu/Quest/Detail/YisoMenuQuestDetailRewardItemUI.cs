using Core.Behaviour;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu.Quest.Detail {
    public class YisoMenuQuestDetailRewardItemUI : RunIBehaviour {
        [SerializeField] private Image rewardImage;
        [SerializeField] private TextMeshProUGUI rewardText;

        public void ShowReward(Sprite icon, string target, string value) {
            rewardImage.sprite = icon;
            rewardText.SetText($"{target} (+{value})");
        }
    }
}