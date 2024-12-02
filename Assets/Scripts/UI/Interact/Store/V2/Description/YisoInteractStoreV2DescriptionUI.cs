using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Locale;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Store.V2.Description {
    public class YisoInteractStoreV2DescriptionUI : YisoUIController {
        [SerializeField] private YisoInteractStoreV2EquipDescriptionUI equipDescriptionUI;
        [SerializeField] private YisoInteractStoreV2OtherDescriptionUI otherDescriptionUI;
        [SerializeField] private TextMeshProUGUI sellOrBuyText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI sellOrBuyButtonText;
        [SerializeField] private CanvasGroup sellOrBuyCanvas;
        [SerializeField] private RectTransform sellOrBuyRect;

        public event UnityAction<YisoInteractStoreContentUI.Types, YisoItem, double> OnButtonActionEvent;
        private CanvasGroup canvasGroup;
        private YisoInteractStoreContentUI.Types currentType;
        private double price;

        
        protected override void Start() {
            canvasGroup = GetComponent<CanvasGroup>();
            Clear();
        }

        public void Visible(bool flag) {
            canvasGroup.Visible(flag);
        }

        public void SetItem(YisoInteractStoreContentUI.Types type, YisoItem item, double price) {
            currentType = type;
            this.price = price;
            if (item is YisoEquipItem equipItem) {
                equipDescriptionUI.SetItem(equipItem);
                equipDescriptionUI.Visible(true);
            } else {
                otherDescriptionUI.SetItem(item);
                otherDescriptionUI.SetStoreItem(type);
                otherDescriptionUI.Visible(true);
            }
            
            SetTexts();
            priceText.SetText(price.ToCommaString());
            sellOrBuyCanvas.Visible(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(sellOrBuyRect);
        }

        public void Clear() {
            if (equipDescriptionUI.Active) {
                equipDescriptionUI.Clear();
                equipDescriptionUI.Visible(false);
            }

            if (otherDescriptionUI.Active) {
                otherDescriptionUI.Clear();
                otherDescriptionUI.Visible(false);
            }
            
            sellOrBuyButtonText.SetText("");
            sellOrBuyText.SetText("");
            priceText.SetText("");
            
            price = 0;
            sellOrBuyCanvas.Visible(false);
        }

        public void OnClickButton() {
            var item = equipDescriptionUI.Active ? equipDescriptionUI.Item : otherDescriptionUI.Item;
            OnButtonActionEvent?.Invoke(currentType, item, price);
        }

        private void SetTexts() {
            string buttonText;
            if (currentType == YisoInteractStoreContentUI.Types.INVENTORY) {
                buttonText = CurrentLocale == YisoLocale.Locale.KR ? "판매" : "Sell";
            } else {
                buttonText = CurrentLocale == YisoLocale.Locale.KR ? "구매" : "Purchase";
            }

            var sellOrButStr = CurrentLocale == YisoLocale.Locale.KR ? $"{buttonText} 가격" : $"{buttonText} Price";
            sellOrBuyButtonText.SetText(buttonText);
            sellOrBuyText.SetText(sellOrButStr);
        }
    }
}