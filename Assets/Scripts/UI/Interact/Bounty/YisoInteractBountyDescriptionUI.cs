using System.Collections.Generic;
using Core.Domain.Bounty;
using Core.Domain.Locale;
using Core.Domain.Types;
using TMPro;
using UI.Base;
using UI.Interact.Bounty.Description;
using UI.Menu.Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Bounty {
    public class YisoInteractBountyDescriptionUI : YisoUIController {
        [SerializeField] private Image targetImage;
        [SerializeField] private TextMeshProUGUI targetNameText;
        [SerializeField] private TextMeshProUGUI targetDescriptionText;
        [SerializeField] private TextMeshProUGUI bountyText;
        [SerializeField] private GameObject rewardTitleObject;
        [SerializeField] private List<YisoInteractBountyRewardItemUI> rewardItemUIs;
        [SerializeField] private YisoMenuInventoryButtonUI goButton;
        [SerializeField] private CanvasGroup completeImagePanel;
         
        public event UnityAction<YisoBounty> OnClickButtonEvent;
        
        public YisoBounty Bounty { get; private set; }
        
        protected override void Start() {
            base.Start();
            goButton.InventoryButton.onClick.AddListener(() => OnClickButtonEvent?.Invoke(Bounty));
        }
        
        public void SetItem(YisoBounty bounty, YisoBountyStatus status) {
            Bounty = bounty;

            targetImage.sprite = bounty.TargetIcon;
            targetImage.enabled = true;
            var targetName = bounty.GetTargetName(CurrentLocale);
            var append = string.Empty;
            if (status == YisoBountyStatus.COMPLETE) {
                append = CurrentLocale == YisoLocale.Locale.KR ? " (완료)" : " (Complete)";
            }
            targetNameText.SetText($"{targetName}{append}");
            targetDescriptionText.SetText(bounty.GetDescription(CurrentLocale));
            
            bountyText.SetText(bounty.BountyReward.ToCommaString());
            
            rewardTitleObject.SetActive(bounty.RewardItemIds.Count > 0);

            goButton.Active(status != YisoBountyStatus.COMPLETE);
            completeImagePanel.Visible(status == YisoBountyStatus.COMPLETE);
            
            var rewardCount = bounty.RewardItemIds.Count;
            for (var i = 0; i < rewardCount; i++) {
                rewardItemUIs[i].SetItem(bounty.ItemUIs[i], CurrentLocale);
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