using Character.Ability;
using Character.Core;
using UnityEngine;

namespace Character.Weapon.Aim {
    [RequireComponent(typeof(YisoWeapon))]
    [AddComponentMenu("Yiso/Weapons/Weapon Aim")]
    public class YisoWeaponAim : YisoBaseAim {
        protected YisoWeapon weapon;

        public override void Initialization() {
            if (initialized) return;
            base.Initialization();

            weapon = GetComponent<YisoWeapon>();
            ownerCharacter = weapon.Owner;
            ownerOrientation2D = ownerCharacter?.FindAbility<YisoCharacterOrientation2D>();

            if (ownerOrientation2D != null) {
                hasOrientation2D = true;
                switch (ownerOrientation2D.currentFacingDirection) {
                    case YisoCharacter.FacingDirections.West:
                        lastNonNullMovement = Vector2.left;
                        break;
                    case YisoCharacter.FacingDirections.East:
                        lastNonNullMovement = Vector2.right;
                        break;
                    case YisoCharacter.FacingDirections.North:
                        lastNonNullMovement = Vector2.up;
                        break;
                    case YisoCharacter.FacingDirections.South:
                        lastNonNullMovement = Vector2.down;
                        break;
                }
            }

            initialized = true;
        }
    }
}