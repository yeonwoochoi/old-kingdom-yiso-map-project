using System;
using System.Collections.Generic;
using System.Linq;
using Core.Behaviour;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Locale;
using Core.Domain.Quest;
using Core.Service;
using Core.Service.Game;
using Core.Service.UI.HUD;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Quest.Detail {
    public class YisoMenuQuestDetailUI : YisoUIController {
        [SerializeField, Title("Title")] private TextMeshProUGUI questTitleText;
        [SerializeField, Title("Prefabs")] private GameObject questObjectivePrefab;
        [SerializeField] private GameObject questRewardPrefab;
        [SerializeField] private GameObject content;
        [SerializeField, Title("Defaults")] private YisoMenuQuestDetailObjectiveItemUI[] defaultObjectiveItems;
        [SerializeField] private YisoMenuQuestDetailRewardItemUI[] defaultRewardItems;
        [SerializeField, Title("Texts")] private TextMeshProUGUI objectiveText;
        [SerializeField] private TextMeshProUGUI rewardText;

        [SerializeField, Title("Action Button")]
        private Button drawButton;
        [SerializeField] private Button findPathButton;

        private UnityAction<int> onQuestPathFind = null;

        public event UnityAction OnClickDraw;
        
        private YisoQuest quest = null;
        
        public bool Active { get; private set; }

        private readonly List<YisoMenuQuestDetailObjectiveItemUI> objectiveItems = new();
        private readonly List<YisoMenuQuestDetailRewardItemUI> rewardItems = new();

        protected override void OnEnable() {
            base.OnEnable();
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>()
                .RegisterOnQuestPathFind(OnQuestPathFind);
        }

        protected override void OnDisable() {
            base.OnDisable();
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>()
                .UnregisterOnQuestPathFind(OnQuestPathFind);
        }

        public void Init() {
            objectiveItems.AddRange(defaultObjectiveItems);
            rewardItems.AddRange(defaultRewardItems);
            
            drawButton.onClick.AddListener(() => OnClickDraw?.Invoke());
            findPathButton.onClick.AddListener(() => onQuestPathFind?.Invoke(quest.Id));
        }

        public void SetQuest(YisoQuest quest, YisoQuestStatus status) {
            this.quest = quest;
            SetQuestTitle();
            
            ShowObjectiveItems();
            ShowRewardItems();
            
            drawButton.gameObject.SetActive(status is YisoQuestStatus.PROGRESS or YisoQuestStatus.PRE_COMPLETE);
            findPathButton.gameObject.SetActive(status is YisoQuestStatus.READY or YisoQuestStatus.PROGRESS);

            Active = true;
        }
        

        private void SetQuestTitle() {
            var progress = (int)quest.GetQuestProgress();
            string typeString;
            if (CurrentLocale == YisoLocale.Locale.KR) {
                typeString = quest.Type == YisoQuest.Types.MAIN ? "메인" : "서브";
            } else {
                typeString = quest.Type == YisoQuest.Types.MAIN ? "Main" : "Sub";
            }

            typeString = $"[{typeString}]";
            
            questTitleText.SetText($"{typeString} {quest.GetName(CurrentLocale)} ({progress.ToPercentage()})");
        }

        public void UpdateQuest(QuestUpdateEventArgs args) {
            if (!TryFindObjectiveItemIndex(args.Index, out var index)) {
                throw new Exception($"Cannot found objective index({args.Index})!");
            }
            
            objectiveItems[index].UpdateObjective(args.OriginalTarget, args.UpdateValue, args.IsComplete);
        }
        
        public void Clear() {
            quest = null;
            Active = false;
            
            rewardText.gameObject.SetActive(false);
            objectiveText.gameObject.SetActive(false);
            drawButton.gameObject.SetActive(false);
            foreach (var item in objectiveItems) item.gameObject.SetActive(false);
            foreach (var item in rewardItems) item.gameObject.SetActive(false);
        }
        
        private void ShowObjectiveItems() {
            foreach (var item in objectiveItems) item.gameObject.SetActive(false);

            var reqs = quest.CompleteRequirements.Select(req => req.GetObjectiveUI(CurrentLocale)).ToList();

            var reqCount = reqs.Count;

            if (reqCount == 0) {
                objectiveText.gameObject.SetActive(false);
                return;
            }
            
            objectiveText.gameObject.SetActive(true);
            
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
            if (rewardCount == 0) {
                rewardText.gameObject.SetActive(false);
                return;
            }
            
            rewardText.gameObject.SetActive(true);
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

        private void OnQuestPathFind(UnityAction<int> onQuestPathFind) {
            this.onQuestPathFind = onQuestPathFind;
        }
 
        private YisoMenuQuestDetailObjectiveItemUI CreateObjectiveItem() {
            var item = CreateObject<YisoMenuQuestDetailObjectiveItemUI>(questObjectivePrefab, content.transform);
            item.gameObject.SetActive(false);
            return item;
        }

        private YisoMenuQuestDetailRewardItemUI CreateRewardItem() {
            var item = CreateObject<YisoMenuQuestDetailRewardItemUI>(questRewardPrefab, content.transform);
            item.gameObject.SetActive(false);
            return item;
        }
    }
}