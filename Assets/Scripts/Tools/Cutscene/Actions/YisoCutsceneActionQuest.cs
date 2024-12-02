using System;
using System.Collections;
using Core.Domain.Quest.SO;
using Core.Service;
using Core.Service.Temp;
using Manager_Temp_.Modules;
using UnityEngine;

namespace Tools.Cutscene.Actions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Action/CutsceneActionQuest")]
    public class YisoCutsceneActionQuest : YisoCutsceneAction {
        public enum QuestActionType {
            Start, End, Fail, Draw, Restart
        }

        [SerializeField] private QuestActionType questActionType;
        [SerializeField] private YisoQuestSO questSO;
        [SerializeField] private bool showPopup = true;
        [SerializeField] private float delay = 0f;

        private int QuestId => questSO.id;
        private IYisoTempService TempService => YisoServiceProvider.Instance.Get<IYisoTempService>();
        private YisoGameQuestModule QuestModule => TempService.GetGameManager().GameModules.QuestModule;

        public override void PerformAction() {
            if (TempService.GetGameManager() == null) return;
            if (delay > 0f) {
                StartCoroutine(TriggerQuestCo());
            }
            else {
                TriggerQuest();
            }
        }

        protected virtual IEnumerator TriggerQuestCo() {
            yield return new WaitForSeconds(delay);
            TriggerQuest();
        }

        protected virtual void TriggerQuest() {
            switch (questActionType) {
                case QuestActionType.Start:
                    if (showPopup) QuestModule.ShowQuestStartPopup(QuestId);
                    else QuestModule.StartQuest(QuestId);
                    break;
                case QuestActionType.End:
                    if (showPopup) QuestModule.ShowQuestCompletePopup(QuestId);
                    else QuestModule.CompleteQuest(QuestId);
                    break;
                case QuestActionType.Fail:
                    if (showPopup) QuestModule.ShowQuestFailPopup(QuestId);
                    else QuestModule.FailQuest(QuestId);
                    break;
                case QuestActionType.Draw:
                    QuestModule.DrawQuest(QuestId);
                    break;
                case QuestActionType.Restart:
                    QuestModule.DrawQuest(QuestId);
                    QuestModule.StartQuest(QuestId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}