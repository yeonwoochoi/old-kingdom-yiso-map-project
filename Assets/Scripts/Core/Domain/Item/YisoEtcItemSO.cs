using System;
using UnityEngine;

namespace Core.Domain.Item {
    [CreateAssetMenu(fileName = "EtcItem", menuName = "Yiso/Item/Etc Item")]
    public class YisoEtcItemSO : YisoItemSO {
        public override YisoItem CreateItem() => new YisoEtcItem(this);
    }

    [Serializable]
    public class YisoEtcItem : YisoItem {

        public YisoEtcItem(YisoEtcItemSO so) : base(so) { }
        
        public YisoEtcItem(YisoEtcItem item) : base(item) { }
    }
}