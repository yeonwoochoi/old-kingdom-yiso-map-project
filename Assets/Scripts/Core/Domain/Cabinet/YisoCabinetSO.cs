using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Locale;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Cabinet {
    [CreateAssetMenu(fileName = "Cabinet", menuName = "Yiso/Cabinet/Cabinet")]
    public class YisoCabinetSO : ScriptableObject {
        public int id;
        [InfoBox("This contents is deprecated, using 'cabinetContents'", InfoMessageType.Warning)]
        internal List<YisoLocale> contents = new();
        public List<Content> cabinetContents;

        public YisoCabinet CreateCabinet() => new(this);

        [Serializable]
        public class Content {
            public bool text = true;
            [ShowIf("text")] public YisoLocale textContent;
            [HideIf("text"), PreviewField] public Sprite imageContent;
        }
    }

    public class YisoCabinet {
        public int Id { get; }
        
        public List<YisoLocale> Contents { get; }
        public List<Content> CabinetContents { get; }

        public YisoCabinet(YisoCabinetSO so) {
            Id = so.id;
            Contents = new List<YisoLocale>(so.contents);
            CabinetContents = new List<Content>(so.cabinetContents.Select(c => new Content(c)));
        }

        public class Content {
            public bool IsText { get; }
            public YisoLocale Text { get; }
            public Sprite Image { get; }

            public Content(YisoCabinetSO.Content content) {
                IsText = content.text;
                Text = content.textContent;
                Image = content.imageContent;
            }
        }
    }
}