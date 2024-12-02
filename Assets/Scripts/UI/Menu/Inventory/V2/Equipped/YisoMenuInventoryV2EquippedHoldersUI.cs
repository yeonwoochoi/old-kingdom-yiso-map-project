using System;
using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Behaviour;
using Core.Domain.Actor.Player;
using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Menu.Inventory.Event;
using UI.Menu.Inventory.V2.Holder;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Inventory.V2.Equipped {
    public class YisoMenuInventoryV2EquippedHoldersUI : YisoUIController {
        [SerializeField, Title("Holders")] private YisoMenuInventoryV2ItemHolderUI hatHolder;
        [SerializeField] private YisoMenuInventoryV2ItemHolderUI topHolder;
        [SerializeField] private YisoMenuInventoryV2ItemHolderUI bottomHolder;
        [SerializeField] private YisoMenuInventoryV2ItemHolderUI weaponHolder;
        [SerializeField] private YisoMenuInventoryV2ItemHolderUI glovesHolder;
        [SerializeField] private YisoMenuInventoryV2ItemHolderUI shoesHolder;
        [SerializeField, Title("Toggle")] private ToggleGroup toggleGroup;
        [SerializeField, Title("Weapon")] private Button weaponSwitchButton;
        [SerializeField] private TextMeshProUGUI weaponTitleText;

        private readonly Dictionary<YisoEquipSlots, YisoMenuInventoryV2ItemHolderUI> holders = new();

        private YisoEquipItem selectedItem = null;
        
        public event UnityAction<InventoryUIEventArgs> OnInventoryEvent;

        protected override void Start() {
            holders[YisoEquipSlots.HAT] = hatHolder;
            holders[YisoEquipSlots.TOP] = topHolder;
            holders[YisoEquipSlots.BOTTOM] = bottomHolder;
            holders[YisoEquipSlots.WEAPON] = weaponHolder;
            holders[YisoEquipSlots.GLOVE] = glovesHolder;
            holders[YisoEquipSlots.SHOES] = shoesHolder;

            foreach (var slot in EnumExtensions.Values<YisoEquipSlots>()) {
                holders[slot].Init();
                holders[slot].ItemToggle.group = toggleGroup;
                holders[slot].ItemToggle.onValueChanged.AddListener(OnToggleHolder(slot));
            }
            
            weaponSwitchButton.onClick.AddListener(OnClickSwitchWeapon);
        }

        public void SetItems(YisoPlayer player) {
            foreach (var slot in EnumExtensions.Values<YisoEquipSlots>().Where(t => t != YisoEquipSlots.WEAPON)) {
                if (!player.InventoryModule.EquippedUnit.TryGetItem(slot, out var equipItem)) continue;
                EquipItem(equipItem);
            }

            var currentWeapon = player.InventoryModule.GetCurrentEquippedWeaponType();
            if (player.InventoryModule.TryGetWeapon(out var weaponItem)) {
                EquipItem(weaponItem);
            }
            
            SetWeaponText(currentWeapon);
        }

        public void EquipItem(YisoItem item) {
            var equipItem = (YisoEquipItem)item;
            holders[equipItem.Slot].SetItem(item, YisoWeapon.AttackType.None);
        }

        public void SetWeapon(YisoEquipItem weaponItem) {
            if (weaponItem == null) {
                holders[YisoEquipSlots.WEAPON].Clear();
                return;
            }
            
            EquipItem(weaponItem);
        }

        public void UnEquipItem(YisoEquipSlots slot) {
            holders[slot].Clear();
        }

        public void SetWeaponText(YisoWeapon.AttackType type) {
            var weaponStr = type.ToString(CurrentLocale);
            var typeStr = CurrentLocale == YisoLocale.Locale.KR ? "무기" : "Weapon";
            weaponTitleText.SetText($"{typeStr}({weaponStr})");
        }

        public void Clear() {
            foreach (var slot in EnumExtensions.Values<YisoEquipSlots>()) {
                holders[slot].Clear();
            }
        }

        private void OnClickSwitchWeapon() {
            var isWeaponSelected = false;
            if (selectedItem != null) {
                isWeaponSelected = selectedItem.Slot == YisoEquipSlots.WEAPON;
            }
            OnInventoryEvent?.Invoke(new InventoryUISwitchWeaponEventArgs(isWeaponSelected));
        }

        private UnityAction<bool> OnToggleHolder(YisoEquipSlots slot) => flag => {
            if (!flag) {
                selectedItem = null;
                OnInventoryEvent?.Invoke(new InventoryUIEquippedItemUnSelectEventArgs());
                return;
            }

            selectedItem = holders[slot].GetItem<YisoEquipItem>();
            OnInventoryEvent?.Invoke(new InventoryUIEquippedItemSelectEventArgs(selectedItem));
        };
    }
}