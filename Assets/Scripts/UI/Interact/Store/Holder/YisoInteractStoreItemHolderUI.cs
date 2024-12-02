using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Locale;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Game;
using TMPro;
using UI.Effects;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Store.Holder {
    public class YisoInteractStoreItemHolderUI : RunIBehaviour, IInstantiatable {
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemRankText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private CanvasGroup equippedCanvas;

        public  Toggle ItemToggle { get; private set; }
        
        public YisoItem Item { get; private set; }
        
        public double Price { get; private set; }

        private bool active = false;

        public bool Active {
            get => active;
            set {
                active = value;
                ItemToggle.interactable = value;
                gameObject.SetActive(value);
            }
        }

        private Material itemMaterial;
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
        
        public void Init() {
            itemMaterial = itemImage.material;
            ItemToggle = GetComponent<Toggle>();
            Clear();
        }

        public void UpdateCount(int count, YisoLocale.Locale locale) {
            SetItemName(YisoInteractStoreContentUI.Types.INVENTORY, count, locale);
        }
        
        public void SetItem(YisoInteractStoreContentUI.Types type, YisoItem item, double price, YisoLocale.Locale locale) {
            Item = item;
            itemImage.sprite = item.Icon;
            itemImage.enabled = true;
            SetItemName(type, item.Quantity, locale);
            priceText.SetText(price.ToCommaString());
            Price = price;
            Active = true;
            if (itemRankText == null) return;
            if (item is not YisoEquipItem equipItem) return;
            SetRank(equipItem.Rank);
        }

        private void SetItemName(YisoInteractStoreContentUI.Types type, int count, YisoLocale.Locale locale) {
            var itemName = Item.GetName(locale);
            if (Item.InvType == YisoItem.InventoryType.EQUIP || type == YisoInteractStoreContentUI.Types.STORE)
                itemNameText.SetText(itemName);
            else {
                itemNameText.SetText($"{itemName} (x{count})");
            }
        }

        private void SetRank(YisoEquipRanks rank) {
            if (rank == YisoEquipRanks.N) return;
            
            itemMaterial.ActiveOutline(true);
            var rankColor = rank.ToColor();
            itemMaterial.SetOutlineColor(rankColor);
            var currentLocale = YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
            itemRankText.SetText($"{rank.ToString(currentLocale)}({rank.ToString()})");
        }

        public void Clear() {
            Active = false;
            itemImage.enabled = false;
            itemNameText.SetText("");
            priceText.SetText("");
            if (itemRankText == null) return;
            itemRankText.SetText("");
            itemMaterial.DisableKeyword("OUTBASE_ON");
            equippedCanvas.Visible(false);
            ItemToggle.interactable = true;
        }
    }
}