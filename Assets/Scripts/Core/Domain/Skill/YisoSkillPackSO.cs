using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Domain.Skill {
    [CreateAssetMenu(fileName = "SkillPack", menuName = "Yiso/Skill/Pack")]
    public class YisoSkillPackSO : ScriptableObject {
        public List<YisoSkillSO> skills;

        public Dictionary<int, YisoSkill> CreateDictionary() => skills.ToDictionary(s => s.id, s => s.CreateSkill());
    }
}