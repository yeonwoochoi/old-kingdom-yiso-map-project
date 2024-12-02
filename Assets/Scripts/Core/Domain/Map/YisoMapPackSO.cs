using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Domain.Map {
    [CreateAssetMenu(fileName = "MapPack", menuName = "Yiso/Map/Pack")]
    public class YisoMapPackSO : ScriptableObject {
        public List<YisoMapSO> mapSOs;

        public Dictionary<int, YisoMap> CreateDict() =>
            mapSOs.ToDictionary(keySelector: so => so.id, elementSelector: so => so.CreateMap());
    }
}