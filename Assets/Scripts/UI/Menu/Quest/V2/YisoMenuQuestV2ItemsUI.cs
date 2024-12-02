using System;
using System.Linq;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Quest;
using Sirenix.OdinInspector;
using UI.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Quest.V2 {
    public class YisoMenuQuestV2ItemsUI : YisoPlayerUIController {
        [SerializeField] private YisoMenuQuestV2ItemHolderUI readyHolderUI;
        [SerializeField] private YisoMenuQuestV2ItemHolderUI progressHolderUI;
        [SerializeField] private YisoMenuQuestV2ItemHolderUI preCompleteHolderUI;
        [SerializeField] private YisoMenuQuestV2ItemHolderUI completeHolderUI;
        [SerializeField, Title("Scroll")] private RectTransform contentRect;
        [SerializeField, Title("Prefab")] private GameObject prefab;

        public event UnityAction<YisoQuestStatus, YisoQuest> OnQuestSelectedEvent;
        
        protected override void Awake() {
            base.Awake();
            readyHolderUI.SetCreateFunc(CreateItem);
            readyHolderUI.SetRefreshContent(RefreshContent);
            progressHolderUI.SetCreateFunc(CreateItem);
            progressHolderUI.SetRefreshContent(RefreshContent);
            preCompleteHolderUI.SetCreateFunc(CreateItem);
            preCompleteHolderUI.SetRefreshContent(RefreshContent);
            completeHolderUI.SetCreateFunc(CreateItem);
            completeHolderUI.SetRefreshContent(RefreshContent);
        }

        protected override void OnEnable() {
            base.OnEnable();
            readyHolderUI.OnEventRaised += OnQuestSelected;
            progressHolderUI.OnEventRaised += OnQuestSelected;
            preCompleteHolderUI.OnEventRaised += OnQuestSelected;
            completeHolderUI.OnEventRaised += OnQuestSelected;
        }

        protected override void OnDisable() {
            base.OnDisable();
            readyHolderUI.OnEventRaised -= OnQuestSelected;
            progressHolderUI.OnEventRaised -= OnQuestSelected;
            preCompleteHolderUI.OnEventRaised -= OnQuestSelected;
            completeHolderUI.OnEventRaised -= OnQuestSelected;
        }

        public void Clear() {
            readyHolderUI.Clear();
            progressHolderUI.Clear();
            preCompleteHolderUI.Clear();
            completeHolderUI.Clear();
        }
        
        public void SelectQuestManual(YisoQuestStatus status, YisoQuest quest) {
            var holder = GetHolderByStatus(status);
            if (!holder.IsOn) {
                holder.OnClickButton();
            }
            holder.SelectQuest(quest);
        }

        public void UpdateQuest(QuestStatusChangeEventArgs args) {
            if (args.To is YisoQuestStatus.READY) {
                readyHolderUI.AddQuest(args.Quest);
                return;
            }

            var beforeStatus = YisoQuestStatus.READY;
            var afterStatus = args.To;

            switch (afterStatus) {
                case YisoQuestStatus.PROGRESS:
                    beforeStatus = YisoQuestStatus.READY;
                    break;
                case YisoQuestStatus.PRE_COMPLETE:
                    beforeStatus = YisoQuestStatus.READY;
                    break;
                case YisoQuestStatus.COMPLETE:
                    beforeStatus = args.Quest.AutoComplete ? YisoQuestStatus.PROGRESS : YisoQuestStatus.PRE_COMPLETE;
                    break;
            }

            var beforeHolder = GetHolderByStatus(beforeStatus);
            var afterHolder = GetHolderByStatus(afterStatus);
            
            beforeHolder.RemoveQuest(args.Quest);
            afterHolder.AddQuest(args.Quest);
        }

        public void UpdateQuest(QuestUpdateEventArgs args) {
            
        }

        public void SetQuests() {
            foreach (var status in EnumExtensions.Values<YisoQuestStatus>().Where(s => s != YisoQuestStatus.IDLE)) {
                var holder = GetHolderByStatus(status);
                var quests = player.QuestModule.GetQuestsByStatuses(status);
                holder.SetTitle(quests.Count);
                foreach (var quest in quests) {
                    holder.AddQuest(quest);
                }
            }
        }

        private void SetTitles() {
            foreach (var status in EnumExtensions.Values<YisoQuestStatus>().Where(s => s != YisoQuestStatus.IDLE)) {
                var holder = GetHolderByStatus(status);
                var questCount = player.QuestModule.GetQuestCountByStatus(status);
                holder.SetTitle(questCount);
            }
        }

        private YisoMenuQuestV2ItemHolderUI GetHolderByStatus(YisoQuestStatus status) => status switch {
            YisoQuestStatus.READY => readyHolderUI,
            YisoQuestStatus.PROGRESS => progressHolderUI,
            YisoQuestStatus.PRE_COMPLETE => preCompleteHolderUI,
            YisoQuestStatus.COMPLETE => completeHolderUI,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

        private YisoMenuQuestItemUI CreateItem(GameObject parent) {
            var item = CreateObject<YisoMenuQuestItemUI>(prefab, parent.transform);
            item.Init();
            return item;
        }

        private void RefreshContent() {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        private void OnQuestSelected(YisoQuestStatus status, YisoQuest quest) {
            OnQuestSelectedEvent?.Invoke(status, quest);
        }
    }
}