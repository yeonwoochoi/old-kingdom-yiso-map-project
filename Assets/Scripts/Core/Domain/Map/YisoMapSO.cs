using Core.Domain.Locale;
using UnityEngine;

namespace Core.Domain.Map {
    [CreateAssetMenu(fileName = "Map", menuName = "Yiso/Map/Map")]
    public class YisoMapSO : ScriptableObject {
        public int id;
        public new YisoLocale name;
        public YisoLocale description;
        public GameObject prefab;

        public YisoMap CreateMap() => new(this);
    }

    public class YisoMap {
        public int Id { get; }
        public GameObject Prefab { get; }

        private readonly YisoLocale name;
        private readonly YisoLocale description;

        public YisoMap(YisoMapSO so) {
            Id = so.id;
            Prefab = so.prefab;
        }

        public string GetName(YisoLocale.Locale locale) => name[locale];
        public string GetDescription(YisoLocale.Locale locale) => description[locale];
    }
}