using System.Collections.Generic;
using System.Linq;
using Core.Behaviour;
using Core.Domain.Quest;
using Sirenix.OdinInspector;
using UI.Base;
using UnityEngine;

namespace UI.Popup.Quest.Detail {
    public class YisoPopupQuestDetailContentUI : YisoUIController {
        [SerializeField, Title("Prefabs")] private GameObject questObjectivePrefab;
        [SerializeField] private GameObject questRewardPrefab;
        [SerializeField] private GameObject content;
        [SerializeField, Title("Items")] private List<YisoPopupQuestDetailRewardItemUI> rewardItems;
        [SerializeField] private List<YisoPopupQuestDetailObjectiveItemUI> objectiveItems;

        private YisoQuest quest;
        
        public void Clear() {
            rewardItems.ForEach(item => item.gameObject.SetActive(false));
            objectiveItems.ForEach(item => item.gameObject.SetActive(false));
        }

        public void SetQuest(YisoQuest quest) {
            this.quest = quest;
            
            ShowObjectiveItems();
            ShowRewardItems();
        }
        private void ShowObjectiveItems() {
            foreach (var item in objectiveItems) item.gameObject.SetActive(false);

            var reqs = quest.CompleteRequirements.Select(req => req.GetObjectiveUI(CurrentLocale)).ToList();

            var reqCount = reqs.Count;
            var objectCount = objectiveItems.Count;

            if (objectCount < reqCount) {
                var diff = reqCount - objectCount;
                for (var i = 0; i < diff; i++) objectiveItems.Add(CreateObjectiveItem());
            }

            for (var i = 0; i < reqCount; i++) {
                var (index, target, value, complete) = reqs[i];
                objectiveItems[i].gameObject.SetActive(true);
                objectiveItems[i].ShowObjective(index, target, value, complete);
            }
        }

        private void ShowRewardItems() {
            foreach (var item in rewardItems) item.gameObject.SetActive(false);
            var rewards = quest.GetRewardActions().Select(action => action.GetRewardUI(CurrentLocale)).ToList();

            var rewardCount = rewards.Count();
            var objectCount = rewardItems.Count();

            if (objectCount < rewardCount) {
                var diff = rewardCount - objectCount;
                for (var i = 0; i < diff; i++) rewardItems.Add(CreateRewardItem());
            }

            for (var i = 0; i < rewardCount; i++) {
                var (sprite, target, value) = rewards[i];
                rewardItems[i].gameObject.SetActive(true);
                rewardItems[i].ShowReward(sprite, target, value);
            }
        }

        private bool TryFindObjectiveItemIndex(int objectiveIndex, out int index) {
            index = -1;

            for (var i = 0; i < objectiveItems.Count; i++) {
                if (objectiveItems[i].Index != objectiveIndex) continue;
                index = i;
                break;
            }

            return index != -1;
        }
        
        private YisoPopupQuestDetailObjectiveItemUI CreateObjectiveItem() {
            var item = CreateObject<YisoPopupQuestDetailObjectiveItemUI>(questObjectivePrefab, content.transform);
            item.gameObject.SetActive(false);
            return item;
        }

        private YisoPopupQuestDetailRewardItemUI CreateRewardItem() {
            var item = CreateObject<YisoPopupQuestDetailRewardItemUI>(questRewardPrefab, content.transform);
            item.gameObject.SetActive(false);
            return item;
        }
    }
}