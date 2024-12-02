using Character.Weapon.Weapons.Base;
using UnityEngine;

namespace Character.Weapon.Weapons {
    [AddComponentMenu("Yiso/Weapons/Sword Weapon")]
    public class YisoWeaponSword : YisoWeaponMelee {
        public override AttackType GetAttackType() {
            return AttackType.Slash;
        }
    }
}