using System;
using Character.Weapon;

namespace UI.Menu.Stats.Event {
    public sealed class StatsUIEventArgs : EventArgs {
        public YisoWeapon.AttackType Weapon { get; } = YisoWeapon.AttackType.None;
        public int Attack { get; }
        public int Defence { get; }
        public double CombatRating { get; }

        public StatsUIEventArgs(int attack, int defence, double combatRating) {
            Attack = attack;
            Defence = defence;
            CombatRating = combatRating;
        }

        public StatsUIEventArgs(YisoWeapon.AttackType weapon, int attack, int defence, double combatRating) : this(
            attack, defence, combatRating) {
            Weapon = weapon;
        }
     }
}