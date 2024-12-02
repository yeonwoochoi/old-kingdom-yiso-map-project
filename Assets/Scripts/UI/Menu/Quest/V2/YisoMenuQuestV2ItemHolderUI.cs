using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Locale;
using Core.Domain.Quest;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Quest.V2 {
    public class YisoMenuQuestV2ItemHolderUI : YisoUIController {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private GameObject itemHolder;
        [SerializeField] private YisoMenuQuestItemUI[] defaultQuestItems;
        [SerializeField] private YisoQuestStatus status;
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField, Title("Toggle")] private Image nonActiveGraphic;
        [SerializeField] private Image activeGraphic;

        public YisoQuestStatus Status => status;

        public event UnityAction<YisoQuestStatus, YisoQuest> OnEventRaised; 
        
        private readonly List<YisoMenuQuestItemUI> questItems = new();

        private Func<GameObject, YisoMenuQuestItemUI> createFunc = null;

        private UnityAction refreshContentAction = null;
        
        private bool isOn = false;

        private Button button;

        public bool IsOn => isOn;

        public void SetCreateFunc(Func<GameObject, YisoMenuQuestItemUI> createFunc) {
            this.createFunc = createFunc;
        }

        public void SetRefreshContent(UnityAction refreshContentAction) {
            this.refreshContentAction = refreshContentAction;
        }

        private CanvasGroup toggleCanvas;

        protected override void Start() {
            base.Start();
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClickButton);
        }

        private void InitItems() {
            questItems.AddRange(defaultQuestItems);
            for (var i = 0; i < questItems.Count; i++) {
                questItems[i].Init();
                questItems[i].QuestToggle.group = toggleGroup;
                questItems[i].QuestToggle.onValueChanged.AddListener(OnClickQuest(i));
            }
        }

        public void Clear() {
            foreach (var item in questItems) {
                item.Clear();
                item.gameObject.SetActive(false);
            }
            
            if (isOn) OnClickButton();
        }

        public void SetTitle(int count = -1) {
            var countText = string.Empty;
            if (count > -1) countText = $"({count})";

            var title = GetTitleByStatus();
            titleText.SetText($"{title} {countText}");
        }

        public void AddQuest(YisoQuest quest) {
            if (questItems.Count == 0) InitItems();

            var index = GetNonActiveIndexOrCreate();
            questItems[index].SetQuest(quest);
        }

        public void SelectQuest(YisoQuest quest) {
            var index = GetIndexByQuestId(quest.Id);
            questItems[index].QuestToggle.isOn = true;
            OnEventRaised?.Invoke(status, quest);
        }

        public void RemoveQuest(YisoQuest quest) {
            var index = GetIndexByQuestId(quest.Id);
            questItems[index].Clear();
            questItems[index].gameObject.SetActive(false);
            questItems[index].gameObject.transform.SetAsLastSibling();
        }

        private int GetNonActiveIndexOrCreate() {
            for (var i = 0; i < questItems.Count; i++) {
                if (questItems[i].Active) continue;
                questItems[i].gameObject.SetActive(IsOn);
                questItems[i].Active = true;
                return i;
            }

            if (createFunc == null) throw new Exception("Create Func not set");
            var newItem = createFunc(itemHolder);
            newItem.gameObject.SetActive(IsOn);
            newItem.Active = true;
            newItem.QuestToggle.group = toggleGroup;
            
            questItems.Add(newItem);
            return questItems.Count - 1;
        }

        private int GetIndexByQuestId(int questId) {
            for (var i = 0; i < questItems.Count; i++) {
                if (!questItems[i].Active || questItems[i].Quest.Id != questId) continue;
                return i;
            }

            throw new Exception($"Quest(id={questId}|status={status}) not found!");
        }

        private UnityAction<bool> OnClickQuest(int index) => flag => {
            if (!flag) return;
            var quest = questItems[index].Quest;
            OnEventRaised?.Invoke(status, quest);
        };

        public void OnClickButton() {
            isOn = !isOn;
            activeGraphic.enabled = isOn;
            nonActiveGraphic.enabled = !isOn;
            foreach (var item in questItems.Where(item => item.Active)) {
                item.gameObject.SetActive(IsOn);
            }
            refreshContentAction?.Invoke();
        }

        private string GetTitleByStatus() => status switch {
            YisoQuestStatus.READY => CurrentLocale == YisoLocale.Locale.KR ? "시작 가능 퀘스트 목록" : "Ready Quests",
            YisoQuestStatus.PROGRESS => CurrentLocale == YisoLocale.Locale.KR ? "진행 퀘스트 목록" : "Progress Quests",
            YisoQuestStatus.PRE_COMPLETE => CurrentLocale == YisoLocale.Locale.KR ? "완료 가능 퀘스트 목록" : "Pre Complete Quests",
            YisoQuestStatus.COMPLETE => CurrentLocale == YisoLocale.Locale.KR ? "완료 퀘스트 목록" : "Complete Quests",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}