using System.Collections.Generic;
using System.Linq;
using Core.Domain.Quest;
using Core.Domain.Quest.SO;
using UnityEngine;

namespace Core.Domain.Stage {
    [CreateAssetMenu(fileName = "Stage", menuName = "Yiso/Stage/Stage")]
    public class YisoStageSO : ScriptableObject {
        public int id;
        public List<YisoQuestSO> mainQuests;
        public List<YisoStageSO> relevantStages;
        public YisoStage CreateStage() => new(this);
    }

    public class YisoStage {
        public int Id { get; }
        public List<int> RelevantStageIds { get; }
        
        public double CombatRating { get; set; }
        
        public double CRDiff { get; set; }
        
        public List<YisoQuest> MainQuests { get; }
        public YisoStage(YisoStageSO so) {
            Id = so.id;
            MainQuests = so.mainQuests.Select(quest => quest.CreateQuest()).ToList();
            RelevantStageIds = so.relevantStages.Select(s => s.id).Distinct().ToList();
            RelevantStageIds.Add(Id);
        }

        public void SetCRDiff() {
            CRDiff = 100 + 10 * Id;
        }
    }
}