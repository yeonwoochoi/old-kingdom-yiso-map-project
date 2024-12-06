using System;
using System.Linq;
using Controller.Emoticon;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Quest;
using Core.Domain.Quest.SO;
using Core.Domain.Stage;
using Core.Service;
using Core.Service.Character;
using Sirenix.OdinInspector;
using Tools.Cutscene;
using Tools.Event;
using Tools.StateMachine;
using UnityEngine;

namespace Controller.Holder {
    [AddComponentMenu("Yiso/Controller/Holder/QuestHoldingController")]
    public class YisoQuestHoldingController : YisoHoldingController, IYisoEventListener<YisoInGameEvent> {
        public enum QuestNpcState {
            Idle,
            Ready,
            StartCutscene,
            InProgress,
            PreComplete,
            EndCutscene,
            Complete
        }

        public YisoQuestSO questSO;
        public bool isStartNpc;
        public bool isEndNpc;
        [ShowIf("isStartNpc")] public YisoCutsceneTrigger startCutsceneTrigger;
        [ShowIf("isEndNpc")] public YisoCutsceneTrigger endCutsceneTrigger;

        private int QuestId => questSO.id;
        private YisoPlayerQuestModule QuestModule => YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().QuestModule;
        public override YisoEmotionController.EmotionType EmotionType => YisoEmotionController.EmotionType.EMPTY;

        private YisoStateMachine<QuestNpcState> questNpcState;
        private bool isQuestInCurrentStage;

        protected override void Initialization() {
            if (initialized) return;
            base.Initialization();

            // Initialize npc state
            questNpcState = new YisoStateMachine<QuestNpcState>(gameObject, true);

            RegisterCutsceneCallback();

            alwaysShowEmoticons = true; // 나중에 리팩토링 할때.. 강제성 지우기
            initialized = true;
        }
        
        private void RegisterCutsceneCallback() {
            if (isStartNpc) startCutsceneTrigger.AddStartCallback(() => questNpcState.ChangeState(QuestNpcState.StartCutscene));
            if (isEndNpc) endCutsceneTrigger.AddStartCallback(() => questNpcState.ChangeState(QuestNpcState.EndCutscene));
        }

        private void CheckQuestInCurrentStage(YisoStage currentStage) {
            isQuestInCurrentStage = currentStage.MainQuests.Any(mainQuest => mainQuest.Id == QuestId);
            if (!isQuestInCurrentStage) HideInteractButton();
        }

        #region Check

        protected override bool CanInteract() => base.CanInteract() && isQuestInCurrentStage && CheckNpcStateForInteraction();
        
        protected override bool CanInteractByMouseClick() {
            return useMouseClickInteractionOnDesktop && !isMobile;
        }
        
        private bool CheckNpcStateForInteraction() {
            return questNpcState.CurrentState switch {
                QuestNpcState.Idle or QuestNpcState.Ready when isStartNpc && startCutsceneTrigger.CanPlay() => true,
                QuestNpcState.PreComplete when isEndNpc && endCutsceneTrigger.CanPlay() => true,
                _ => false
            };
        }

        protected override bool CanDisplayEmoticon() {
            return isQuestInCurrentStage && base.CanDisplayEmoticon();
        }
        

        #endregion

        #region Emoticon

        protected override void ShowEmoticonIfConditionsMet() {
            if (!CanDisplayEmoticon()) return;

            var emotionType = GetEmoticonType();
            if (emotionType == null) return;

            emotionController.ShowEmoticon(emotionType.Value, GetStopPredicate(), () => { isShowingEmoticon = false; });
            isShowingEmoticon = true;
        }

        private YisoEmotionController.EmotionType? GetEmoticonType() {
            return questNpcState.CurrentState switch {
                QuestNpcState.Idle or QuestNpcState.Ready when isStartNpc && startCutsceneTrigger.CanPlay() => 
                    YisoEmotionController.EmotionType.PROGRESS_1,
                QuestNpcState.InProgress => 
                    YisoEmotionController.EmotionType.PROGRESS_2,
                QuestNpcState.PreComplete when isEndNpc && endCutsceneTrigger.CanPlay() => 
                    YisoEmotionController.EmotionType.PROGRESS_3,
                _ => null
            };
        }
        
        private Func<bool> GetStopPredicate() {
            if (!CanDisplayEmoticon()) return () => false;
            return () => questNpcState.CurrentState switch {
                QuestNpcState.Idle or QuestNpcState.Ready => questNpcState.CurrentState != QuestNpcState.Idle && questNpcState.CurrentState != QuestNpcState.Ready,
                QuestNpcState.InProgress => questNpcState.CurrentState != QuestNpcState.InProgress,
                QuestNpcState.PreComplete => questNpcState.CurrentState != QuestNpcState.PreComplete,
                _ => true
            };
        }

        #endregion

        #region Event

        public void OnEvent(YisoInGameEvent e) {
            if (e.stage == null) return;
            if (e.eventType is YisoInGameEventTypes.StageStart or YisoInGameEventTypes.MoveNextStage) {
                CheckQuestInCurrentStage(e.stage);
            }
        }

        #endregion

        #region Button
        
        protected override void PerformInteraction() {
            switch (questNpcState.CurrentState) {
                case QuestNpcState.Idle or QuestNpcState.Ready:
                    if (isStartNpc && startCutsceneTrigger.CanPlay()) startCutsceneTrigger.Play();
                    break;
                case QuestNpcState.PreComplete:
                    if (isEndNpc && endCutsceneTrigger.CanPlay()) endCutsceneTrigger.Play();
                    break;
            }
        }

        #endregion

        protected override void OnEnable() {
            base.OnEnable();
            this.YisoEventStartListening();
            QuestModule.OnQuestEvent += CheckQuestStateChanged;
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.YisoEventStopListening();
            QuestModule.OnQuestEvent -= CheckQuestStateChanged;
        }
        
        private void CheckQuestStateChanged(QuestEventArgs args) {
            if (args.QuestId != QuestId || !initialized) return;

            if (args is QuestStatusChangeEventArgs statusChangeEventArgs) {
                var state = statusChangeEventArgs.To switch {
                    YisoQuestStatus.IDLE => QuestNpcState.Idle,
                    YisoQuestStatus.READY => QuestNpcState.Ready,
                    YisoQuestStatus.PROGRESS => QuestNpcState.InProgress,
                    YisoQuestStatus.PRE_COMPLETE => QuestNpcState.PreComplete,
                    YisoQuestStatus.COMPLETE => QuestNpcState.Complete,
                    _ => questNpcState.CurrentState
                };
                questNpcState.ChangeState(state);
            }
        }
    }
}