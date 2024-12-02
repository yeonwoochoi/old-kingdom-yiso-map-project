using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Quest;
using Core.Domain.Types;
using UI.Menu.Base;
using UI.Menu.Quest.Detail;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Menu.Quest.V2 {
    public class YisoMenuQuestV2ContentUI : YisoMenuBasePanelUI {
        [SerializeField] private YisoMenuQuestV2ItemsUI itemsUI;
        [SerializeField] private YisoMenuQuestDetailUI detailUI;

        public event UnityAction<YisoQuest> OnDrawEventRaised;

        private YisoQuest currentQuest = null;

        protected override void RegisterEvents() {
            base.RegisterEvents();
            itemsUI.OnQuestSelectedEvent += OnQuestSelected;
            player.QuestModule.OnQuestEvent += OnQuestEvent;
            detailUI.OnClickDraw += OnClickDraw;
        }

        protected override void UnregisterEvents() {
            base.UnregisterEvents();
            itemsUI.OnQuestSelectedEvent -= OnQuestSelected;
            player.QuestModule.OnQuestEvent -= OnQuestEvent;
            detailUI.OnClickDraw -= OnClickDraw;
        }

        protected override void Start() {
            base.Start();
            detailUI.Init();
        }

        protected override void HandleData(object data) {
            var quest = (YisoQuest) data;
            var status = player.QuestModule.GetStatusByQuestId(quest.Id);
            itemsUI.SelectQuestManual(status, quest);
        }

        private void OnQuestEvent(QuestEventArgs args) {
            var quest = args.Quest;
            switch (args) {
                case QuestStatusChangeEventArgs statusChangedArgs:
                    itemsUI.UpdateQuest(statusChangedArgs);
                    break;
                case QuestUpdateEventArgs updateEventArgs:
                    itemsUI.UpdateQuest(updateEventArgs);
                    if (currentQuest?.Id != quest.Id) return;
                    detailUI.UpdateQuest(updateEventArgs);
                    break;
            }
        }

        private void OnClickDraw() {
            OnDrawEventRaised?.Invoke(currentQuest);
        }

        protected override void OnVisible() {
            itemsUI.SetQuests();
        }

        private void OnQuestSelected(YisoQuestStatus status, YisoQuest quest) {
            currentQuest = quest;
            detailUI.SetQuest(quest, status);
        }
        
        public override void ClearPanel() {
            itemsUI.Clear();
            detailUI.Clear();
        }

        public override YisoMenuTypes GetMenuType() => YisoMenuTypes.QUEST;
    }
}