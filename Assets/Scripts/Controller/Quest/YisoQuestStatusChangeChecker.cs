using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Locale;
using Core.Logger;
using Core.Service;
using Core.Service.Character;
using Core.Service.Log;
using UnityEngine;

namespace Controller.Quest {
    public class YisoQuestStatusChangeChecker : MonoBehaviour {
        public YisoPlayerQuestModule QuestModule =>
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().QuestModule;
        private YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoQuestStatusChangeChecker>();

        protected virtual void UpdateQuestRequirement(QuestEventArgs args) {
            switch (args) {
                case QuestStatusChangeEventArgs statusChangeEventArgs:
                    LogService.Debug($"[YisoQuestStatusChangeChecker] [Quest Status Change] QuestID: {args.QuestId} ({args.Quest.GetName(YisoLocale.Locale.KR)}) \n Status: {statusChangeEventArgs.To}");
                    break;
                case QuestUpdateEventArgs updateEventArgs:
                    LogService.Debug($"[YisoQuestStatusChangeChecker] [Update Quest Req Result] QuestID: {args.QuestId} ({args.Quest.GetName(YisoLocale.Locale.KR)}) \n Result: {updateEventArgs.ToString()}");
                    break;
            }
        }

        protected virtual void OnEnable() {
            QuestModule.OnQuestEvent += UpdateQuestRequirement;
        }

        protected virtual void OnDisable() {
            QuestModule.OnQuestEvent -= UpdateQuestRequirement;
        }
    }
}