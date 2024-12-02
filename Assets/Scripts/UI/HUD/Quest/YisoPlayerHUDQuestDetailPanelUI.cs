using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Quest;
using Core.Domain.Types;
using Core.Service;
using Core.Service.UI;
using Core.Service.UI.HUD;
using Sirenix.OdinInspector;
using UI.Base;
using UI.Menu.Quest.Detail;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.HUD.Quest {
    public class YisoPlayerHUDQuestDetailPanelUI : YisoUIController {
        public event UnityAction OnBackEvent; 
        
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private List<YisoMenuQuestDetailObjectiveItemUI> objectiveItemUis;
        [SerializeField] private CanvasGroup actionButtonCanvas;
        [SerializeField] private Button backButton;
        [SerializeField] private Button showDetailButton;
        [SerializeField] private Button locationButton;
        [SerializeField] private CanvasGroup backButtonCanvas;
        [SerializeField] private ScrollRect scrollRect;

        private UnityAction<int> onQuestPathFind = null;
        
        private YisoQuest quest;

        public int QuestId => quest?.Id ?? -1;
        
        public bool ExistDetail => quest != null;
        
        protected override void Start() {
            base.Start();
            
            showDetailButton.onClick.AddListener(OnClickDetail);
            locationButton.onClick.AddListener(OnClickFind);
            
            backButton.onClick.AddListener(() => OnBackEvent?.Invoke());
        }

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

        public void Visible(bool flag) {
            canvasGroup.Visible(flag.FlagToFloat());
            actionButtonCanvas.Visible(flag.FlagToFloat());
            backButtonCanvas.Visible(flag.FlagToFloat());
        }

        public void Clear() {
            foreach (var item in objectiveItemUis) item.gameObject.SetActive(false);
            quest = null;
            scrollRect.verticalNormalizedPosition = 1f;
        }

        public void UpdateQuest(QuestStatusChangeEventArgs args) {
            if (quest == null || quest.Id != args.QuestId) return;
            if (args.To is YisoQuestStatus.PRE_COMPLETE or YisoQuestStatus.COMPLETE) {
                OnBackEvent?.Invoke();
            }
        }

        public void UpdateQuest(QuestUpdateEventArgs args) {
            if (quest == null || quest.Id != args.QuestId) return;
            if (!TryFindObjectiveItemIndex(args.Index, out var index))
                throw new Exception($"Cannot found objective index({args.Index})");
            
            objectiveItemUis[index].UpdateObjective(args.OriginalTarget, args.UpdateValue, args.IsComplete);
        }

        public void SetQuest(YisoQuest quest) {
            this.quest = quest;
            
            foreach (var item in objectiveItemUis) item.gameObject.SetActive(false);

            var reqs = quest.CompleteRequirements.Select(req => req.GetObjectiveUI(CurrentLocale))
                .ToList();

            for (var i = 0; i < reqs.Count(); i++) {
                var (index, target, value, complete) = reqs[i];
                objectiveItemUis[i].gameObject.SetActive(true);
                objectiveItemUis[i].ShowObjective(index, target, value, complete);
            }
        }

        private void OnClickDetail() {
            YisoServiceProvider.Instance.Get<IYisoUIService>()
                .ShowMenuUI(YisoMenuTypes.QUEST, quest);
        }

        private void OnClickFind() {
            onQuestPathFind?.Invoke(quest.Id);
        }

        private void OnQuestPathFind(UnityAction<int> onQuestPathFind) {
            this.onQuestPathFind = onQuestPathFind;
        }
        
        private bool TryFindObjectiveItemIndex(int objectiveIndex, out int index) {
            index = -1;

            for (var i = 0; i < objectiveItemUis.Count; i++) {
                if (objectiveItemUis[i].Index != objectiveIndex) continue;
                index = i;
                break;
            }

            return index != -1;
        }
    }
}