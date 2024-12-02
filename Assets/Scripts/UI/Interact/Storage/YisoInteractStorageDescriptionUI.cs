using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Locale;
using Core.Domain.Types;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Common.Inventory.Potential;
using UI.Menu.Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Storage {
    public class YisoInteractStorageDescriptionUI : YisoUIController {
        [SerializeField, Title("Descriptions")]
        private Image itemImage;

        [SerializeField] private TextMeshProUGUI itemRankText;
        [SerializeField] private TextMeshProUGUI itemTitleText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField, Title("Buttons")] private YisoMenuInventoryButtonUI keepOrPullButton;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private YisoPlayerPotentialDescriptionUI potentialDescriptionUI;
        
        private YisoItem item = null;
        private YisoInteractStorageItemHoldersUI.Types currentType;
        
        private Material itemMaterial;

        public event UnityAction<YisoInteractStorageItemHoldersUI.Types, YisoItem> OnButtonActionRaised; 

        protected override void Start() {
            itemMaterial = itemImage.material;
            Clear();
            keepOrPullButton.InventoryButton.onClick.AddListener(() => OnButtonActionRaised?.Invoke(currentType, item));
        }

        public void SetItem(YisoInteractStorageItemHoldersUI.Types type, YisoItem item) {
            scrollRect.DOVerticalNormalizedPos(1f, .2f);
            this.item = item;
            currentType = type;
            itemImage.sprite = item.Icon;
            itemImage.enabled = true;
            itemTitleText.SetText(item.GetName(CurrentLocale));
            SetDescription();
            SetButton();
            if (item is not YisoEquipItem equipItem) {
                potentialDescriptionUI.ActiveDescription(false);
                return;
            }

            SetRank(equipItem.Rank);
            potentialDescriptionUI.ActiveDescription(true);
            potentialDescriptionUI.SetPotentials(equipItem);
        }

        public void Clear(bool refresh = false) {
            item = null;
            if (!refresh) {
                itemImage.enabled = false;
                itemTitleText.SetText("");
                itemDescriptionText.SetText("");
            }
            keepOrPullButton.Active(false);
            itemRankText.SetText("");
            itemMaterial.ActiveOutline(false);
            itemMaterial.ActiveOutlineDistortion(false);
            potentialDescriptionUI.Clear();
            scrollRect.verticalNormalizedPosition = 1f;
        }
        
        public void UpdateItem(YisoItem item) {
            Clear(true);
        }
        
        private void SetRank(YisoEquipRanks rank) {
            if (rank == YisoEquipRanks.N) return;
            
            itemMaterial.ActiveOutline(true);
            var rankColor = rank.ToColor();
            itemMaterial.SetOutlineColor(rankColor);
            itemMaterial.ActiveOutlineDistortion(true);
            itemRankText.SetText($"{rank.ToString(CurrentLocale)}({rank.ToString()})");
        }

        private void SetButton() {
            string buttonText;
            if (currentType == YisoInteractStorageItemHoldersUI.Types.STORAGE) {
                buttonText = CurrentLocale == YisoLocale.Locale.KR ? "꺼내기" : "Pull";
            } else {
                buttonText = CurrentLocale == YisoLocale.Locale.KR ? "보관" : "Keep";
            }
            keepOrPullButton.SetText(buttonText);
            keepOrPullButton.Active(true);
        }
        
        private void SetDescription() {
            var description = item.GetDescription(CurrentLocale);
            if (item is YisoEquipItem equipItem) {
                var attack = CurrentLocale == YisoLocale.Locale.KR ? "공격력" : "Attack";
                var defence = CurrentLocale == YisoLocale.Locale.KR ? "방어력" : "Defence";
                description = string.Empty;
                // $"{description}\n\n{attack}: +{equipItem.GetAttack()}\n{defence}: +{equipItem.GetDefence()}";
            }
            itemDescriptionText.SetText(description);
        }
    }
}