using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Locale;
using UnityEngine;

namespace Core.UI.Story {
    [CreateAssetMenu(fileName = "StoryLoadingComments", menuName = "Yiso/UI/Story/Loading Comments")]
    public class YisoStoryLoadingCommentsSO : ScriptableObject {
        public List<Comment> comments;

        public YisoStoryLoadingComments CreateComments() => new(this);
        
        [Serializable]
        public class Comment {
            public int stage;
            public List<YisoLocale> comments;
        }
    }

    public class YisoStoryLoadingComments {
        public Dictionary<int, List<YisoLocale>> Comments { get; }

        public YisoStoryLoadingComments(YisoStoryLoadingCommentsSO so) {
            Comments = so.comments.ToDictionary(keySelector: c => c.stage, elementSelector: c => c.comments);
        }
    }
}