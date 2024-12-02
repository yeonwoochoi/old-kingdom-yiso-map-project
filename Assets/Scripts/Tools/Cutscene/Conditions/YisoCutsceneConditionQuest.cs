using Character.Ability;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Item;
using Core.Domain.Quest.SO;
using Core.Service;
using Core.Service.Character;
using UnityEngine;

namespace Tools.Cutscene.Conditions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Condition/CutsceneConditionQuest")]
    public class YisoCutsceneConditionQuest : YisoCutsceneCondition {
        [SerializeField] private YisoQuestSO questSO;
        [SerializeField] private bool isStart;

        private YisoCharacterStat enemies;
        private YisoItemSO items;

        private int QuestId => questSO.id;

        private YisoPlayerQuestModule QuestModule =>
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().QuestModule;

        public override bool CanPlay() {
            return isStart
                ? QuestModule.IsStartRequirementAllDone(QuestId)
                : QuestModule.IsCompleteRequirementAllDone(QuestId);
        }
    }
}