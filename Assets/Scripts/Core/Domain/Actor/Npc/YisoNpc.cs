using Core.Domain.Entity;
using Core.Domain.Locale;

namespace Core.Domain.Actor.Npc {
    public class YisoNpc : IYisoEntity {
        public int Id { get; }

        private readonly YisoLocale name;

        private readonly YisoLocale description;

        public YisoNpc(YisoNpcSO so) {
            Id = so.id;
            name = so.name;
            description = so.description;
        }

        public int GetId() => Id;

        public string GetName(YisoLocale.Locale locale) => name[locale];

        public string GetDescription(YisoLocale.Locale locale) => description[locale];
    }
}