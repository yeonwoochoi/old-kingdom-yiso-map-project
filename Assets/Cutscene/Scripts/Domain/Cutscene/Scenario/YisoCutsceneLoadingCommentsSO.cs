using System;
using System.Collections.Generic;
using Core.Domain.Locale;
using UnityEngine;

namespace Cutscene.Scripts.Domain.Cutscene
{
    
    [CreateAssetMenu(fileName = "Loading comments", menuName = "Yiso/Cutscene/Loading comments")]
    public class YisoCutsceneLoadingCommentsSO: ScriptableObject
    {
        public List<LoadingComment> loadingComments;

        public YisoCutsceneLoadingComments CreateYisoCutsceneLoadingComments() => new YisoCutsceneLoadingComments(this);
        
        
        [Serializable]
        public class LoadingComment
        {
            public YisoLocale comment = new YisoLocale();
            public YisoLocale readingTime = new YisoLocale();
        }
    }

    public class YisoCutsceneLoadingComments
    {
        public List<LoadingComment> comments;
        public YisoCutsceneLoadingComments(YisoCutsceneLoadingCommentsSO so)
        {
            comments = new List<LoadingComment>();
            foreach (var comment in so.loadingComments)
            {
                comments.Add(new LoadingComment(comment));
            }
        }
    }

    public class LoadingComment
    {
        public YisoLocale comment;
        public YisoLocale readingTime;

        public LoadingComment(YisoCutsceneLoadingCommentsSO.LoadingComment so)
        {
            comment = so.comment;
            readingTime = so.readingTime;
        }
    }
}