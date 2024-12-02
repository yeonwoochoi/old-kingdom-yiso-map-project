using System;
using System.Collections.Generic;

namespace Core.Domain.Data {
    [Serializable]
    public class YisoPlayerSkillData {
        public Dictionary<int, int> skillData = new();
        public int skillPoint = 0;
    }
}