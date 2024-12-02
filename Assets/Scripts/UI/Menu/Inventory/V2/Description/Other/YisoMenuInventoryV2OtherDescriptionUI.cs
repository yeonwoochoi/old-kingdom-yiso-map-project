using Core.Domain.Item;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Inventory.V2.Description.Other {
    public class YisoMenuInventoryV2OtherDescriptionUI : YisoMenuInventoryV2BaseDescriptionUI<YisoItem> {
        [SerializeField, Title("Specific")] private TextMeshProUGUI descriptionText;

        private RectTransform descriptionTextRect;

        protected override void Start() {
            base.Start();
            descriptionTextRect = (RectTransform) descriptionText.transform;
        }

        public override void SetItem(YisoItem item) {
            base.SetItem(item);
            if (item is YisoUseItem useItem) {
                descriptionText.SetText(useItem.GetUIDescription(CurrentLocale));
            } else 
                descriptionText.SetText(item.GetDescription(CurrentLocale));
            LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionTextRect);
        }

        public override void Clear() {
            base.Clear();
            descriptionText.SetText("");
        }

        public void UpdateCount(int count) {
            combatRatingOrCountText.SetText(count.ToCommaString());
        }
    }
}