using System;
using Character.Weapon;
using Core.Domain.Actor.Player.Modules.Inventory;
using Core.Domain.Item;
using Core.Domain.Types;
using Core.Service;
using Core.Service.UI.Popup;
using Sirenix.OdinInspector;
using TMPro;
using UI.Menu.Base;
using UI.Menu.Inventory.Event;
using UI.Menu.Inventory.V2.Description;
using UI.Menu.Inventory.V2.Equipped;
using UI.Menu.Inventory.V2.Holder;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Inventory.V2 {
    public class YisoMenuInventoryV2ContentUI : YisoMenuBasePanelUI {
        [SerializeField] private Toggle[] tabs;
        [SerializeField] private YisoMenuInventoryV2ItemHoldersUI holdersUI;
        [SerializeField] private YisoMenuInventoryV2EquippedHoldersUI equippedHoldersUI;
        [SerializeField] private YisoMenuInventoryV2DescriptionUI descriptionUI;
        [SerializeField] private TextMeshProUGUI combatRatingText;
        [SerializeField] private Image yisoImage;

        [SerializeField, Title("Yiso Sprites")] private Sprite defaultSprite;
        [SerializeField] private Sprite swordSprite;
        [SerializeField] private Sprite spearSprite;
        [SerializeField] private Sprite bowSprite;

        [SerializeField, Title("Utils")] private Button orderButton;

        private YisoItem.InventoryType currentInvType = YisoItem.InventoryType.EQUIP;
        
        protected override void Start() {
            base.Start();

            for (var i = 0; i < tabs.Length; i++) {
                tabs[i].onValueChanged.AddListener(OnToggleTab(i));
            }
            
            var slotLimit = player.InventoryModule.GetSlotLimit();
            holdersUI.CreateHolders(slotLimit);
            
            orderButton.onClick.AddListener(OnClickOrder);
        }

        protected override void RegisterEvents() {
            base.RegisterEvents();
            player.StatModule.OnWeaponCombatRatingChangedEvent += OnWeaponCombatRatingChanged;
            
            player.InventoryModule.OnInventoryEvent += OnInventoryEvent;
            holdersUI.OnInventoryEvent += OnInventoryEvent;
            equippedHoldersUI.OnInventoryEvent += OnInventoryEvent;
            descriptionUI.OnClickButtonEvent += OnInventoryButtonEvent;
        }

        protected override void UnregisterEvents() {
            base.UnregisterEvents();
            player.StatModule.OnWeaponCombatRatingChangedEvent += OnWeaponCombatRatingChanged;

            player.InventoryModule.OnInventoryEvent -= OnInventoryEvent;
            holdersUI.OnInventoryEvent -= OnInventoryEvent;
            equippedHoldersUI.OnInventoryEvent -= OnInventoryEvent;
            descriptionUI.OnClickButtonEvent -= OnInventoryButtonEvent;
        }

        protected override void OnVisible() {
            OnToggleTab(0)(true);
            equippedHoldersUI.SetItems(player);
            SetYisoSprite();
            var currentWeaponType = player.InventoryModule.GetCurrentEquippedWeaponType();
            var combatRating = player.StatModule.WeaponCombatRatings[currentWeaponType];
            OnWeaponCombatRatingChanged(currentWeaponType, combatRating);
        }

        public override void ClearPanel() {
            if (currentInvType != YisoItem.InventoryType.EQUIP)
                tabs[0].isOn = true;
            else
                holdersUI.Clear();
            equippedHoldersUI.Clear();
            descriptionUI.Clear();
            descriptionUI.Visible(false);
        }

        private void OnCombatRatingChanged(double combatRating) {
            combatRatingText.SetText(Mathf.CeilToInt((float) combatRating).ToCommaString());
        }

        private void OnWeaponCombatRatingChanged(YisoWeapon.AttackType type, double cr) {
            if (player.InventoryModule.GetCurrentEquippedWeaponType() != type) return;
            combatRatingText.SetText(Mathf.CeilToInt((float) cr).ToCommaString());
        }

        private void OnInventoryEvent(YisoPlayerInventoryEventArgs args) {
            var inventory = player.InventoryModule;
            var currentWeapon = player.InventoryModule.GetCurrentEquippedWeaponType();
            switch (args) {
                case YisoPlayerInventoryAddEventArgs addArgs:
                    var addPosition = addArgs.Position;
                    var item = inventory.InventoryUnits[currentInvType][addPosition];
                    holdersUI.SetItem(addPosition, item, currentWeapon);
                    break;
                case YisoPlayerInventoryRemoveEventArgs removeArgs:
                    holdersUI.Clear(removeArgs.Position);
                    break;
                case YisoPlayerInventoryCountEventArgs countArgs:
                    holdersUI.UpdateCount(countArgs.Position, countArgs.AfterCount);
                    descriptionUI.UpdateCount(countArgs.ItemId, countArgs.AfterCount);
                    break;
                case YisoPlayerInventoryEquipEventArgs equipArgs:
                    var equipSlot = equipArgs.Slot;
                    YisoEquipItem equipItem;
                    if (equipSlot == YisoEquipSlots.WEAPON) {
                        equipItem = player.InventoryModule.GetCurrentEquippedWeaponItem();
                        if (equipItem == null) {
                            OnSwitchWeapon(true, true);
                            return;
                        }
                        if (currentWeapon != equipItem.AttackType) return;
                    } else if (!inventory.EquippedUnit.TryGetItem(equipArgs.Slot, out equipItem))
                        throw new Exception("Equip Item must not null!");
                    equippedHoldersUI.EquipItem(equipItem);
                    holdersUI.UpdateDiffs(currentWeapon);
                    SetYisoSprite();
                    break;
                case YisoPlayerInventoryUnEquipEventArgs unEquipArgs:
                    equippedHoldersUI.UnEquipItem(unEquipArgs.Slot);
                    holdersUI.UpdateDiffs(currentWeapon);
                    SetYisoSprite();
                    break;
                case YisoPlayerInventoryReOrderedEventArgs:
                    holdersUI.Clear();
                    SetItems();
                    break;
                case YisoPlayerInventorySwitchWeaponEventArgs switchWeaponArgs:
                    equippedHoldersUI.SetWeaponText(switchWeaponArgs.AfterType);
                    equippedHoldersUI.SetWeapon(switchWeaponArgs.SwitchedWeapon);
                    SetYisoSprite();
                    OnCombatRatingChanged(switchWeaponArgs.CombatRating);
                    holdersUI.UpdateDiffs(switchWeaponArgs.AfterType);
                    break;
            }
        }

        private void OnInventoryEvent(InventoryUIEventArgs args) {
            switch (args) {
                case InventoryUIItemSelectedEventArgs selectedArgs:
                    descriptionUI.HideEquippedDescriptionIfVisible();
                    descriptionUI.SetItem(selectedArgs.Item);
                    descriptionUI.Visible(true);
                    break;
                case InventoryUIEquippedItemSelectEventArgs equippedSelectArgs:
                    descriptionUI.SetEquippedItem(equippedSelectArgs.Item);
                    descriptionUI.Visible(true);
                    break;
                case InventoryUIItemUnSelectedEventArgs:
                case InventoryUIEquippedItemUnSelectEventArgs:
                    descriptionUI.Visible(false);
                    descriptionUI.Clear();
                    break;
                case InventoryUISwitchWeaponEventArgs switchWeaponArgs:
                    OnSwitchWeapon(switchWeaponArgs.IsWeaponSelected);
                    break;
            }
        }
        
        private void OnInventoryButtonEvent(YisoMenuInventoryV2DescriptionUI.ButtonTypes type, YisoItem item) {
            var popupService = YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
            switch (type) {
                case YisoMenuInventoryV2DescriptionUI.ButtonTypes.EQUIP:
                    player.InventoryModule.EquipItem(item as YisoEquipItem);
                    break;
                case YisoMenuInventoryV2DescriptionUI.ButtonTypes.UN_EQUIP:
                    player.InventoryModule.UnEquipItem(item as YisoEquipItem);
                    break;
                case YisoMenuInventoryV2DescriptionUI.ButtonTypes.USE:
                    player.InventoryModule.UseItem(item as YisoUseItem);
                    break;
                case YisoMenuInventoryV2DescriptionUI.ButtonTypes.DROP:
                    if (item is YisoEquipItem) {
                        player.InventoryModule.DropItem(item);
                        return;
                    }
                    popupService.ShowDropItemCount(item, value => {
                        player.InventoryModule.DropItem(item, value);
                    });
                    break;
                case YisoMenuInventoryV2DescriptionUI.ButtonTypes.QUICK:
                    break;
            }
        }

        public override YisoMenuTypes GetMenuType() => YisoMenuTypes.INVENTORY;
        
        private void OnClickOrder() {
            descriptionUI.Clear();
            player.InventoryModule.ReOrderItems(currentInvType);
        }

        private void SetYisoSprite() {
            if (!player.InventoryModule.TryGetWeapon(out var equipItem)) {
                yisoImage.sprite = defaultSprite;
                return;
            }

            yisoImage.sprite = equipItem.AttackType switch {
                YisoWeapon.AttackType.Shoot => bowSprite,
                YisoWeapon.AttackType.Thrust => spearSprite,
                _ => swordSprite
            };
        }
        
        private void OnSwitchWeapon(bool isWeaponSelected, bool forceExist = false) {
            if (isWeaponSelected) {
                descriptionUI.Visible(false);
                descriptionUI.Clear();
            }
            
            if (forceExist) 
                player.InventoryModule.SwitchExistWeapon();
            else 
                player.InventoryModule.SwitchWeapon();
        }


        private UnityAction<bool> OnToggleTab(int index) => flag => {
            if (!flag) {
                holdersUI.Clear();
                return;
            }

            currentInvType = index switch {
                0 => YisoItem.InventoryType.EQUIP,
                1 => YisoItem.InventoryType.USE,
                _ => YisoItem.InventoryType.ETC
            };
            SetItems();
        };

        private void SetItems() {
            var units = player.InventoryModule.InventoryUnits[currentInvType];
            foreach (var (position, item) in units.ItemDict) {
                holdersUI.SetItem(position, item, player.InventoryModule.GetCurrentEquippedWeaponType());
            }
        }
    }
}