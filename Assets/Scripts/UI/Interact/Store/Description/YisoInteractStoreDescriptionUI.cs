using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Locale;
using Core.Domain.Types;
using DG.Tweening;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Common.Inventory.Potential;
using UI.Interact.Store.Holder;
using UI.Menu.Inventory;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Store.Description {
    public class YisoInteractStoreDescriptionUI : YisoUIController {
        [SerializeField, Title("Descriptions")]
        private Image itemImage;

        [SerializeField] private TextMeshProUGUI itemRankText;
        [SerializeField] private TextMeshProUGUI itemTitleText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private YisoInteractStoreDescriptionPriceUI priceUI;
        [SerializeField, Title("Button")] private YisoMenuInventoryButtonUI sellOrPurchaseButton;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private YisoPlayerPotentialDescriptionUI potentialDescriptionUI;

        private YisoItem item = null;
        private YisoInteractStoreContentUI.Types currentType;
        private double currentPrice;

        private Material itemMaterial;

        private CanvasGroup canvasGroup;

        public void Visible(bool flag) {
            canvasGroup.Visible(flag);
        }
        
        public event UnityAction<YisoInteractStoreContentUI.Types, YisoItem, double> OnButtonActionEvent;

        protected override void Start() {
            itemMaterial = itemImage.material;
            canvasGroup = GetComponent<CanvasGroup>();
            Clear();
            sellOrPurchaseButton.InventoryButton.onClick.AddListener(() => OnButtonActionEvent?.Invoke(currentType, item, currentPrice));
        }

        public void SetItem(YisoInteractStoreContentUI.Types type, YisoItem item, double price) {
            scrollRect.DOVerticalNormalizedPos(1f, 0.2f);
            this.item = item;
            currentType = type;
            itemImage.sprite = item.Icon;
            itemImage.enabled = true;
            itemTitleText.SetText(item.GetName(CurrentLocale));
            SetDescription();
            SetButton();
            
            currentPrice = type == YisoInteractStoreContentUI.Types.STORE ? price : item.SellPrice;
            priceUI.SetPrice(type, currentPrice, CurrentLocale);

            if (type == YisoInteractStoreContentUI.Types.STORE || item is not YisoEquipItem equipItem) {
                potentialDescriptionUI.ActiveDescription(false);
                return;
            }
            SetRank(equipItem.Rank);
            sellOrPurchaseButton.Active(!equipItem.Equipped);
            
            potentialDescriptionUI.ActiveDescription(true);
            potentialDescriptionUI.SetPotentials(equipItem);
        }

        public void Clear() {
            item = null;
            itemImage.enabled = false;
            itemTitleText.SetText("");
            itemDescriptionText.SetText("");
            sellOrPurchaseButton.Active(false);
            priceUI.Clear();
            itemRankText.SetText("");
            
            itemMaterial.ActiveOutline(false);
            itemMaterial.ActiveOutlineDistortion(false);
            
            sellOrPurchaseButton.Active(true);
            potentialDescriptionUI.Clear();
            scrollRect.verticalNormalizedPosition = 1f;
        }
        
        private void SetRank(YisoEquipRanks rank) {
            if (rank == YisoEquipRanks.N) return;
            
            itemMaterial.ActiveOutline(true);
            var rankColor = rank.ToColor();
            itemMaterial.SetOutlineColor(rankColor);
            itemMaterial.ActiveOutlineDistortion(true);
            itemRankText.SetText($"{rank.ToString(CurrentLocale)}({rank.ToString()})");
        }
        
        private void SetDescription() {
            var description = item.GetDescription(CurrentLocale);
            if (item is YisoEquipItem equipItem) {
                var attack = CurrentLocale == YisoLocale.Locale.KR ? "공격력" : "Attack";
                var defence = CurrentLocale == YisoLocale.Locale.KR ? "방어력" : "Defence";
                // description = $"{description}\n\n{attack}: +{equipItem.GetAttack()}\n{defence}: +{equipItem.GetDefence()}";
            }
            itemDescriptionText.SetText(description);
        }

        private void SetButton() {
            string buttonText;
            if (currentType == YisoInteractStoreContentUI.Types.INVENTORY) {
                buttonText = CurrentLocale == YisoLocale.Locale.KR ? "판매" : "Sell";
            } else {
                buttonText = CurrentLocale == YisoLocale.Locale.KR ? "구매" : "Purchase";
            }
            sellOrPurchaseButton.SetText(buttonText);
            sellOrPurchaseButton.Active(true);
        }
    }
}