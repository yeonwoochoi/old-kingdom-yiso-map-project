using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Item {
    [CreateAssetMenu(fileName = "ItemPack", menuName = "Yiso/Item/Pack")]
    public class YisoItemPackSO : ScriptableObject {
        [ReadOnly] public string lastUpdatedAt;
        public List<YisoItemSO> items = new();

        public virtual YisoItemPack CreatePack() => new (this);

        public bool TryGetItem(int id, out YisoItemSO so) {
            so = items.Find(item => item.id == id);
            return so != null;
        }
        
        #if UNITY_EDITOR

        [Button]
        public void Validate() {
            var totalCount = items.Count;
            var missingCount = items.Count(item => item == null);
            Debug.Log($"Validated, Total: {totalCount}, Missing: {missingCount}");
        }
        
        #endif
    }

    public class YisoItemPack {
        public List<YisoItem> Items { get; }
        public YisoItemPack(YisoItemPackSO so) {
            Items = new(so.items.Select(i => i.CreateItem()));
        }

        public bool TryGetItem<T>(int itemId, out T item) where T : YisoItem {
            item = null;
            var raw = Items.Find(i => i.Id == itemId);

            if (raw != null) {
                item = raw as T;
            }
            
            return item != null;
        }
    }
}