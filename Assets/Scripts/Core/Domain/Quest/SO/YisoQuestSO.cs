using System.Collections.Generic;
using Core.Domain.Locale;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Quest.SO {
    [CreateAssetMenu(fileName = "Quest", menuName = "Yiso/Quests/Quest")]
    public class YisoQuestSO : ScriptableObject {
        [Title("Basic")] public int id;
        public YisoQuest.Types type;
        public new YisoLocale name;
        
        public bool autoComplete;

        [Title("Requirements")] public List<YisoQuestRequirementSO> startRequirements = default;
        public List<YisoQuestRequirementSO> completeRequirements = default;

        [Title("Actions")] public List<YisoQuestActionSO> startActions;
        public List<YisoQuestActionSO> completeActions;
        
        public YisoQuest CreateQuest() => new (this);
    }
}