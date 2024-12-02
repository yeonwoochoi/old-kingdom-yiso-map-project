using System;
using Core.Behaviour;
using Core.Domain.Locale;
using TMPro;
using UI.Interact.Store.Holder;
using UnityEngine;
using Utils.Extensions;

namespace UI.Interact.Store.Description {
    public class YisoInteractStoreDescriptionPriceUI : RunIBehaviour {
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private CanvasGroup canvasGroup;

        public void SetPrice(YisoInteractStoreContentUI.Types type, double price, YisoLocale.Locale locale) {
            typeText.SetText(GetTypeText(type, locale));
            priceText.SetText(price.ToCommaString());
            canvasGroup.Visible(true);
        }

        public void Clear() {
            canvasGroup.Visible(false);
        }

        private string GetTypeText(YisoInteractStoreContentUI.Types type, YisoLocale.Locale locale) => type switch {
            YisoInteractStoreContentUI.Types.STORE => locale == YisoLocale.Locale.KR ? "구매 가격:" : "Purchase Price:",
            YisoInteractStoreContentUI.Types.INVENTORY => locale == YisoLocale.Locale.KR ? "판매 가격:" : "Sell Price:",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}