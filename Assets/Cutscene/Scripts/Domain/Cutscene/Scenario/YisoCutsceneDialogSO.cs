using System;
using System.Collections.Generic;
using Core.Domain.Locale;
using UnityEngine;

namespace Cutscene.Scripts.Domain.Cutscene.Scenario
{
    public enum UIPosition
    {
        TopLeft, BottomRight, TopRight, BottomLeft
    }
    
    [CreateAssetMenu(fileName = "Dialog", menuName = "Yiso/Cutscene/Dialog")]
    public class YisoCutsceneDialogSO : ScriptableObject {
        public List<Message> messages = new List<Message>();

        public YisoCutsceneDialog CreateCutSceneDialog() => new YisoCutsceneDialog(this);

        [Serializable]
        public class Message
        {
            public YisoCutSceneCharacterInfoSO characterSo;
            public bool pause = true;
            public UIPosition uiPosition;
            public YisoLocale message = new YisoLocale();
            public YisoLocale readingTime = new YisoLocale();
        }
    }

    public class YisoCutsceneDialog {
        public List<Message> messages;

        public YisoCutsceneDialog(YisoCutsceneDialogSO so)
        {
            messages = new List<Message>();
            foreach (var dialog in so.messages)
            {
                messages.Add(new Message(dialog));
            }
        }
    }

    public class Message
    {
        public Sprite icon;
        public UIPosition uiPosition;
        public YisoLocale name;
        public YisoLocale message;
        public YisoLocale readingTime;
        public bool pause;

        public Message(YisoCutsceneDialogSO.Message message)
        {
            this.message = message.message;
            icon = message.characterSo.characterIcon;
            uiPosition = message.uiPosition;
            name = message.characterSo.name;
            readingTime = message.readingTime;
            pause = message.pause;
        }
    }
}