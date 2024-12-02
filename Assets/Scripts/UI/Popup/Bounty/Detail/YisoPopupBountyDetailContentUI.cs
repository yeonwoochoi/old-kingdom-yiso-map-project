using System;
using System.Collections.Generic;
using Core.Domain.Bounty;
using Core.Domain.Locale;
using Core.Service;
using Core.Service.Domain;
using Sirenix.OdinInspector;
using UI.Base;
using UI.Popup.Quest.Detail;
using UnityEngine;
using Utils.Extensions;

namespace UI.Popup.Bounty.Detail {
    public class YisoPopupBountyDetailContentUI : YisoUIController {
        [SerializeField, Title("Prefab")] private GameObject rewardPrefab;
        [SerializeField] private GameObject rewardContent;
        [SerializeField, Title("Items")] private List<YisoPopupQuestDetailRewardItemUI> rewardItems;

        private YisoBounty bounty;

        public void Clear() {
            rewardItems.ForEach(r => r.gameObject.SetActive(false));
        }

        public void SetBounty(YisoBounty bounty) {
            this.bounty = bounty;
            ShowRewardItems();
        }

        private void ShowRewardItems() {
            foreach (var item in rewardItems) item.gameObject.SetActive(false);
            var rewardCount = bounty.ItemUIs.Count;
            if (bounty.BountyReward > 0)
                rewardCount += 1;
            
            var objectCount = rewardItems.Count;

            if (objectCount < rewardCount) {
                var diff = rewardCount - objectCount;
                for (var i = 0; i < diff; i++) rewardItems.Add(CreateRewardItem());
            }

            var moneyIcon = YisoServiceProvider.Instance.Get<IYisoDomainService>().GetMoneyIcon();

            var idx = 0;
            if (bounty.BountyReward > 0) {
                var moneyTitle = CurrentLocale == YisoLocale.Locale.KR ? "량" : "NYANG";
                rewardItems[idx].ShowReward(moneyIcon, moneyTitle, bounty.BountyReward.ToCommaString());
                rewardItems[idx].gameObject.SetActive(true);
                idx++;
            }

            foreach (var item in bounty.ItemUIs) {
                rewardItems[idx].ShowReward(item.Icon, item.GetName(CurrentLocale));
                rewardItems[idx].gameObject.SetActive(true);
                idx++;
            }
        }

        private YisoPopupQuestDetailRewardItemUI CreateRewardItem() {
            var item = CreateObject<YisoPopupQuestDetailRewardItemUI>(rewardPrefab, rewardContent.transform);
            item.gameObject.SetActive(false);
            return item;
        }
    }
}