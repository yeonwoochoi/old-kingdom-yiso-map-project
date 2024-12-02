using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Locale;
using Core.Domain.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Store.Selection {
    public class YisoInteractStoreSelectionItemHolderUI : RunIBehaviour {
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemRankText;
        [SerializeField] private TextMeshProUGUI priceText;

        public bool Active { get; private set; } = false;
        
        public YisoItem Item { get; private set; }
        public double Price { get; private set; }

        private Material itemMaterial;

        public void SetItem(YisoItem item, double price, YisoLocale.Locale locale) {
            Active = true;
            Item = item;
            itemImage.sprite = item.Icon;
            itemImage.preserveAspect = true;
            itemImage.enabled = true;
            var itemName = item.GetName(locale);
            itemNameText.SetText(item.InvType == YisoItem.InventoryType.EQUIP
                ? itemName
                : $"{itemName} (x{item.Quantity})");

            Price = price;
            priceText.SetText(price.ToCommaString());
            
            if (item is not YisoEquipItem equipItem) return;
            if (equipItem.Rank == YisoEquipRanks.N) return;
            itemMaterial.ActiveOutline(true);
            var rankColor = equipItem.Rank.ToColor();
            itemMaterial.SetOutlineColor(rankColor);
            itemRankText.SetText($"{equipItem.Rank.ToString(locale)}({equipItem.Rank.ToString()})");
        }

        public void Clear() {
            Active = false;
        }
        
        public void Init() {
            itemMaterial = itemImage.material;
            itemImage.enabled = false;
        }
    }
}