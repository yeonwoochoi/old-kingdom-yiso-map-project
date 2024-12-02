using Core.Domain.Item;
using Core.Domain.Locale;
using Sirenix.OdinInspector;
using TMPro;
using UI.Menu.Inventory.V2.Description;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Store.V2.Description {
    public class YisoInteractStoreV2OtherDescriptionUI : YisoMenuInventoryV2BaseDescriptionUI<YisoItem> {
        [SerializeField, Title("Specific")] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI countTitleText;

        private RectTransform descriptionTextRect;
        
        protected override void Start() {
            base.Start();
            descriptionTextRect = (RectTransform)descriptionText.transform;
        }

        public override void SetItem(YisoItem item) {
            base.SetItem(item);
            if (item is YisoUseItem useItem) {
                descriptionText.SetText(useItem.GetUIDescription(CurrentLocale));
            } else {
                descriptionText.SetText(item.GetDescription(CurrentLocale));
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionTextRect);
        }

        public void SetStoreItem(YisoInteractStoreContentUI.Types type) {
            if (type == YisoInteractStoreContentUI.Types.STORE) {
                combatRatingOrCountText.SetText("");
                countTitleText.SetText("");
            } else {
                countTitleText.SetText(CurrentLocale == YisoLocale.Locale.KR ? "보유량" : "Quantity");
            }
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