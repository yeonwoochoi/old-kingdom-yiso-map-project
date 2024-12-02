using Character.Weapon.Weapons.Base;
using UnityEngine;

namespace Character.Weapon.Weapons {
    [AddComponentMenu("Yiso/Weapons/None Weapon")]
    public class YisoWeaponNone : YisoWeaponMelee {
        public override AttackType GetAttackType() {
            return AttackType.None;
        }
    }
}