using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Actor.Npc;
using Core.Domain.Locale;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Dialogue {
    [CreateAssetMenu(fileName = "Dialogue", menuName = "Yiso/Dialogue/Dialogue")]
    public class YisoDialogueSO : ScriptableObject {
        public int id;
        public List<Dialogue> dialogues;
        
        [Serializable]
        public class Dialogue {
            public Speakers speaker = Speakers.OTHER_ACTOR;
            [ShowIf("speaker", Speakers.OTHER_ACTOR)]
            public YisoNpcSO npc;
            [Required]
            public YisoLocale content;

            public enum Speakers {
                YISO, ERRY, OTHER_ACTOR
            }
        }

        public YisoDialogue CreateDialogue(Sprite yisoIcon, Sprite erryIcon) => new(this, yisoIcon, erryIcon);
    }

    public class YisoDialogue {
        public int Id { get; }
        
        public List<Dialogue> Dialogues { get; }

        public YisoDialogue(YisoDialogueSO so, Sprite yisoIcon, Sprite erryIcon) {
            Id = so.id;
            Dialogues = new List<Dialogue>(so.dialogues.Select(d => new Dialogue(d, yisoIcon, erryIcon)));
        }

        public class Dialogue {
            public bool IsYiso { get; }
            public Sprite Icon { get; set; }
            private readonly YisoLocale speaker;
            private readonly YisoLocale content;

            public Dialogue(YisoDialogueSO.Dialogue dialogue, Sprite yisoIcon, Sprite erryIcon) {
                IsYiso = dialogue.speaker == YisoDialogueSO.Dialogue.Speakers.YISO;
                content = dialogue.content;
                switch (dialogue.speaker) {
                    case YisoDialogueSO.Dialogue.Speakers.YISO:
                        speaker = new YisoLocale {
                            kr = "이소",
                            en = "Yiso"
                        };
                        Icon = yisoIcon;
                        break;
                    case YisoDialogueSO.Dialogue.Speakers.ERRY:
                        speaker = new YisoLocale {
                            kr = "어리",
                            en = "Erry"
                        };
                        Icon = erryIcon;
                        break;
                    case YisoDialogueSO.Dialogue.Speakers.OTHER_ACTOR:
                        Icon = dialogue.npc.icon;
                        speaker = dialogue.npc.name;
                        break;
                }
            }

            public string GetSpeaker(YisoLocale.Locale locale) => speaker[locale];

            public string GetContent(YisoLocale.Locale locale) => content[locale];
        }
    }
}