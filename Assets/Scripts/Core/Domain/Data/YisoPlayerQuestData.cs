using System;
using System.Collections.Generic;

namespace Core.Domain.Data {
    [Serializable]
    public class YisoPlayerQuestData {
        public List<int> completeQuestIds = new();
    }

    [Serializable]
    public class YisoPlayerProgressQuestData {
        public int id;
        
    }

    [Serializable]
    public class YisoPlayerProgressQuestRequirementData {
        public int index;
        
    }
}