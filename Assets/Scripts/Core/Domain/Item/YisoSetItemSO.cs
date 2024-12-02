using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Locale;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using Utils.Extensions;

namespace Core.Domain.Item {
    [CreateAssetMenu(fileName = "SetItem", menuName = "Yiso/Item/Set Item")]
    public class YisoSetItemSO : ScriptableObject {
        public int id;
        public new YisoLocale name;
        public List<YisoEquipItemSO> items;

        [ShowIf("@this.items.Count > 1")] public List<YisoSetItem.ItemEffect> effect2;
        [ShowIf("@this.items.Count > 2")] public List<YisoSetItem.ItemEffect> effect3;
        [ShowIf("@this.items.Count > 3")] public List<YisoSetItem.ItemEffect> effect4;
        [ShowIf("@this.items.Count > 4")] public List<YisoSetItem.ItemEffect> effect5;
        [ShowIf("@this.items.Count > 5")] public List<YisoSetItem.ItemEffect> effect6;

        public YisoSetItem CreateSetItem() => new(this);
    }

    [Serializable]
    public class YisoSetItem {
        private YisoLocale name;
        public int Id { get; }
        public Dictionary<int, List<ItemEffect>> Effects { get; }
        public List<int> ItemIds { get; }

        public YisoSetItem(YisoSetItemSO so) {
            name = so.name;
            Id = so.id;
            Effects = new Dictionary<int, List<ItemEffect>>();
            ItemIds = new List<int>(so.items.Select(item => item.id));

            AddEffect(so);
        }

        private void AddEffect(YisoSetItemSO so) {
            var items = so.items;
            switch (items.Count) {
                case 2 when so.effect2.IsEmpty():
                    throw new Exception($"Set({name[YisoLocale.Locale.KR]}) Item set '2' but, effect must be set!");
                case 3 when (so.effect2.IsEmpty() || so.effect3.IsEmpty()):
                    throw new Exception($"Set({name[YisoLocale.Locale.KR]}) Item set '3' but, effect must be set!");
                case 4 when (so.effect2.IsEmpty() || so.effect3.IsEmpty() || so.effect4.IsEmpty()):
                    throw new Exception($"Set({name[YisoLocale.Locale.KR]}) Item set '4' but, effect must be set!");
                case 5 when (so.effect2.IsEmpty() || so.effect3.IsEmpty() || so.effect4.IsEmpty() ||
                             so.effect5.IsEmpty()):
                    throw new Exception($"Set({name[YisoLocale.Locale.KR]}) Item set '5' but, effect must be set!");
                case 6 when (so.effect2.IsEmpty() || so.effect3.IsEmpty() || so.effect4.IsEmpty() ||
                             so.effect5.IsEmpty() || so.effect6.IsEmpty()):
                    throw new Exception($"Set({name[YisoLocale.Locale.KR]}) Item set '6' but, effect must be set!");
            }

            switch (items.Count) {
                case 2:
                    Effects[2] = new List<ItemEffect>(so.effect2);
                    break;
                case 3:
                    Effects[2] = new List<ItemEffect>(so.effect2);
                    Effects[3] = new List<ItemEffect>(so.effect3);
                    break;
                case 4:
                    Effects[2] = new List<ItemEffect>(so.effect2);
                    Effects[3] = new List<ItemEffect>(so.effect3);
                    Effects[4] = new List<ItemEffect>(so.effect4);
                    break;
                case 5:
                    Effects[2] = new List<ItemEffect>(so.effect2);
                    Effects[3] = new List<ItemEffect>(so.effect3);
                    Effects[4] = new List<ItemEffect>(so.effect4);
                    Effects[5] = new List<ItemEffect>(so.effect5);
                    break;
                case 6:
                    Effects[2] = new List<ItemEffect>(so.effect2);
                    Effects[3] = new List<ItemEffect>(so.effect3);
                    Effects[4] = new List<ItemEffect>(so.effect4);
                    Effects[5] = new List<ItemEffect>(so.effect5);
                    Effects[6] = new List<ItemEffect>(so.effect6);
                    break;
            }
        }

        public Queue<int> GetKeys() {
            var result = new Queue<int>();
            switch (ItemIds.Count) {
                case 2:
                    result.Enqueue(2);
                    break;
                case 3:
                    result.Enqueue(2);
                    result.Enqueue(3);
                    break;
                case 4:
                    result.Enqueue(2);
                    result.Enqueue(3);
                    result.Enqueue(4);
                    break;
                case 5:
                    result.Enqueue(2);
                    result.Enqueue(3);
                    result.Enqueue(4);
                    result.Enqueue(5);
                    break;
                case 6:
                    result.Enqueue(2);
                    result.Enqueue(3);
                    result.Enqueue(4);
                    result.Enqueue(5);
                    result.Enqueue(6);
                    break;
            }

            return result;
        }

        public string GetName(YisoLocale.Locale locale) => name[locale];

        public string[] GetEffectUIText(int level, YisoLocale.Locale locale) {
            var effects = Effects[level];
            return effects.Select(effect => effect.effect.ToString(effect.value, locale)).ToArray();
        }

        [Serializable]
        public class ItemEffect {
            public YisoBuffEffectTypes effect;
            public int value;
        }
    }
}