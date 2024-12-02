using System;
using System.IO;
using System.Linq;
using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Domain.Types;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Test {
    public class TestSOData : MonoBehaviour {
        [SerializeField] private YisoItemPackSO equipPackSO;

        [Button]
        public void SaveToJson() {
            var items = equipPackSO.items.Cast<YisoEquipItemSO>()
                .Where(equipItem => {
                    if (equipItem.slot != YisoEquipSlots.WEAPON) return true;
                    var en = equipItem.name.en;
                    var kr = equipItem.name.kr;
                    return !en.Contains("shield", StringComparison.OrdinalIgnoreCase) &&
                           !kr.Contains("방패");
                }).Select(so => new Item(so));
            
            var serialized = JsonConvert.SerializeObject(items);
            File.WriteAllText("/Users/joshlee/study/python/items.json", serialized);
        }

        [Serializable]
        public class Item {
            public int id;
            public YisoEquipSlots slot;
            public int reqLevel;
            public string name;
            public YisoEquipRanks rank;
            public int attack;
            public int defence;

            public Item(YisoEquipItemSO so) {
                id = so.id;
                slot = so.slot;
                reqLevel = so.reqLevel;
                name = so.name.kr;
                rank = so.rank;
                attack = so.attack;
                defence = so.defence;
            }
        }
    }
}