using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Domain.Stage {
    [CreateAssetMenu(fileName = "StagePack", menuName = "Yiso/Stage/Pack")]
    public class YisoStagePackSO : ScriptableObject {
        public List<YisoStageSO> stageSOs;

        public Dictionary<int, YisoStage> CreateDict() =>
            stageSOs.ToDictionary(keySelector: so => so.id, elementSelector: so => so.CreateStage());
    }
}