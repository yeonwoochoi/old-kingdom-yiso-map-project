using Character.Weapon;
using Core.Behaviour;
using Core.Domain.Actor.Player.Modules.Inventory;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Menu.Inventory.Event;
using UI.Menu.Stats.Event;
using UnityEngine;
using Utils.Extensions;

namespace UI.Menu {
    public class YisoMenuPlayerStatsUI : YisoPlayerUIController {
        [SerializeField, Title("Stats")] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI attackText;
        [SerializeField] private TextMeshProUGUI defenceText;
        [SerializeField] private TextMeshProUGUI combatRatingText;
        [SerializeField] private TextMeshProUGUI moneyText;

        protected override void Start() {
            moneyText.SetText(player.InventoryModule.Money.ToCommaString());
            
            var combatRating = player.StatModule.WeaponCombatRatings[player.InventoryModule.GetCurrentEquippedWeaponType()];
            combatRatingText.SetText(Mathf.CeilToInt((float) combatRating).ToCommaString());
        }

        protected override void OnEnable() {
            base.OnEnable();
            player.StatModule.OnStatsUIEvent += OnStatsEvent;
            player.InventoryModule.OnInventoryEvent += OnInventoryEvent;
            player.InventoryModule.OnMoneyChanged += OnMoneyChanged;
        }

        protected override void OnDisable() {
            base.OnDisable();
            player.StatModule.OnStatsUIEvent -= OnStatsEvent;
            player.InventoryModule.OnInventoryEvent -= OnInventoryEvent;
            player.InventoryModule.OnMoneyChanged -= OnMoneyChanged;
        }

        private void OnMoneyChanged(double money) {
            moneyText.SetText(money.ToCommaString());
        }

        private void OnInventoryEvent(YisoPlayerInventoryEventArgs args) {
            if (args is not YisoPlayerInventorySwitchWeaponEventArgs switchWeaponArgs) return;

            var (attack, defence) = player.StatModule.WeaponAttackDefences[switchWeaponArgs.AfterType];
            var combatRating = player.StatModule.WeaponCombatRatings[switchWeaponArgs.AfterType];
            
            SetValues(attack, defence, combatRating);
        }

        private void OnStatsEvent(StatsUIEventArgs args) {
            if (args.Weapon != YisoWeapon.AttackType.None) {
                if (args.Weapon != player.InventoryModule.GetCurrentEquippedWeaponType()) return;
            }
            
            SetValues(args.Attack, args.Defence, args.CombatRating);
        }

        private void SetValues(int attack, int defence, double combatRating) {
            attackText.SetText(attack.ToCommaString());
            defenceText.SetText(defence.ToCommaString());
            combatRatingText.SetText(Mathf.CeilToInt((float) combatRating).ToCommaString());
        }
    }
}