using System.Collections;
using System.Linq;
using Character.Ability;
using Character.Core;
using Character.Weapon.Aim;
using Character.Weapon.Weapons.Base;
using Sirenix.OdinInspector;
using Tools.AI.Utils;
using Unity.VisualScripting;
using UnityEngine;
using Utils.Beagle;

namespace Tools.AI.Actions.Attack {
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionAttack")]
    public class YisoAIActionAttack : YisoAIAction {
        [Title("Behaviour")] [ReadOnly] public Vector3 weaponAimDirection;
        public YisoAIBrain.AITargetType aiTargetType = YisoAIBrain.AITargetType.Main;

        protected Transform Target {
            get {
                return aiTargetType switch {
                    YisoAIBrain.AITargetType.Main => brain.mainTarget,
                    YisoAIBrain.AITargetType.Sub => brain.subTarget,
                    YisoAIBrain.AITargetType.SpawnPosition => brain.spawnPositionTarget,
                    _ => null
                };
            }
        }

        public bool Attacking => attacking;

        protected YisoWeaponAim weaponAim;
        protected YisoCharacterOrientation2D orientation2D;
        protected YisoCharacterHandleWeapon targetHandleWeaponAbility;
        protected YisoCharacter character;

        protected YisoWeaponProjectile projectileWeapon;
        protected int numberOfShoots = 0;
        protected bool attacking = false;

        protected bool hasMultipleAttackActions = false;
        protected YisoAIUtilAttackAimingController attackAimingController;
        protected YisoAIActionAttack[] attackActions;

        protected virtual bool FaceTargetWhenAttacking => true;
        protected virtual bool AimAtTargetWhenEnterState => true;
        protected virtual bool AimAtTargetDuringState => false;

        #region Initialization

        public override void Initialization() {
            if (!ShouldInitialize) return;
            base.Initialization();
            character = gameObject.GetComponentInParent<YisoCharacter>();
            orientation2D = character?.FindAbility<YisoCharacterOrientation2D>();

            if (targetHandleWeaponAbility == null) {
                targetHandleWeaponAbility = character?.FindAbility<YisoCharacterHandleWeapon>();
            }

            weaponAim = targetHandleWeaponAbility?.CurrentWeaponAim;
            StartCoroutine(InitializeWeaponAimCo());

            attackActions = brain.GetComponentsInChildren<YisoAIActionAttack>();
            hasMultipleAttackActions = attackActions.Length > 1;
            if (hasMultipleAttackActions) InitializeAttackAimingController();
        }

        protected virtual IEnumerator InitializeWeaponAimCo() {
            while (weaponAim == null) {
                if (targetHandleWeaponAbility.currentWeapon != null) {
                    weaponAim = targetHandleWeaponAbility.currentWeapon.gameObject
                        .YisoGetComponentNoAlloc<YisoWeaponAim>();
                }

                yield return null;
            }
        }

        protected virtual void InitializeAttackAimingController() {
            if (attackAimingController != null && !attackAimingController.ShouldInitialized) return;
            attackAimingController = gameObject.GetOrAddComponent<YisoAIUtilAttackAimingController>();
            attackAimingController.Setup(attackActions.ToList(), orientation2D, weaponAim);
        }

        protected virtual void InitializeWeapon() {
            if (targetHandleWeaponAbility.currentWeapon != null) {
                // TODO(최적화): enemy가 weapon change 할 일은 없을 것 같지만.. 나중에 최적화하기
                weaponAim = targetHandleWeaponAbility.currentWeapon.gameObject.YisoGetComponentNoAlloc<YisoWeaponAim>();
                projectileWeapon = targetHandleWeaponAbility.currentWeapon.gameObject
                    .YisoGetComponentNoAlloc<YisoWeaponProjectile>();
            }
        }

        public override void OnEnterState() {
            base.OnEnterState();
            numberOfShoots = 0;
            attacking = true;
            InitializeWeapon();
            AimAtTarget();
            SetWeaponInterrupted();
            FaceTarget(weaponAimDirection);
        }

        public override void OnExitState() {
            base.OnExitState();
            if (targetHandleWeaponAbility != null) {
                targetHandleWeaponAbility.ForceStop();
            }

            attacking = false;
        }

        #endregion

        #region core

        public override void PerformAction() {
            Attack();
        }

        protected virtual void AimAtTarget() {
            if (!AimAtTargetWhenEnterState) return;
            if (targetHandleWeaponAbility.currentWeapon != null) {
                if (weaponAim == null) {
                    weaponAim = targetHandleWeaponAbility.currentWeapon.gameObject
                        .YisoGetComponentNoAlloc<YisoWeaponAim>();
                }

                if (weaponAim != null && projectileWeapon != null) {
                    projectileWeapon.DetermineSpawnPosition();
                    weaponAimDirection = Target.position - projectileWeapon.spawnPosition;
                }
                else {
                    weaponAimDirection = Target.position - brain.owner.transform.position;
                }
            }
        }

        protected virtual void SetWeaponInterrupted() {
            if (targetHandleWeaponAbility.currentWeapon != null) {
                targetHandleWeaponAbility.currentWeapon.canDelayBetweenUsesInterrupted = true;
            }
        }

        protected virtual void FaceTarget(Vector2 direction) {
            if (!FaceTargetWhenAttacking) return;
            YisoCharacter.FacingDirections newDirection;
            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)) {
                newDirection = direction.x < 0
                    ? YisoCharacter.FacingDirections.West
                    : YisoCharacter.FacingDirections.East;
            }
            else {
                newDirection = direction.y < 0
                    ? YisoCharacter.FacingDirections.South
                    : YisoCharacter.FacingDirections.North;
            }
            
            orientation2D.Face(newDirection);
        }

        protected virtual void Attack() {
            if (numberOfShoots < 1) {
                targetHandleWeaponAbility.AttackStart();
                numberOfShoots++;
            }
        }

        #endregion

        #region Update

        public override void OnUpdate() {
            if (AimAtTargetDuringState && !attacking) return;
            if (targetHandleWeaponAbility == null || weaponAim == null) return;
            if (targetHandleWeaponAbility.currentWeapon == null) return;
            if (hasMultipleAttackActions) InitializeAttackAimingController();

            if (attacking) {
                weaponAim.SetCurrentAim(weaponAimDirection);
            }
            else if (!hasMultipleAttackActions) {
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
        }

        #endregion
    }
}