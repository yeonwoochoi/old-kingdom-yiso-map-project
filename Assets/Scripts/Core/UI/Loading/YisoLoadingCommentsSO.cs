using System.Collections.Generic;
using System.Linq;
using Core.Domain.Locale;
using UnityEngine;
using Utils;

namespace Core.UI.Loading {
    [CreateAssetMenu(fileName = "LoadingComments", menuName = "Yiso/UI/Loading/Comments")]
    public class YisoLoadingCommentsSO : ScriptableObject {
        public List<YisoLocale> comment;

        public string GetRandomComment(YisoLocale.Locale locale) => comment.TakeRandom(1).First()[locale];
    }
}