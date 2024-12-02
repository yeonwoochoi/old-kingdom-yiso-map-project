using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Domain.Bounty {
    [CreateAssetMenu(fileName = "BountyPack", menuName = "Yiso/Bounty/Pack")]
    public class YisoBountyPackSO : ScriptableObject {
        public List<YisoBountySO> bountySos;

        public Dictionary<int, YisoBounty> ToDictionary() =>
            bountySos.ToDictionary(so => so.id, so => so.CreateBounty());
    }
}