using System.Collections.Generic;
using Core.Domain.Item;
using UnityEngine.Events;

namespace UI.Popup.Inventory {
    public class YisoPopupInventoryInputArgs {
        public YisoPopupInventoryInputContentUI.Types Type { get; set; }
        public YisoItem Item { get; set; }
        public List<UnityAction<int>> OnClickOkList { get; } = new();
        public List<UnityAction> OnClickCloseList { get; } = new();
        public double Price { get; set; }
    }
}