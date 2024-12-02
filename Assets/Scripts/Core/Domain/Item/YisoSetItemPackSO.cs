using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Domain.Item {
    [CreateAssetMenu(fileName = "SetItemPack", menuName = "Yiso/Item/Set Item Pack")]
    public class YisoSetItemPackSO : ScriptableObject {
        public List<YisoSetItemSO> setItems;

        public YisoSetItemPack CreatePack() => new(this);
    }

    public class YisoSetItemPack {
        public List<YisoSetItem> SetItems { get; }

        public YisoSetItemPack(YisoSetItemPackSO so) {
            SetItems = new List<YisoSetItem>(so.setItems.Select(set => set.CreateSetItem()));
        }
    }
}