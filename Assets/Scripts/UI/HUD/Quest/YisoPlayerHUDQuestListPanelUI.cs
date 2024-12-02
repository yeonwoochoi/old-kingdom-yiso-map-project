using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Locale;
using Core.Domain.Quest;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.HUD.Quest {
    public class YisoPlayerHUDQuestListPanelUI : YisoPlayerUIController {
        [SerializeField, Title("Prefab")] private GameObject questItemPrefab;
        [SerializeField] private GameObject readyContent;
        [SerializeField] private GameObject progressContent;
        [SerializeField] private GameObject preCompleteContent;
        [SerializeField, Title("Canvas")] private CanvasGroup canvasGroup;
        [SerializeField, Title("Titles")] private TextMeshProUGUI readyTitleText;
        [SerializeField] private TextMeshProUGUI progressTitleText;
        [SerializeField] private TextMeshProUGUI preCompleteTitleText;
        [SerializeField, Title("Refresh")] private RectTransform contentRect;

        private readonly Dictionary<YisoQuestStatus, List<YisoPlayerHUDQuestPanelItemUI>> items = new () {
            { YisoQuestStatus.READY , new List<YisoPlayerHUDQuestPanelItemUI>()},
            { YisoQuestStatus.PROGRESS, new List<YisoPlayerHUDQuestPanelItemUI>()},
            { YisoQuestStatus.PRE_COMPLETE, new List<YisoPlayerHUDQuestPanelItemUI>()},
        };

        private readonly Dictionary<YisoQuestStatus, YisoQuestStatus[]> beforeStatusDict = new() {
            { YisoQuestStatus.COMPLETE , new [] { YisoQuestStatus.PRE_COMPLETE , YisoQuestStatus.PROGRESS, YisoQuestStatus.READY}},
            { YisoQuestStatus.PRE_COMPLETE, new[] { YisoQuestStatus.PROGRESS, YisoQuestStatus.READY } },
            { YisoQuestStatus.PROGRESS, new[] { YisoQuestStatus.READY } },
        };

        public event UnityAction<YisoQuest> OnQuestEvent;

        protected override void Start() {
            base.Start();
            SetQuests();
        }

        public void Visible(bool flag) {
            canvasGroup.Visible(flag.FlagToFloat());
        }

        public void ChangeStatus(QuestStatusChangeEventArgs args) {
            var afterStatus = args.To;

            if (args.IsDraw) {
                if (args.Before is YisoQuestStatus.IDLE or YisoQuestStatus.COMPLETE) return;
                UnSetQuest(args.Before, args.Quest);
                return;
            }
            
            if (TryFindBeforeItemStatus(afterStatus, args.QuestId, out var beforeStatus)) {
                UnSetQuest(beforeStatus, args.Quest);
                RefreshContentRect();
            }
            
            if (args.StatusIs(YisoQuestStatus.COMPLETE) || args.StatusIs(YisoQuestStatus.IDLE)) return;
            
            SetQuest(afterStatus, args.Quest);
            RefreshContentRect();
            SetQuestTitles();
        }

        private void RefreshContentRect() {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        public void UpdateQuest(QuestUpdateEventArgs args) {
            var status = player.QuestModule.GetStatusByQuestId(args.QuestId);
            if (status != YisoQuestStatus.PROGRESS) return;
            var index = GetQuestIndex(status, args.QuestId);
            if (index == -1) {
                var logger = YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoPlayerHUDQuestListPanelUI>();
                logger.Warn($"Quest(id={args.QuestId}|status={status}) not exists!");
                return;
            }
            var item = items[status][index];
            item.SetQuestNameWithProgress(args.Quest, CurrentLocale);
        }

        private void SetQuests() {
            foreach (var status in GetStatuses()) {
                var quests = player.QuestModule.GetQuestsByStatuses(status);
                foreach (var quest in quests) {
                    SetQuest(status, quest);
                }
            }
            
            SetQuestTitles();
        }

        private bool TryFindBeforeItemStatus(YisoQuestStatus status, int questId, out YisoQuestStatus beforeStatus) {
            beforeStatus = YisoQuestStatus.IDLE;
            if (status is YisoQuestStatus.IDLE or YisoQuestStatus.READY) {
                return false;
            }
            
            var beforeStatuses = beforeStatusDict[status];
            foreach (var s in beforeStatuses) {
                var item = items[s].Where(item => item.Active).FirstOrDefault(item => item.CompareId(questId));
                if (item == null) continue;
                beforeStatus = s;
                return true;
            }

            return false;
        }

        private void SetQuestTitles() {
            foreach (var status in GetStatuses()) {
                var titleText = GetTitleTextByStatus(status);
                var title = GetTitleByStatus(status);
                var count = player.QuestModule.GetQuestsByStatuses(status).Count;
                titleText.SetText($"{title} ({count.ToCommaString()})");
            }
        }

        private void UnSetQuest(YisoQuestStatus status, YisoQuest quest) {
            var index = GetQuestIndex(status, quest.Id);
            if (index == -1) {
                return;
            }
            items[status][index].Active = false;
        }

        private void SetQuest(YisoQuestStatus status, YisoQuest quest) {
            if (!TryGetIndex(status, out var index)) {
                var newItem = CreateItem(GetContentByStatus(status));
                items[status].Add(newItem);
                index = items[status].Count - 1;
            }

            items[status][index].Active = true;
            items[status][index].SetQuest(index, quest, CurrentLocale);
            if (status is YisoQuestStatus.PROGRESS or YisoQuestStatus.PRE_COMPLETE)
                items[status][index].SetQuestNameWithProgress(quest, CurrentLocale);
            else if (status is YisoQuestStatus.READY) 
                items[status][index].SetQuestName(quest, CurrentLocale);
            
            items[status][index].ItemButton.onClick.AddListener(() => OnClickQuest(index, status));
        }

        private bool TryGetIndex(YisoQuestStatus status, out int index) {
            index = -1;

            for (var i = 0; i < items[status].Count(); i++) {
                var item = items[status][i];
                if (item.Active) continue;
                index = i;
                return true;
            }
            
            return false;
        }

        private void OnClickQuest(int index, YisoQuestStatus status) {
            var item = items[status][index];
            var quest = item.Quest;
            RaiseQuest(quest);
        }

        private int GetQuestIndex(YisoQuestStatus status, int questId) {
            var itemUis = items[status];
            
            for (var i = 0; i < itemUis.Count; i++) {
                var item = itemUis[i];
                if (!item.Active) continue;
                if (!item.CompareId(questId)) continue;
                return i;
            }

            return -1;
        }

        private IEnumerable<YisoQuestStatus> GetStatuses() => EnumExtensions.Values<YisoQuestStatus>()
            .Where(status =>
                status is YisoQuestStatus.READY or YisoQuestStatus.PROGRESS or YisoQuestStatus.PRE_COMPLETE);

        private GameObject GetContentByStatus(YisoQuestStatus status) => status switch {
            YisoQuestStatus.READY => readyContent,
            YisoQuestStatus.PROGRESS => progressContent,
            YisoQuestStatus.PRE_COMPLETE => preCompleteContent,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

        private TextMeshProUGUI GetTitleTextByStatus(YisoQuestStatus status) => status switch {
            YisoQuestStatus.READY => readyTitleText,
            YisoQuestStatus.PROGRESS => progressTitleText,
            YisoQuestStatus.PRE_COMPLETE => preCompleteTitleText,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

        private string GetTitleByStatus(YisoQuestStatus status) => status switch {
            YisoQuestStatus.READY => CurrentLocale == YisoLocale.Locale.KR ? "시작 가능 퀘스트 목록" : "Ready Quests",
            YisoQuestStatus.PROGRESS => CurrentLocale == YisoLocale.Locale.KR ? "진행 퀘스트 목록" : "Progress Quests",
            YisoQuestStatus.PRE_COMPLETE => CurrentLocale == YisoLocale.Locale.KR ? "완료 가능 퀘스트 목록" : "Pre Complete Quests",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

        private YisoPlayerHUDQuestPanelItemUI CreateItem(GameObject content) {
            return CreateObject<YisoPlayerHUDQuestPanelItemUI>(questItemPrefab, content.transform);
        }

        private void RaiseQuest(YisoQuest quest) {
            OnQuestEvent?.Invoke(quest);
        }
    }
}