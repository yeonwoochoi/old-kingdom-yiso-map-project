using System;
using Character;
using Character.Ability;
using Character.Core;
using Character.Weapon;
using Character.Weapon.Aim;
using Tools.Vision;
using UnityEngine;
using Utils.Beagle;

namespace Tools.AI.Actions {
    /// <summary>
    /// This AIAction will rotate this AI's ConeOfVision2D either towards the AI's movement or its weapon aim direction
    /// </summary>
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionRotateConeOfVision")]
    public class YisoAIActionRotateConeOfVision : YisoAIAction {
        public enum ConeOfVisionRotateModes {
            Movement,
            WeaponAim
        }

        public ConeOfVisionRotateModes rotateMode = ConeOfVisionRotateModes.WeaponAim;
        public bool reverse = false;

        protected TopDownController controller;
        protected YisoCharacterHandleWeapon characterHandleWeapon;
        protected YisoWeaponAim weaponAim;
        protected YisoConeOfVision targetConeOfVision;

        public override void Initialization() {
            if (!ShouldInitialize) return;
            base.Initialization();
            controller = gameObject.GetComponentInParent<TopDownController>();
            characterHandleWeapon = gameObject.GetComponentInParent<YisoCharacter>()
                ?.FindAbility<YisoCharacterHandleWeapon>();
            if (targetConeOfVision == null) {
                targetConeOfVision = gameObject.GetComponentInParent<YisoConeOfVision>();
            }
        }

        protected virtual void GrabWeaponAim() {
            if ((characterHandleWeapon != null) && (characterHandleWeapon.currentWeapon != null)) {
                weaponAim = characterHandleWeapon.currentWeapon.gameObject.YisoGetComponentNoAlloc<YisoWeaponAim>();
            }
        }

        public override void OnEnterState() {
            base.OnEnterState();
            GrabWeaponAim();
        }

        public override void PerformAction() {
            Rotate();
        }

        protected virtual void Rotate() {
            if (targetConeOfVision == null) return;
            switch (rotateMode) {
                case ConeOfVisionRotateModes.Movement:
                    AimAt(controller.currentDirection.normalized);
                    break;
                case ConeOfVisionRotateModes.WeaponAim:
                    if (weaponAim == null) GrabWeaponAim();
                    else AimAt(weaponAim.CurrentAim.normalized);
                    break;
            }
        }

        protected virtual void AimAt(Vector3 newAim) {
            var angle = YisoMathUtils.AngleBetween(transform.right, reverse ? -newAim : newAim);
            var eulerAngle = Vector3.zero;
            eulerAngle.y = -angle;
            targetConeOfVision.SetDirectionAndAngles(reverse ? -newAim : newAim, eulerAngle);
        }
    }
}