using Character.Ability;
using Character.Weapon;
using UnityEngine;

namespace Tools.AI.Actions.Attack {
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionChangeWeapon")]
    public class YisoAIActionChangeWeapon: YisoAIAction {
        public YisoWeapon weapon;

        protected YisoCharacterHandleWeapon characterHandleWeapon;

        public override void Initialization() {
            base.Initialization();
            characterHandleWeapon = brain.owner?.FindAbility<YisoCharacterHandleWeapon>();
        }
        
        public override void PerformAction() {
            if (characterHandleWeapon == null) return;
            if (characterHandleWeapon.currentWeapon == weapon) return;
            characterHandleWeapon.ChangeWeapon(weapon);
        }
    }
}