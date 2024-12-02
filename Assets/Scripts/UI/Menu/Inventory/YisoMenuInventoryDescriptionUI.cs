using System;
using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Locale;
using Core.Domain.Types;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Common.Inventory.Potential;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Inventory {
    public class YisoMenuInventoryDescriptionUI : YisoUIController {
        [SerializeField, Title("Descriptions")]
        private Image itemImage;

        [SerializeField] private TextMeshProUGUI itemRankText;
        [SerializeField] private TextMeshProUGUI itemTitleText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField, Title("Buttons")] private YisoMenuInventoryButtonUI dropButton;
        [SerializeField] private YisoMenuInventoryButtonUI quickSlotButton;
        [SerializeField] private YisoMenuInventoryButtonUI activeButton;
        [SerializeField] private YisoPlayerPotentialDescriptionUI potentialDescriptionUI;
        [SerializeField] private ScrollRect descriptionScrollRect;

        private YisoItem item = null;
        public bool ActiveQuickSlots { get; set; }

        private Material itemMaterial;
        public event UnityAction<YisoMenuInventoryButtonUI.Types, YisoItem> OnButtonActionRaised;

        protected override void Start() {
            itemMaterial = itemImage.material;
            Clear();
            AddOnClick();
        }

        public void SetItem(YisoItem item) {
            descriptionScrollRect.DOVerticalNormalizedPos(1f, 0.2f);
            this.item = item;

            itemImage.sprite = item.Icon;
            itemImage.enabled = true;
            itemTitleText.SetText(item.GetName(CurrentLocale));
            SetDescription();
            SetButtons();
            if (item is not YisoEquipItem equipItem) {
                potentialDescriptionUI.ActiveDescription(true);
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
            dropButton.Active(false);
            activeButton.Active(false);
            quickSlotButton.Active(false);
            itemRankText.SetText("");
            
            itemMaterial.ActiveOutline(false);
            itemMaterial.ActiveOutlineDistortion(false);
            descriptionScrollRect.verticalNormalizedPosition = 1f;
            potentialDescriptionUI.Clear();
        }

        private void AddOnClick() {
            dropButton.InventoryButton.onClick.AddListener(() => {
                RaiseEvent(YisoMenuInventoryButtonUI.Types.DROP);
            });
            
            quickSlotButton.InventoryButton.onClick.AddListener(() => {
                RaiseEvent(YisoMenuInventoryButtonUI.Types.QUICK);
            });
            
            activeButton.InventoryButton.onClick.AddListener(() => {
                switch (item) {
                    case null:
                        return;
                    case YisoEquipItem equipItem:
                        RaiseEvent(equipItem.Equipped
                            ? YisoMenuInventoryButtonUI.Types.UN_EQUIP
                            : YisoMenuInventoryButtonUI.Types.EQUIP);
                        return;
                    default:
                        RaiseEvent(YisoMenuInventoryButtonUI.Types.USE);
                        break;
                }
            });
        }

        public void UpdateItem(YisoItem item) {
            Clear(true);
            this.item = item;
            SetButtons();
        }

        private void SetRank(YisoEquipRanks rank) {
            if (rank == YisoEquipRanks.N) return;
            
            itemMaterial.ActiveOutline(true);
            var rankColor = rank.ToColor();
            itemMaterial.SetOutlineColor(rankColor);
            itemMaterial.ActiveOutlineDistortion(true);
            itemRankText.SetText($"{rank.ToString(CurrentLocale)}({rank.ToString()})");
        }
        
        private void SetButtons() {
            activeButton.Active(item.InvType != YisoItem.InventoryType.ETC);
            quickSlotButton.Active(item.InvType == YisoItem.InventoryType.USE);
            if (item is YisoEquipItem equipItem) {
                dropButton.Active(!equipItem.Equipped);
            } else 
                dropButton.Active(true);
            activeButton.SetText(GetActiveButtonText());
        }

        private string GetActiveButtonText() {
            var type = item.InvType;
            switch (type) {
                case YisoItem.InventoryType.EQUIP:
                    var equipItem = (YisoEquipItem) item;
                    if (equipItem.Equipped) return CurrentLocale == YisoLocale.Locale.KR ? "해제" : "UnEquip";
                    return CurrentLocale == YisoLocale.Locale.KR ? "장착" : "Equip";
                case YisoItem.InventoryType.USE:
                    return CurrentLocale == YisoLocale.Locale.KR ? "사용" : "Use";
                default:
                    return "";
            }
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

        private void RaiseEvent(YisoMenuInventoryButtonUI.Types type) {
            OnButtonActionRaised?.Invoke(type, item);
        }
    }
}