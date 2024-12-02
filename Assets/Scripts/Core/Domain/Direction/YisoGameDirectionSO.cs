using System.Collections.Generic;
using Core.Domain.Locale;
using UnityEngine;

namespace Core.Domain.Direction {
    [CreateAssetMenu(fileName = "GameDirection", menuName = "Yiso/Game/Direction")]
    public class YisoGameDirectionSO : ScriptableObject {
        public int id;
        public List<YisoLocale> directions;

        public YisoGameDirection CreateDirection() => new(this);
    }

    public class YisoGameDirection {
        public int Id { get; }
        public List<YisoLocale> Directions { get; }

        public YisoGameDirection(YisoGameDirectionSO so) {
            Id = so.id;
            Directions = new List<YisoLocale>(so.directions);
        }
    }
}