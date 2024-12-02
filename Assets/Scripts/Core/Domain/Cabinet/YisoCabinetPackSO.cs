using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Domain.Cabinet {
    [CreateAssetMenu(fileName = "CabinetPack", menuName = "Yiso/Cabinet/Pack")]
    public class YisoCabinetPackSO : ScriptableObject {
        public List<YisoCabinetSO> cabinetSos;

        public Dictionary<int, YisoCabinet> ToDictionary() =>
            cabinetSos.ToDictionary(c => c.id, c => c.CreateCabinet());
    }
}