using Character.Ability;
using Core.Behaviour;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using Utils.Beagle;

namespace Character.Weapon {
    [AddComponentMenu("Yiso/Weapons/Combo Weapon")]
    public class YisoComboWeapon : RunIBehaviour {
        public bool droppableCombo = true; // 일정 시간 지나면 콤보 리셋 시킬건지 여부
        [ShowIf("droppableCombo")] public float dropComboDelay = 0.25f;

        [ReadOnly] public YisoWeapon[] weapons;

        protected bool initialized = false;
        protected YisoCharacterHandleWeapon ownerCharacterHandleWeapon;
        public int currentComboIndex = 0;
        protected bool countDownActive = false;
        protected float lastWeaponStopped;

        protected const string ComboIndexAnimationParameterName = "Combo";
        protected int comboIndexAnimationParameter;

        #region Initialization

        public virtual void Initialization() {
            if (initialized) return;
            initialized = true;
            currentComboIndex = 0;
            weapons = GetComponents<YisoWeapon>();
            InitializeUnusedWeapons();
            InitializeAnimatorParameters();
        }

        protected virtual void InitializeUnusedWeapons() {
            for (var i = 0; i < weapons.Length; i++) {
                if (i != currentComboIndex) {
                    weapons[i].SetOwner(weapons[currentComboIndex].Owner,
                        weapons[currentComboIndex].CharacterHandleWeapon);
                    weapons[i].Initialization();
                    weapons[i].weaponActive = false;
                }
                else {
                    weapons[i].weaponActive = true;
                }
            }
            weapons[currentComboIndex].AttachWeaponToModelParent();
        }

        #endregion

        #region Reset (Update)

        public override void OnUpdate() {
            ResetCombo();
        }

        protected virtual void ResetCombo() {
            if (weapons.Length > 1) {
                if (countDownActive && droppableCombo) {
                    lastWeaponStopped += Time.deltaTime;
                    if (lastWeaponStopped > dropComboDelay) {
                        countDownActive = false;

                        currentComboIndex = 0;
                        ownerCharacterHandleWeapon.currentWeapon = weapons[currentComboIndex];
                        ownerCharacterHandleWeapon.ChangeWeapon(weapons[currentComboIndex], true);
                        UpdateAnimator();
                    }
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// When one of the weapons get used we turn our countdown off
        /// </summary>
        public virtual void WeaponStarted() {
            countDownActive = false;
        }

        /// <summary>
        /// When one of the weapons has ended its attack, we start our countdown and switch to the next weapon
        /// </summary>
        public virtual void WeaponStopped() {
            ProceedToNextCombo();
        }

        protected virtual void ProceedToNextCombo() {
            ownerCharacterHandleWeapon = weapons[currentComboIndex].CharacterHandleWeapon;
            var newIndex = 0;
            if (ownerCharacterHandleWeapon != null) {
                if (weapons.Length > 1) {
                    if (currentComboIndex >= weapons.Length - 1) {
                        newIndex = 0;
                    }
                    else {
                        newIndex = currentComboIndex + 1;
                    }

                    countDownActive = true;
                    lastWeaponStopped = 0f;

                    currentComboIndex = newIndex;
                    ownerCharacterHandleWeapon.currentWeapon = weapons[newIndex];
                    ownerCharacterHandleWeapon.currentWeapon.weaponActive = false;
                    ownerCharacterHandleWeapon.ChangeWeapon(weapons[newIndex], true);
                    ownerCharacterHandleWeapon.currentWeapon.weaponActive = true;
                    UpdateAnimator();
                }
            }
        }

        #endregion

        #region Animator

        protected virtual void InitializeAnimatorParameters() {
            if (weapons is {Length: > 0}) {
                weapons[currentComboIndex].CharacterHandleWeapon.RegisterAnimatorParameter(
                    ComboIndexAnimationParameterName, AnimatorControllerParameterType.Int,
                    out comboIndexAnimationParameter);
            }
        }

        protected virtual void UpdateAnimator() {
            YisoAnimatorUtils.UpdateAnimatorInteger(ownerCharacterHandleWeapon.CharacterAnimator,
                comboIndexAnimationParameter, currentComboIndex);
        }

        #endregion
    }
}