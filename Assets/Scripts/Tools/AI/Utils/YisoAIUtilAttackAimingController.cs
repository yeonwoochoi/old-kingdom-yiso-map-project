using System.Collections.Generic;
using System.Linq;
using Character.Ability;
using Character.Core;
using Character.Weapon;
using Character.Weapon.Aim;
using Core.Behaviour;
using Tools.AI.Actions.Attack;
using UnityEngine;

namespace Tools.AI.Utils {
    /// <summary>
    /// YisoAIActionAttack이 한 오브젝트 내에 2개 이상 있는 경우 둘 다 동시에 current Aim을 Setting하려고 해서 문제가 생김
    /// 따라서 중간에 이 컴포넌트를 추가해두고 attack 상태가 아닌 경우에 Current Aim을 세팅하는 역할을 얘가 맞는 거
    /// </summary>
    [AddComponentMenu("Yiso/Character/AI/Utils/AIUtilAttackAimingController")]
    public class YisoAIUtilAttackAimingController : RunIBehaviour {
        protected List<YisoAIActionAttack> attackActions = new();
        protected YisoCharacterOrientation2D orientation2D;
        protected YisoWeaponAim weaponAim;

        public bool ShouldInitialized => orientation2D == null || weaponAim == null || attackActions.Count == 0;

        public virtual void Setup(List<YisoAIActionAttack> actions, YisoCharacterOrientation2D orientation,
            YisoWeaponAim aim) {
            if (!ShouldInitialized) return;

            attackActions = actions;
            orientation2D = orientation;
            weaponAim = aim;
        }

        public override void OnUpdate() {
            if (ShouldInitialized || attackActions == null || attackActions.Count < 1) return;
            if (IsAnyAttackActionActive()) return;
            if (orientation2D != null) {
                switch (orientation2D.currentFacingDirection) {
                    case YisoCharacter.FacingDirections.West:
                        weaponAim.SetCurrentAim(Vector2.left);
                        break;
                    case YisoCharacter.FacingDirections.East:
                        weaponAim.SetCurrentAim(Vector2.right);
                        break;
                    case YisoCharacter.FacingDirections.North:
                        weaponAim.SetCurrentAim(Vector2.up);
                        break;
                    case YisoCharacter.FacingDirections.South:
                        weaponAim.SetCurrentAim(Vector2.down);
                        break;
                }
            }
        }

        protected virtual bool IsAnyAttackActionActive() {
            return attackActions.Any(actionAttack => actionAttack.Attacking);
        }
    }
}