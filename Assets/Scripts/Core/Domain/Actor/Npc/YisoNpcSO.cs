using Core.Domain.Locale;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Actor.Npc {
    public class YisoNpcSO : ScriptableObject {
        public int id;
        public new YisoLocale name;
        public YisoLocale description;
        [PreviewField] public Sprite icon;

        public YisoNpc CreateNpc() => new(this);
    }
}