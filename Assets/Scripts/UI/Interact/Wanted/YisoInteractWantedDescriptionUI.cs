using System.Collections.Generic;
using Core.Domain.Wanted;
using TMPro;
using UI.Base;
using UI.Interact.Wanted.Description;
using UI.Menu.Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Wanted {
    public class YisoInteractWantedDescriptionUI : YisoUIController {
        [SerializeField] private Image targetImage;
        [SerializeField] private TextMeshProUGUI targetNameText;
        [SerializeField] private TextMeshProUGUI targetDescriptionText;
        [SerializeField] private TextMeshProUGUI bountyText;
        [SerializeField] private GameObject rewardTitleObject;
        [SerializeField] private List<YisoInteractWantedRewardItemUI> rewardItemUIs;
        [SerializeField] private YisoMenuInventoryButtonUI goButton;

        public event UnityAction<YisoWanted> OnClickButtonEvent; 
        
        public YisoWanted Wanted { get; private set; }

        protected override void Start() {
            base.Start();
            goButton.InventoryButton.onClick.AddListener(() => OnClickButtonEvent?.Invoke(Wanted));
        }

        public void SetItem(YisoWanted wanted) {
            Wanted = wanted;

            targetImage.sprite = wanted.TargetImage;
            targetImage.enabled = true;
            targetNameText.SetText(wanted.GetTargetName(CurrentLocale));
            targetDescriptionText.SetText(wanted.GetTargetDescription(CurrentLocale));
            
            bountyText.SetText(wanted.Bounty.ToCommaString());
            
            rewardTitleObject.SetActive(wanted.RewardItems.Count > 0);

            goButton.Active(true);
            
            var rewardCount = wanted.RewardItems.Count;
            for (var i = 0; i < rewardCount; i++) {
                rewardItemUIs[i].SetItem(wanted.RewardItems[i], CurrentLocale);
            }
        }

        public void Clear() {
            targetImage.sprite = null;
            targetImage.enabled = false;
            targetNameText.SetText("");
            targetDescriptionText.SetText("");
            bountyText.SetText("");
            rewardTitleObject.SetActive(false);
            goButton.Active(false);
            
            foreach (var itemUI in rewardItemUIs) itemUI.Clear();
        }
    }
}