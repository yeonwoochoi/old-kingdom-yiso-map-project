using System.Collections.Generic;
using Core.Domain.Cabinet;
using Core.Domain.Dialogue;
using Core.Domain.Direction;
using Core.Domain.Locale;
using Core.Domain.Quest;
using Core.Domain.Skill;
using Core.Domain.Wanted;
using UnityEngine;

namespace Core.Service.Domain {
    public interface IYisoDomainService : IYisoService {
        public List<YisoQuest> GetQuests();
        public bool TryGetQuest(int questId, out YisoQuest quest);
        public YisoCabinet GetCabinetByIdElseThrow(int cabinetId);
        public YisoDialogue GetDialogueByIdElseThrow(int dialogueId);
        public YisoGameDirection GetGameDirectionByIdOrElseThrow(int id);
        public List<YisoWanted> GetWantedList();
        public Sprite GetMoneyIcon();
        public Sprite GetExpIcon();
        public Sprite GetCombatRatingIcon();
        public string GetStoryLoadingComment(int stage, YisoLocale.Locale locale);
        public string GetStageFlowComment(int stage, YisoLocale.Locale locale);
        public IReadOnlyDictionary<int, YisoSkill> GetSkills();
    }
}