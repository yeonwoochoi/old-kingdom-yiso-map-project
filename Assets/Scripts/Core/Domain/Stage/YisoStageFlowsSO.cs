using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Locale;
using UnityEngine;

namespace Core.Domain.Stage {
    [CreateAssetMenu(fileName = "Stage Flows", menuName = "Yiso/Stage/Flows")]
    public class YisoStageFlowsSO : ScriptableObject {
        public List<Content> contents;
        
        [Serializable]
        public class Content {
            public int stage;
            public YisoLocale flow;
        }

        public Dictionary<int, YisoStageFlow> CreateDictionary() =>
            contents.ToDictionary(c => c.stage, c => new YisoStageFlow(c));
    }

    public class YisoStageFlow {
        public int StageId { get; }
        public YisoLocale Content { get; }

        public YisoStageFlow(YisoStageFlowsSO.Content content) {
            StageId = content.stage;
            Content = content.flow;
        }
    }
}