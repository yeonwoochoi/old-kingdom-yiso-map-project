using UnityEngine;
using Core.Domain.Locale;

namespace Cutscene.Scripts.Domain.Cutscene.Scenario
{
    [CreateAssetMenu(fileName = "Chat Bubble", menuName = "Yiso/Cutscene/Chat Bubble")]
    public class YisoCutsceneChatBubbleSO: ScriptableObject
    {
        public YisoLocale comment;

        public YisoCutsceneChatBubble CreateYisoCutsceneChatBubble() => new YisoCutsceneChatBubble(this);
    }

    public class YisoCutsceneChatBubble
    {
        public YisoLocale comment;

        public YisoCutsceneChatBubble(YisoCutsceneChatBubbleSO so)
        {
            comment = so.comment;
        }
    }
}