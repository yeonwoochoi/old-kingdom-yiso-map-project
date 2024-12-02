using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Cabinet;
using Core.Domain.Data;
using Core.Domain.Dialogue;
using Core.Domain.Direction;
using Core.Domain.Locale;
using Core.Domain.Quest;
using Core.Domain.Quest.SO;
using Core.Domain.Skill;
using Core.Domain.Stage;
using Core.Domain.Wanted;
using Core.UI.Story;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Service.Domain {
    public class YisoDomainService : IYisoDomainService {
        private readonly Dictionary<int, YisoQuest> quests;
        private readonly Dictionary<int, YisoWanted> wanteds;
        private readonly Dictionary<int, YisoCabinet> cabinets;
        private readonly Dictionary<int, YisoDialogue> dialogues;
        private readonly Dictionary<int, YisoGameDirection> directions;
        private readonly Dictionary<int, YisoStageFlow> flows;
        private readonly Dictionary<int, YisoSkill> skills;
        

        private readonly YisoStoryLoadingComments storyLoadingComments;

        private readonly Sprite moneySprite;
        private readonly Sprite expSprite;
        private readonly Sprite combatRatingSprite;

        private readonly YisoGameData gameData;

        private YisoDomainService(Settings settings) {
            quests = settings.questPackSO.questSOs.Select(q => q.CreateQuest()).ToDictionary(q => q.Id, q => q);

            wanteds = settings.wantedPackSO.CreateDict();

            cabinets = settings.cabinetPackSO.ToDictionary();
            dialogues = settings.dialoguePackSO.ToDictionary(settings.yisoIcon, settings.erryIcon);
            directions = settings.directionPackSO.ToDictionary();
            flows = settings.flowsSO.CreateDictionary();
            skills = settings.skillPackSO.CreateDictionary();
            
            storyLoadingComments = settings.storyLoadingCommentsSO.CreateComments();

            moneySprite = settings.moneySprite;
            expSprite = settings.expSprite;
            combatRatingSprite = settings.combatRatingSprite;
        }
        
        #region DOMAIN_DATA
        
        public YisoCabinet GetCabinetByIdElseThrow(int cabinetId) {
            if (!cabinets.TryGetValue(cabinetId, out var cabinet))
                throw new ArgumentException($"Cabinet(id={cabinetId}) Not Found");
            return cabinet;
        }

        public YisoDialogue GetDialogueByIdElseThrow(int dialogueId) {
            if (!dialogues.TryGetValue(dialogueId, out var dialogue))
                throw new ArgumentException($"Dialogue(id={dialogueId}) not found!");
            return dialogue;
        }

        public YisoGameDirection GetGameDirectionByIdOrElseThrow(int id) {
            if (!directions.TryGetValue(id, out var direction))
                throw new ArgumentException($"Direction(id={id}) not found!");
            return direction;
        }

        public string GetStoryLoadingComment(int stage, YisoLocale.Locale locale) {
            if (stage is < 1 or > 100)
                throw new ArgumentOutOfRangeException(nameof(stage), "Stage must be ranged in 1 to 100");
            var comments = storyLoadingComments.Comments[stage];
            return comments.TakeRandom(1).First()[locale];
        }

        public string GetStageFlowComment(int stage, YisoLocale.Locale locale) {
            if (stage is < 1 or > 100)
                throw new ArgumentOutOfRangeException(nameof(stage), $"Stage must be ranged in 1 to 100 not '{stage}'");
            return flows[stage].Content[locale];
        }

        public List<YisoQuest> GetQuests() => quests.Values.ToList();

        public bool TryGetQuest(int questId, out YisoQuest quest) => quests.TryGetValue(questId, out quest);

        public List<YisoWanted> GetWantedList() => wanteds.Values.ToList();

        public IReadOnlyDictionary<int, YisoSkill> GetSkills() => skills;

        #endregion
        
        #region NORMAL_DATA

        public Sprite GetMoneyIcon() => moneySprite;


        public Sprite GetExpIcon() => expSprite;

        public Sprite GetCombatRatingIcon() => combatRatingSprite;

        #endregion

        public bool IsReady() => true;
        public void OnDestroy() { }

        public static YisoDomainService CreateService(Settings settings) => new(settings);

        [Serializable]
        public class Settings {
            [Title("Quests")] public YisoQuestPackSO questPackSO;
            [Title("Wanted")] public YisoWantedPackSO wantedPackSO;
            [Title("Story Loading Comments")] public YisoStoryLoadingCommentsSO storyLoadingCommentsSO;
            [Title("Stage Flow Comments")] public YisoStageFlowsSO flowsSO;
            [Title("Skills")] public YisoSkillPackSO skillPackSO;
            [Title("Constant Sprites")] public Sprite moneySprite;
            public Sprite expSprite;
            public Sprite combatRatingSprite;
            [Title("Cabinet")] public YisoCabinetPackSO cabinetPackSO;
            [Title("Dialogue")] public YisoDialoguePackSO dialoguePackSO;
            [Title("Direction")] public YisoGameDirectionPackSO directionPackSO;
            [Title("Icons")] public Sprite yisoIcon;
            public Sprite erryIcon;
        }
    }
}