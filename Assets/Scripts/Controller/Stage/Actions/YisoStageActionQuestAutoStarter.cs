using System.Collections;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Quest;
using Core.Domain.Quest.SO;
using Core.Service;
using Core.Service.Character;
using Core.Service.Temp;
using Manager.Modules;
using UnityEngine;

namespace Controller.Stage.Actions {
    [AddComponentMenu("Yiso/Controller/Quest/StageActionQuestAutoStarter")]
    public class YisoStageActionQuestAutoStarter : YisoStageAction {
        [SerializeField] private YisoQuestSO questSO;
        [SerializeField] private bool showPopup = true;
        [SerializeField] private float startDelay = 1f;

        private int QuestId => questSO.id;
        private IYisoTempService TempService => YisoServiceProvider.Instance.Get<IYisoTempService>();
        private YisoGameQuestModule GameQuestModule => TempService.GetGameManager().GameModules.QuestModule;

        private YisoPlayerQuestModule QuestModule =>
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().QuestModule;

        public override void PerformAction() {
            StartCoroutine(StartQuestCo());
        }

        protected virtual IEnumerator StartQuestCo() {
            if (TempService.GetGameManager() == null || questSO == null) yield break;
            yield return new WaitForSeconds(startDelay);
            if (QuestModule.GetStatusByQuestId(QuestId) != YisoQuestStatus.READY) yield break;
            if (showPopup) GameQuestModule.ShowQuestStartPopup(QuestId);
            else GameQuestModule.StartQuest(QuestId);
        }
    }
}