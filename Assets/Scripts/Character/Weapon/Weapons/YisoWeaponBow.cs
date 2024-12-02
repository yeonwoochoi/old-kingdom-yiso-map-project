using Character.Core;
using Character.Weapon.Projectiles;
using Character.Weapon.Weapons.Base;
using Core.Service;
using Core.Service.Character;
using UnityEngine;

namespace Character.Weapon.Weapons {
    [AddComponentMenu("Yiso/Weapons/Bow Weapon")]
    public class YisoWeaponBow : YisoWeaponProjectile {
        public override AttackType GetAttackType() {
            return AttackType.Shoot;
        }

        protected override YisoProjectile GetOneProjectile() {
            if (owner.characterType == YisoCharacter.CharacterTypes.Player) {
                return YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule
                    .TryUseArrow(1, out var reason)
                    ? PoolService.SpawnObject<YisoProjectile>(projectilePrefab)
                    : null;
            }

            return PoolService.SpawnObject<YisoProjectile>(projectilePrefab);
        }
    }
}