using System.Collections.Generic;
using UnityEngine;

namespace Core.Domain.Quest.SO {
    [CreateAssetMenu(fileName = "QuestPack", menuName = "Yiso/Quests/Pack")]
    public class YisoQuestPackSO : ScriptableObject {
        public List<YisoQuestSO> questSOs;
    }
}