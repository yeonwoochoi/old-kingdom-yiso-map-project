using Character.Weapon.Weapons.Base;
using UnityEngine;

namespace Character.Weapon.Weapons {
    [AddComponentMenu("Yiso/Weapons/Spear Weapon")]
    public class YisoWeaponSpear : YisoWeaponMelee {
        public override AttackType GetAttackType() {
            return AttackType.Thrust;
        }
    }
}