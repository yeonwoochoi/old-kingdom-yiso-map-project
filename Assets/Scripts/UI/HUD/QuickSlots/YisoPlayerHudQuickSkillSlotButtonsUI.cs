using System;
using System.Collections.Generic;
using Character.Weapon;
using Core.Behaviour;
using Core.Domain.Actor.Player.Modules.Inventory;
using Core.Domain.Actor.Player.Modules.UI;
using Sirenix.OdinInspector;
using UI.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.HUD.QuickSlots {
    public class YisoPlayerHudQuickSkillSlotButtonsUI : YisoPlayerUIController {
        [SerializeField, Title("Slots")] private List<YisoPlayerHudQuickSkillSlotButtonUI> buttons;
        
        protected override void Start() {
            base.Start();
            SetSlots(player.InventoryModule.GetCurrentEquippedWeaponType());
            
            for (var i = 0; i < buttons.Count; i++) 
                buttons[i].SlotButton.onClick.AddListener(OnClickSlotButton(i));
        }

        protected override void OnEnable() {
            base.OnEnable();
            player.UIModule.OnSlotEvent += OnSlotUIEvent;
            player.InventoryModule.OnInventoryEvent += OnInventoryEvent;
        }

        protected override void OnDisable() {
            base.OnDisable();
            player.UIModule.OnSlotEvent -= OnSlotUIEvent;
            player.InventoryModule.OnInventoryEvent -= OnInventoryEvent;
        }

        private void OnInventoryEvent(YisoPlayerInventoryEventArgs args) {
            if (args is not YisoPlayerInventorySwitchWeaponEventArgs switchedArgs) return;
            SetSlots(player.InventoryModule.GetCurrentEquippedWeaponType());
        }

        private void OnSlotUIEvent(YisoPlayerSlotUIEventArgs args) {
            var position = args.Position;
            var currentWeapon = player.InventoryModule.GetCurrentEquippedWeaponType();
            if (args.Weapon != currentWeapon) return;
            switch (args) {
                case YisoPlayerSlotSkillSetEventArgs setArgs:
                    buttons[position].SetSkill(setArgs.Skill);
                    break;
                case YisoPlayerSlotSkillUnSetEventArgs:
                    buttons[position].Clear();
                    break;
                case YisoPlayerSlotSkillReplaceEventArgs replaceArgs:
                    buttons[position].Clear();
                    buttons[position].SetSkill(replaceArgs.Skill);
                    break;
            }
        }

        private UnityAction OnClickSlotButton(int index) => () => {
            player.SkillModule.CastSkill(buttons[index].SkillId);
            buttons[index].CreateCooldown();
        };
        
        private void SetSlots(YisoWeapon.AttackType weapon) {
            for (var i = 0; i < 4; i++) {
                if (!player.UIModule.TryGetSkill(weapon, i, out var skill)) {
                    buttons[i].Clear();
                    continue;
                }
                buttons[i].SetSkill(skill);
            }
        }
    }
}