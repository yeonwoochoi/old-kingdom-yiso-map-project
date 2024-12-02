using Character.Ability;
using Character.Core;
using UnityEngine;

namespace Character.Weapon.Aim {
    [AddComponentMenu("Yiso/Weapons/Weapon Aim")]
    public class YisoSkillAim : YisoBaseAim {
        public virtual void Initialization(YisoCharacter owner, AimControls control) {
            Initialization();
            ownerCharacter = owner;
            ownerOrientation2D = ownerCharacter?.FindAbility<YisoCharacterOrientation2D>();
            aimControl = control;
            initialized = true;
        }
    }
}