using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Domain.Wanted {
    [CreateAssetMenu(fileName = "WantedPack", menuName = "Yiso/Wanted/Pack")]
    public class YisoWantedPackSO : ScriptableObject {
        public List<YisoWantedSO> wantedList;

        public Dictionary<int, YisoWanted> CreateDict() => wantedList.ToDictionary(keySelector: w => w.id, elementSelector: w => w.CreateWanted());
    }
}