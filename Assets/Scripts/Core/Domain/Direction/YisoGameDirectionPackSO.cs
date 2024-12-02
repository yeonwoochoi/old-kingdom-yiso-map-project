using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Domain.Direction {
    [CreateAssetMenu(fileName = "GameDirectionPack", menuName = "Yiso/Game/DirectionPack")]
    public class YisoGameDirectionPackSO : ScriptableObject {
        public List<YisoGameDirectionSO> directionSos;

        public Dictionary<int, YisoGameDirection> ToDictionary() =>
            directionSos.ToDictionary(d => d.id, d => d.CreateDirection());
    }
}