using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using TMPro;
using UI.Common.Inventory;
using UI.Menu.Inventory.V2.Description;
using UI.Menu.Inventory.V2.Description.Equip;
using UnityEngine;
using Utils.Extensions;

namespace UI.Interact.Store.V2.Description {
    public class YisoInteractStoreV2EquipDescriptionUI : YisoMenuInventoryV2BaseDescriptionUI<YisoEquipItem> {
        [SerializeField, Title("Specific")] private TextMeshProUGUI statTitleText;
        [SerializeField] private TextMeshProUGUI statValueText;
        [SerializeField] private TextMeshProUGUI upgradableText;
        [SerializeField] private YisoMenuInventoryV2EquipAdditionalStatsDescriptionUI additionalDescriptionUI;
        [SerializeField] private YisoMenuInventoryV2EquipSetDescriptionUI setDescriptionUI;
        [SerializeField] private YisoButtonWithCanvas setButton;

        private bool visibleSet = false;

        private Material itemMaterial;

        protected override void Start() {
            base.Start();
            itemMaterial = itemImage.material;
            setButton.onClick.AddListener(OnClickSetButton);
        }

        public override void SetItem(YisoEquipItem item) {
            base.SetItem(item);
            SetRank();
            SetStats();
            additionalDescriptionUI.SetPotentials(item);
            setButton.Visible = setDescriptionUI.SetItem(item);
        }

        private void OnClickSetButton() {
            visibleSet = !visibleSet;
            additionalDescriptionUI.Visible(!visibleSet);
            setDescriptionUI.Visible(visibleSet);
            SetButtonText();
        }

        private void SetButtonText() {
            string buttonText;
            if (visibleSet) {
                buttonText = CurrentLocale == YisoLocale.Locale.KR ? "옵션 보기" : "Show Options";
            } else {
                buttonText = CurrentLocale == YisoLocale.Locale.KR ? "세트 보기" : "Show Set";
            }
            
            setButton.SetText(buttonText);
        }
        
        private void SetRank() {
            var rank = Item.Rank;
            var (outline, pattern) = rank.GetRankColorPair();
            if (rank == YisoEquipRanks.N) return;

            itemNameText.color = pattern;
            itemRankText.color = pattern;

            itemMaterial.ActiveOutline(true);
            itemMaterial.SetOutlineColor(pattern);
            itemMaterial.ActiveOutlineDistortion(true);
        }
        
        public override void Clear() {
            base.Clear();
            
            upgradableText.SetText("");
            
            itemNameText.color = Color.white;
            itemRankText.color = Color.white;
            
            additionalDescriptionUI.Clear();

            itemMaterial.ActiveOutline(false);
            itemMaterial.ActiveOutlineDistortion(false);
            
            additionalDescriptionUI.Clear();
            setDescriptionUI.Clear();
            setDescriptionUI.Visible(false);
            additionalDescriptionUI.Visible(true);

            if (setButton.Visible) {
                setButton.Visible = false;
                visibleSet = false;
                SetButtonText();
            }
        }
        
        private void SetStats() {
            string title;
            if (Item.Slot == YisoEquipSlots.WEAPON) {
                title = CurrentLocale == YisoLocale.Locale.KR ? "공격력:" : "Attack";
            } else {
                title = CurrentLocale == YisoLocale.Locale.KR ? "방어력:" : "Defence";
            }
            statTitleText.SetText(title);
            var value = Item.CombatRating * 0.8654321f;
            value = Mathf.CeilToInt((float) value);
            statValueText.SetText(value.ToCommaString());

            var upgradableTitle = CurrentLocale == YisoLocale.Locale.KR ? "강화 가능 횟수" : "Reinforce Count";
            var upgradableValue = Item.UpgradableSlots;
            upgradableText.SetText($"{upgradableTitle}: <b>{upgradableValue}</b>");
        }
    }
}