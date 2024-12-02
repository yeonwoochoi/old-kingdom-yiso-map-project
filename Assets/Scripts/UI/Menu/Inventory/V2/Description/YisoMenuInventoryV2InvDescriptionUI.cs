using System;
using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Locale;
using Sirenix.OdinInspector;
using UI.Base;
using UI.Common.Inventory;
using UI.Menu.Inventory.V2.Description.Equip;
using UI.Menu.Inventory.V2.Description.Other;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace UI.Menu.Inventory.V2.Description {
    public class YisoMenuInventoryV2InvDescriptionUI : YisoPlayerUIController {
        [SerializeField] private YisoMenuInventoryV2EquipDescriptionUI equipDescriptionUI;
        [SerializeField] private YisoMenuInventoryV2OtherDescriptionUI otherDescriptionUI;
        [SerializeField, Title("Buttons")] internal YisoButtonWithCanvas dropButton;
        [SerializeField] internal YisoButtonWithCanvas quickButton;
        [SerializeField] internal YisoButtonWithCanvas equipOrUseButton;

        private YisoItem item;
        
        private CanvasGroup canvasGroup;
        
        protected override void Start() {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Visible(bool flag) {
            canvasGroup.Visible(flag);
        }

        public void RegisterEvent(UnityAction<YisoMenuInventoryV2DescriptionUI.ButtonTypes, YisoItem> handler) {
            quickButton.onClick.AddListener(() => handler(YisoMenuInventoryV2DescriptionUI.ButtonTypes.QUICK, item));
            dropButton.onClick.AddListener(() => handler(YisoMenuInventoryV2DescriptionUI.ButtonTypes.DROP, item));
            equipOrUseButton.onClick.AddListener(() => {
                YisoMenuInventoryV2DescriptionUI.ButtonTypes type = YisoMenuInventoryV2DescriptionUI.ButtonTypes.USE;
                switch (item.InvType) {
                    case YisoItem.InventoryType.EQUIP:
                        type = YisoMenuInventoryV2DescriptionUI.ButtonTypes.EQUIP;
                        break;
                }
                handler(type, item);
            });
        }

        public void SetItem(YisoItem item) {
            this.item = item;
            if (item.InvType == YisoItem.InventoryType.EQUIP) {
                var equipItem = (YisoEquipItem) item;
                equipDescriptionUI.SetItem(equipItem);
                equipDescriptionUI.Visible(true);
            }
            else {
                otherDescriptionUI.SetItem(item);
                otherDescriptionUI.Visible(true);
            }
            
            ShowButtons();
        }

        public void UpdateCount(int itemId, int count) {
            if (!otherDescriptionUI.Active) return;
            if (otherDescriptionUI.Item.Id != itemId) return;
            otherDescriptionUI.UpdateCount(count);
        }

        private void ShowButtons() {
            dropButton.Visible = true;
            switch (item.InvType) {
                case YisoItem.InventoryType.EQUIP:
                    equipOrUseButton.Visible = true;
                    equipOrUseButton.SetText(CurrentLocale == YisoLocale.Locale.KR ? "장착" : "Equip");
                    break;
                case YisoItem.InventoryType.USE:
                    equipOrUseButton.Visible = true;
                    quickButton.Visible = true;
                    equipOrUseButton.SetText(CurrentLocale == YisoLocale.Locale.KR ? "사용" : "Use");
                    break;
            }
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

            dropButton.Visible = false;
            quickButton.Visible = false;
            equipOrUseButton.Visible = false;
        }
    }
}