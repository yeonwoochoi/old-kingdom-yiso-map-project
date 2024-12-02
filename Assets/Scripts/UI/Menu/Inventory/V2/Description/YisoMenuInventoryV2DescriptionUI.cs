using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Types;
using UI.Base;
using UI.Menu.Inventory.V2.Description.Equip;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Inventory.V2.Description {
    public class YisoMenuInventoryV2DescriptionUI : YisoPlayerUIController {
        [SerializeField] private YisoMenuInventoryV2EquipDescriptionUI equippedDescriptionUI;
        [SerializeField] private YisoMenuInventoryV2InvDescriptionUI inventoryDescriptionUI;
        [SerializeField] private Button unEquipButton;
        
        public event UnityAction<ButtonTypes, YisoItem> OnClickButtonEvent; 

        private CanvasGroup canvasGroup;

        protected override void Start() {
            base.Start();
            canvasGroup = GetComponent<CanvasGroup>();
            unEquipButton.onClick.AddListener(() => RaiseEvent(ButtonTypes.UN_EQUIP, equippedDescriptionUI.Item));
            inventoryDescriptionUI.RegisterEvent(RaiseEvent);
        }

        public void Visible(bool flag) {
            canvasGroup.Visible(flag);
        }

        public void SetItem(YisoItem item) {
            inventoryDescriptionUI.SetItem(item);
            if (item.InvType == YisoItem.InventoryType.EQUIP) {
                var equipItem = (YisoEquipItem) item;
                if (equipItem.Slot == YisoEquipSlots.WEAPON) {
                    var currentWeapon = player.InventoryModule.GetCurrentEquippedWeaponItem();
                    if (currentWeapon != null) {
                        if (equipItem.AttackType == currentWeapon.AttackType) {
                            equippedDescriptionUI.SetItem(currentWeapon);
                            equippedDescriptionUI.Visible(true);
                        }
                    }
                } else {
                    if (player.InventoryModule.EquippedUnit.TryGetItem(equipItem.Slot, out var equippedItem)) {
                        equippedDescriptionUI.SetItem(equippedItem);
                        equippedDescriptionUI.Visible(true);
                    }
                }
            }
            inventoryDescriptionUI.Visible(true);
        }

        public void SetEquippedItem(YisoEquipItem item) {
            equippedDescriptionUI.SetItem(item);
            equippedDescriptionUI.Visible(true);
        }

        public void HideEquippedDescriptionIfVisible() {
            if (equippedDescriptionUI.Active)
                equippedDescriptionUI.Visible(false);
        }

        public void UpdateCount(int itemId, int count) {
            inventoryDescriptionUI.UpdateCount(itemId, count);
        }

        public void Clear() {
            if (equippedDescriptionUI.Active) {
                equippedDescriptionUI.Clear();
                equippedDescriptionUI.Visible(false);
            }
            
            inventoryDescriptionUI.Clear();
            inventoryDescriptionUI.Visible(false);
        }

        private void RaiseEvent(ButtonTypes type, YisoItem item) {
            OnClickButtonEvent?.Invoke(type, item);
        }

        public enum ButtonTypes {
            EQUIP, USE, UN_EQUIP, DROP, QUICK
        }
    }
}