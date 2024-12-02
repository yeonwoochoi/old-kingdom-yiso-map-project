using System;
using System.Linq;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Quest;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using UI.Menu.Base;
using UI.Menu.Quest.Detail;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Quest { 
    public class YisoMenuQuestContentUI : YisoMenuBasePanelUI {
        [SerializeField, Title("Tabs")] private Toggle[] tabs;
        [SerializeField, Title("Items")] private YisoMenuQuestItemsUI itemsUI;
        [SerializeField, Title("Detail")] private YisoMenuQuestDetailUI detailUI;

        public event UnityAction<YisoQuest> OnDrawEventRaised;
        
        private YisoQuest.Types currentType = YisoQuest.Types.MAIN;
        private YisoQuest currentQuest = null;

        protected override void Awake() {
            base.Awake();
            for (var i = 0; i < tabs.Length; i++) 
                tabs[i].onValueChanged.AddListener(OnToggleTab(i));
        }

        protected override void RegisterEvents() {
            base.RegisterEvents();
            player.QuestModule.OnQuestEvent += OnQuestEvent;
            itemsUI.OnEventRaised += OnQuestSelected;
            detailUI.OnClickDraw += OnClickDraw;
        }

        protected override void UnregisterEvents() {
            base.UnregisterEvents();
            player.QuestModule.OnQuestEvent -= OnQuestEvent;
            itemsUI.OnEventRaised -= OnQuestSelected;
            detailUI.OnClickDraw -= OnClickDraw;
        }

        protected override void HandleData(object data) {
            var quest = (YisoQuest) data;
            OnQuestSelected(quest);
        }

        private void OnQuestSelected(YisoQuest quest) {
            currentQuest = quest;
            // detailUI.SetQuest(quest);
        }

        private void OnQuestEvent(QuestEventArgs args) {
            var quest = args.Quest;
            if (quest.Type != currentType) return;
            switch (args) {
                case QuestStatusChangeEventArgs statusChangeEventArgs:
                    itemsUI.UpdateQuest(quest);
                    OnQuestStatusChanged(statusChangeEventArgs);
                    break;
                case QuestUpdateEventArgs updateEventArgs:
                    itemsUI.UpdateQuest(quest);
                    if (currentQuest?.Id != quest.Id) return;
                    detailUI.UpdateQuest(updateEventArgs);
                    break;
            }
        }

        private void OnClickDraw() {
            OnDrawEventRaised?.Invoke(currentQuest);
        }

        private void OnQuestStatusChanged(QuestStatusChangeEventArgs args) {
            var quest = args.Quest;
            if (args.IsProgress && currentType == quest.Type) {
                itemsUI.SetQuest(quest);
            }
        }

        public override void Init() {
            SetQuests();
            detailUI.Init();
        }

        public override YisoMenuTypes GetMenuType() => YisoMenuTypes.QUEST;

        private UnityAction<bool> OnToggleTab(int index) => flag => {
            if (!flag) {
                itemsUI.Clear();
                detailUI.Clear();
                return;
            }

            var type = ToQuestType(index);
            currentType = type;
        };

        private void SetQuests() {
            var quests = player.QuestModule
                .GetQuestsByStatuses(YisoQuestStatus.READY, YisoQuestStatus.PROGRESS, YisoQuestStatus.PRE_COMPLETE, YisoQuestStatus.COMPLETE)
                .Where(quest => quest.Type == currentType)
                .OrderBy(quest => !quest.IsComplete ? 1 : 0)
                .ToList();

            foreach (var quest in quests) {
                itemsUI.SetQuest(quest);
            }
        }

        private YisoQuest.Types ToQuestType(int index) => index.ToEnum<YisoQuest.Types>();

        public override void ClearPanel() {
            tabs[0].isOn = true;
        }
    }
}