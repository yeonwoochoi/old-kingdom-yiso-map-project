using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Controller.Holder;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Locale;
using Core.Domain.Quest;
using Core.Domain.Types;
using Core.Logger;
using Core.Service;
using Core.Service.Bounty;
using Core.Service.Character;
using Core.Service.Game;
using Core.Service.Log;
using Core.Service.Scene;
using Core.Service.Stage;
using Core.Service.UI.Popup;
using Cutscene.Scripts.Control.Cutscene;
using Items.Pickable;
using Tools.Cutscene.Conditions;
using Tools.Event;
using UnityEngine.Events;
using Utils.Beagle;

namespace Manager.Modules {
    public class YisoGameQuestModule : YisoGameBaseModule, IYisoEventListener<YisoInGameEvent>,
        IYisoEventListener<YisoFieldEnterEvent>, IYisoEventListener<YisoPickableObjectEvent>,
        IYisoEventListener<YisoCutsceneStateChangeEvent> {
        private YisoPlayerQuestModule QuestModule =>
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().QuestModule;

        private IYisoSceneService SceneService => YisoServiceProvider.Instance.Get<IYisoSceneService>();
        private IYisoBountyService BountyService => YisoServiceProvider.Instance.Get<IYisoBountyService>();
        private IYisoPopupUIService PopupUIService => YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
        private IYisoStageService StageService => YisoServiceProvider.Instance.Get<IYisoStageService>();
        private YisoLocale.Locale CurrentLocale => YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
        private YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoGameQuestModule>();
        
        private YisoLocale questFailPopupTitle = new YisoLocale {
            [YisoLocale.Locale.KR] = "퀘스트 실패",
            [YisoLocale.Locale.EN] = "Quest Failed"
        };
        private YisoLocale questFailPopupContent = new YisoLocale {
            [YisoLocale.Locale.KR] = "다시 시작하시겠습니까?",
            [YisoLocale.Locale.EN] = "Would you like to restart?"
        };

        private List<int> StageIds {
            get {
#if UNITY_EDITOR
                return StageService.GetRelevantStageIds(GameManager.Instance.CurrentStageId);
#else
                return StageService.GetRelevantStageIds();
#endif
            }
        }

        public YisoGameQuestModule(GameManager manager, Settings settings) : base(manager) {
        }

        #region Event Listener

        public void OnEvent(YisoInGameEvent e) {
            if (manager.CurrentGameMode == GameManager.GameMode.Story && e.stage != null) {
                ProcessEventByType(e.stage.Id);
            }
            else if (manager.CurrentGameMode == GameManager.GameMode.Bounty && e.bounty != null) {
                ProcessEventByType(e.bounty.Id);
            }

            void ProcessEventByType(int id) {
                switch (e.eventType) {
                    case YisoInGameEventTypes.StageStart:
                        YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule
                            .CheckStageQuestItemExist(id);
                        break;
                    case YisoInGameEventTypes.StageClear:
                        QuestModule.DrawQuests(id);
                        break;
                }
            }
        }

        public void OnEvent(YisoFieldEnterEvent e) {
            LogService.Debug($"[QuestModule] [Update Quest Req Call] Requirement Type: FIELD ENTER \n FieldID: {e.fieldId}");
            UpdateQuestRequirement(YisoQuestRequirement.Types.FIELD_ENTER, e.fieldId);
        }

        public void OnEvent(YisoPickableObjectEvent e) {
            if (e.pickedItem.TryGetComponent<YisoItemPickableObject>(out var itemPickableObject)) {
                LogService.Debug($"[QuestModule] [Update Quest Req Call] Requirement Type: ITEM \n Item: {itemPickableObject.Item.GetName()}");
                YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule
                    .CheckStageQuestItemExist(StageIds);
            }
        }

        protected virtual void UpdateQuestRequirement(QuestEventArgs args) {
            switch (args) {
                case QuestStatusChangeEventArgs statusChangeEventArgs:
                    if (statusChangeEventArgs.To == YisoQuestStatus.PROGRESS) {
                        // Start Action으로 아이템을 주는 경우도 있어서. (Sub quest 6-1)
                        YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule
                            .CheckStageQuestItemExist(StageIds);
                    }

                    if (statusChangeEventArgs.To == YisoQuestStatus.PRE_COMPLETE) {
                        LogService.Debug($"[QuestModule] [Update Quest Req Call] Requirement Type: PRE_COMPLETE \n QuestID:  \n Details: {args.QuestId} ({args.Quest.GetName(YisoLocale.Locale.KR)})");
                        UpdateQuestRequirement(YisoQuestRequirement.Types.PRE_COMPLETE_QUEST, args.QuestId);
                    }

                    if (statusChangeEventArgs.To == YisoQuestStatus.COMPLETE) {
                        LogService.Debug($"[QuestModule] [Update Quest Req Call] Requirement Type: COMPLETE \n QuestID:  \n Details: {args.QuestId} ({args.Quest.GetName(YisoLocale.Locale.KR)})");
                        UpdateQuestRequirement(YisoQuestRequirement.Types.COMPLETE_QUEST, args.QuestId);

                        if (args.Quest.AutoComplete) {
                            ExecuteWithDelay(1.5f, () => AutoCompleteQuest(args.QuestId));
                        }
                    }

                    break;
            }

            async void ExecuteWithDelay(float delay, Action action) {
                await Task.Delay((int) (delay * 1000));
                action?.Invoke();
            }

            void AutoCompleteQuest(int questId) {
                PopupUIService.ShowQuestCompletePopup(questId, GameManager.Instance.DeathCount, () => { });
            }
        }

        public void OnEvent(YisoCutsceneStateChangeEvent cutsceneStateChangeEvent) {
        }

        #endregion

        #region Public API

        public virtual void UpdateQuestRequirement(YisoQuestRequirement.Types type, object value) {
            QuestModule.UpdateQuestRequirement(GameManager.Instance.CurrentStageId, type, value);
        }

        public virtual void ShowQuestStartPopup(int questId, UnityAction onClickOk = null,
            UnityAction onClickCancel = null) {
            PopupUIService.ShowQuestStartPopup(questId, () => StartQuest(questId, onClickOk),
                () => StartQuestFailed(questId, onClickCancel));
        }

        public virtual void ShowQuestCompletePopup(int questId, UnityAction onClickOk = null,
            UnityAction onClickCancel = null) {
            PopupUIService.ShowQuestCompletePopup(questId, GameManager.Instance.DeathCount,
                () => CompleteQuest(questId, onClickOk), () => CompleteQuestFailed(questId, onClickCancel));
        }

        public virtual void ShowQuestFailPopup(int questId, UnityAction onClickOk = null,
            UnityAction onClickCancel = null) {
            PopupUIService.AlertS(questFailPopupTitle[CurrentLocale], questFailPopupContent[CurrentLocale],
                () => FailQuest(questId, onClickOk), () => DrawQuest(questId, onClickCancel));
        }

        public void StartQuest(int questId, UnityAction callback = null) {
            QuestModule.StartQuest(StageIds, questId);
            callback?.Invoke();
        }

        public void CompleteQuest(int questId, UnityAction callback = null) {
            QuestModule.CompleteQuest(questId);
            callback?.Invoke();
        }

        public void FailQuest(int questId, UnityAction callback = null) {
            QuestModule.ChangeQuestStatus(questId, YisoQuestStatus.IDLE);
            callback?.Invoke();
        }

        public void DrawQuest(int questId, UnityAction callback = null) {
            QuestModule.ChangeQuestStatus(questId, YisoQuestStatus.READY);
            callback?.Invoke();
        }

        #endregion

        #region Private API

        private void StartQuestFailed(int questId, UnityAction callback = null) {
            LogService.Error($"[QuestModule] [Start Quest Fail] Quest ID ({questId}) Start Failed");
            callback?.Invoke();
        }

        private void CompleteQuestFailed(int questId, UnityAction callback = null) {
            SpawnQuestRewardBox(questId);
            callback?.Invoke();
        }

        private void SpawnQuestRewardBox(int questId) {
            var questBoxPrefab = manager.GameModules.AssetModule.QuestRewardPrefab;
            var player = GameManager.Instance.Player;
            if (player == null || questBoxPrefab == null) return;

            var questBox = YisoGameObjectUtils.Instantiate(questBoxPrefab,
                YisoPhysicsUtils.GetRandomPositionInRadius(player.transform.position, 1f, 3f),
                UnityEngine.Quaternion.identity);
            var questBoxController = questBox.GetComponent<YisoQuestRewardBoxHoldingController>();
            questBoxController.Initialization(questId);
        }
        
        private void OnSceneChanged(YisoSceneTypes beforeScene, YisoSceneTypes afterScene) {
            switch (beforeScene) {
                case YisoSceneTypes.STORY:
                    QuestModule.DrawStage(StageService.GetCurrentStageId());
                    break;
                case YisoSceneTypes.BOUNTY:
                    BountyService.DrawBounty();
                    break;
            }
        }

        #endregion

        public override void OnEnabled() {
            base.OnEnabled();
            QuestModule.OnQuestEvent += UpdateQuestRequirement;
            SceneService.RegisterOnSceneChanged(OnSceneChanged);
            this.YisoEventStartListening<YisoFieldEnterEvent>();
            this.YisoEventStartListening<YisoInGameEvent>();
            this.YisoEventStartListening<YisoPickableObjectEvent>();
            this.YisoEventStartListening<YisoCutsceneStateChangeEvent>();
        }

        public override void OnDisabled() {
            base.OnDisabled();
            QuestModule.OnQuestEvent -= UpdateQuestRequirement;
            SceneService.UnregisterOnSceneChanged(OnSceneChanged);
            this.YisoEventStopListening<YisoFieldEnterEvent>();
            this.YisoEventStopListening<YisoInGameEvent>();
            this.YisoEventStopListening<YisoPickableObjectEvent>();
            this.YisoEventStopListening<YisoCutsceneStateChangeEvent>();
        }

        [Serializable]
        public class Settings {
        }
    }
}