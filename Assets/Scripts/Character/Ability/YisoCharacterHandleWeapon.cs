using Character.Core;
using Character.Weapon;
using Character.Weapon.Aim;
using Core.Domain.Actor.Player.Modules.Inventory;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Character;
using Sirenix.OdinInspector;
using Tools.Cool;
using Tools.Feedback;
using Tools.Feedback.Core;
using Tools.Inputs;
using UnityEngine;
using Utils.Beagle;

namespace Character.Ability {
    [AddComponentMenu("Yiso/Character/Abilities/Character Handle Weapon")]
    public class YisoCharacterHandleWeapon : YisoCharacterAbility {
        [Title("Weapon")] public Transform weaponAttachment;
        public YisoWeapon initialWeapon;
        [ReadOnly] public YisoWeapon currentWeapon;

        [Title("Feedback")] public YisoFeedBacks weaponUseFeedbacks;

        [Title("Input")] public bool continuousPressAttack;

        [Title("Weapon Change")] public YisoCooldown weaponChangeCooldown;

        public delegate void OnWeaponChangeDelegate();

        public OnWeaponChangeDelegate onWeaponChange;

        public Animator CharacterAnimator { get; set; }
        public GameObject CurrentWeaponModel { get; set; }
        public YisoWeaponAim CurrentWeaponAim => currentWeaponAim;
        public bool AttackForbidden { get; set; } = false;

        protected float attackSpeed = 1f;
        public float AttackSpeed {
            get {
                var speed = attackSpeed;
                if (currentWeapon != null) {
                    speed *= currentWeapon.attackSpeed;
                }
                return speed;
            }
            set => attackSpeed = value;
        }

        protected YisoWeaponAim currentWeaponAim;
        protected YisoWeapon lastWeapon;
        protected YisoWeapon weaponBeforeDeath = null;

        protected const string AttackAnimationParameterName = "IsAttack";
        protected const string AttackMoveAnimationParameterName = "IsAttackMove";
        protected const string AttackTypeAnimationParameterName = "AttackType";
        protected const string AttackSpeedAnimationParameterName = "AttackSpeed";

        protected int attackAnimationParameter;
        protected int attackMoveAnimationParameter;
        protected int attackTypeAnimationParameter;
        protected int attackSpeedAnimationParameter;

        #region Initialization

        protected override void Initialization() {
            base.Initialization();
            Setup();
        }

        /// <summary>
        /// Respawn 되는 경우 필요한 것만 Init하기 위해 Initialization과 분리함
        /// </summary>
        public virtual void Setup() {
            CharacterAnimator = animator;
            weaponChangeCooldown.Initialization();

            // Respawn인 경우
            if (weaponBeforeDeath != null) {
                ChangeWeapon(weaponBeforeDeath);
                lastWeapon = weaponBeforeDeath;
                return;
            }

            // Initialization인 경우
            switch (character.characterType) {
                case YisoCharacter.CharacterTypes.Player:
                    ChangeWeapon(GetPlayerEquippedWeapon());
                    lastWeapon = currentWeapon != null ? currentWeapon : initialWeapon;
                    break;
                case YisoCharacter.CharacterTypes.AI:
                    ChangeWeapon(initialWeapon);
                    break;
            }
            
            ResetAttackSpeed();
        }

        protected override void InitializeAnimatorParameters() {
            base.InitializeAnimatorParameters();
            RegisterAnimatorParameter(AttackAnimationParameterName, AnimatorControllerParameterType.Bool,
                out attackAnimationParameter);
            RegisterAnimatorParameter(AttackMoveAnimationParameterName, AnimatorControllerParameterType.Bool,
                out attackMoveAnimationParameter);
            RegisterAnimatorParameter(AttackTypeAnimationParameterName, AnimatorControllerParameterType.Int,
                out attackTypeAnimationParameter);
            RegisterAnimatorParameter(AttackSpeedAnimationParameterName, AnimatorControllerParameterType.Float,
                out attackSpeedAnimationParameter);
        }

        protected override void OnEnable() {
            base.OnEnable();
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule.OnInventoryEvent += OnChangeWeapon;
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().StatModule.OnAttackSpeedChangedEvent += ResetAttackSpeed;
        }

        protected override void OnDisable() {
            base.OnDisable();
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule.OnInventoryEvent -= OnChangeWeapon;
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().StatModule.OnAttackSpeedChangedEvent -= ResetAttackSpeed;
        }

        protected virtual void OnChangeWeapon(YisoPlayerInventoryEventArgs args) {
            if (character.characterType != YisoCharacter.CharacterTypes.Player) return;
            var equippedWeaponType = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule.GetCurrentEquippedWeaponType();
            switch (args) {
                case YisoPlayerInventoryEquipEventArgs equipArgs:
                    if (equipArgs.Slot != YisoEquipSlots.WEAPON) return;
                    if (equippedWeaponType != equipArgs.AttackType) return;
                    ChangeWeapon(GetPlayerEquippedWeapon());
                    break;
                case YisoPlayerInventoryUnEquipEventArgs unEquipArgs:
                    if (unEquipArgs.Slot != YisoEquipSlots.WEAPON) return;
                    if (equippedWeaponType != unEquipArgs.AttackType) return;
                    ChangeWeapon(GetPlayerEquippedWeapon());
                    break;
                case YisoPlayerInventorySwitchWeaponEventArgs switchedWeaponArgs:
                    ChangeWeapon(GetPlayerEquippedWeapon());
                    break;
            }
        }

        protected virtual YisoWeapon GetPlayerEquippedWeapon() {
            if (character.characterType == YisoCharacter.CharacterTypes.AI) return null;
            var equippedWeapon = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule
                .GetCurrentEquippedWeaponItem();
            return equippedWeapon == null ? initialWeapon : equippedWeapon.EquippedPrefab.GetComponent<YisoWeapon>();
        }

        #endregion

        #region Update (PreProcess)

        protected override void HandleInput() {
            base.HandleInput();
            
            if (!AbilityAuthorized || AttackForbidden ||
                conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Normal) {
                return;
            }

            if (currentWeapon != null) {
                var inputAuthorized = currentWeapon.weaponActive;

                if (currentWeapon.WeaponAim.aimControl == YisoBaseAim.AimControls.Mouse) {
                    if (inputAuthorized &&
                        inputManager.LeftMouseClick.State.CurrentState == YisoInput.ButtonStates.ButtonDown) {
                        AttackStart();
                    }

                    if (inputAuthorized && continuousPressAttack && inputManager.LeftMouseClick.State.CurrentState ==
                        YisoInput.ButtonStates.ButtonPressed) {
                        AttackStart();
                    }

                    if (inputAuthorized &&
                        inputManager.LeftMouseClick.State.CurrentState == YisoInput.ButtonStates.ButtonUp) {
                        AttackStop();
                    }

                    if (inputManager.LeftMouseClick.State.CurrentState == YisoInput.ButtonStates.Off) {
                        AttackStop();
                    }
                }
                else {
                    if (inputAuthorized && inputManager.AttackButton.State.CurrentState ==
                        YisoInput.ButtonStates.ButtonDown) {
                        AttackStart();
                    }

                    if (inputAuthorized && continuousPressAttack && inputManager.AttackButton.State.CurrentState ==
                        YisoInput.ButtonStates.ButtonPressed) {
                        AttackStart();
                    }

                    if (inputAuthorized &&
                        inputManager.AttackButton.State.CurrentState == YisoInput.ButtonStates.ButtonUp) {
                        AttackStop();
                    }

                    if (inputManager.AttackButton.State.CurrentState == YisoInput.ButtonStates.Off) {
                        AttackStop();
                    }
                }
            }
        }

        #endregion

        #region Update (Process)

        public override void PreProcessAbility() {
            base.PreProcessAbility();
            UpdateWeaponVisibility();
        }

        public override void ProcessAbility() {
            base.ProcessAbility();
            weaponChangeCooldown.Update();
            CheckCharacterConditionState();
            HandleFeedback();
            HandleBuffer();
            HandleSwitchWeaponButtonInput();
        }

        protected virtual void CheckCharacterConditionState() {
            if (conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Normal) {
                AttackStop();
            }
        }

        protected virtual void HandleFeedback() {
            if (currentWeapon != null) {
                if (currentWeapon.weaponState.CurrentState == YisoWeapon.WeaponStates.Use) {
                    weaponUseFeedbacks?.PlayFeedbacks();
                }
            }
        }

        protected virtual void HandleBuffer() {
            // TODO : Buffer 추가할지 말지 테스트해보고 결정
        }

        protected virtual void HandleSwitchWeaponButtonInput() {
            if (inputManager == null) return;
            if (inputManager.SwitchWeaponButton.State.CurrentState == YisoInput.ButtonStates.ButtonDown) {
                YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule.SwitchWeapon();
            }
        }

        #endregion

        #region Core

        protected Vector3 lastWeaponAim;

        public virtual void ChangeWeapon(YisoWeapon newWeapon, bool combo = false) {
            if (currentWeapon != null && newWeapon != null) {
                if (currentWeapon == newWeapon) return;
            }

            if (currentWeapon != null) {
                currentWeapon.AttackStop();
                if (!combo) {
                    AttackStop();
                    lastWeaponAim = currentWeapon.WeaponAim.CurrentAim;
                    Destroy(currentWeapon.gameObject);
                }
            }

            weaponChangeCooldown.Start();

            if (newWeapon != null) {
                if (!combo) {
                    currentWeapon = Instantiate(newWeapon, weaponAttachment.transform.position,
                        weaponAttachment.transform.rotation);
                }

                currentWeapon.name = newWeapon.name;
                currentWeapon.SetOwner(character, this);
                currentWeapon.Initialization();
                currentWeapon.InitializeComboWeapons();
                currentWeaponAim = currentWeapon.gameObject.YisoGetComponentNoAlloc<YisoWeaponAim>();
                currentWeaponAim.ApplyAim(lastWeaponAim, !combo);
                currentWeapon.transform.parent = weaponAttachment.transform;
            }
            else {
                currentWeapon = null;
            }

            onWeaponChange?.Invoke();
        }

        public virtual void AttackStart() {
            if (!CanAttack()) return;

            PlayAbilityStartFeedbacks();
            currentWeapon.AttackStart();
        }

        public virtual void AttackStop() {
            if (!AbilityAuthorized || currentWeapon == null) return;

            if (currentWeapon.weaponState.CurrentState == YisoWeapon.WeaponStates.Idle) return;
            if (currentWeapon.weaponState.CurrentState == YisoWeapon.WeaponStates.DelayBeforeUse &&
                !currentWeapon.canDelayBeforeUseInterrupted) return;
            if (currentWeapon.weaponState.CurrentState == YisoWeapon.WeaponStates.DelayBetweenUses &&
                !currentWeapon.canDelayBetweenUsesInterrupted) return;
            if (currentWeapon.weaponState.CurrentState == YisoWeapon.WeaponStates.Use) return;

            ForceStop();
        }

        public virtual void ForceStop() {
            StopStartFeedbacks();
            PlayAbilityStopFeedbacks();
            if (currentWeapon != null) {
                currentWeapon.AttackStop();
            }
            if (inputManager != null) inputManager.ResetMovement();
        }

        protected virtual void ResetAttackSpeed(float speed = 1f) {
            if (character.characterType == YisoCharacter.CharacterTypes.Player) {
                AttackSpeed = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().StatModule.AttackSpeed;
                return;
            }
            AttackSpeed = speed;
        }

        protected virtual bool CanAttack() {
            if (!AbilityAuthorized || AttackForbidden || currentWeapon == null) return false;
            if (conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Normal) return false;
            if (movementState.CurrentState == YisoCharacterStates.MovementStates.SkillCasting) return false;
            return true;
        }

        #endregion

        #region Animator

        /// <summary>
        /// PreprocessAbility -> ProcessAbility -> PostprocessAbility -> UpdateAnimator
        /// </summary>
        public override void UpdateAnimator() {
            base.UpdateAnimator();
            if (currentWeapon == null) {
                if (lastWeapon == null) {
                    YisoAnimatorUtils.UpdateAnimatorInteger(CharacterAnimator, attackTypeAnimationParameter, (int) YisoWeapon.AttackType.None, character.AnimatorParameters);
                }
                else {
                    YisoAnimatorUtils.UpdateAnimatorInteger(CharacterAnimator, attackTypeAnimationParameter, (int) lastWeapon.GetAttackType() % 3, character.AnimatorParameters);
                }

                YisoAnimatorUtils.UpdateAnimatorBool(CharacterAnimator, attackAnimationParameter, false, character.AnimatorParameters);
                YisoAnimatorUtils.UpdateAnimatorBool(CharacterAnimator, attackMoveAnimationParameter, false, character.AnimatorParameters);
                YisoAnimatorUtils.UpdateAnimatorFloat(CharacterAnimator, attackSpeedAnimationParameter, 1f, character.AnimatorParameters);
            }
            else {
                YisoAnimatorUtils.UpdateAnimatorBool(CharacterAnimator, attackAnimationParameter, currentWeapon.weaponState.CurrentState == YisoWeapon.WeaponStates.Use, character.AnimatorParameters);
                if (character.characterType == YisoCharacter.CharacterTypes.Player) {
                    YisoAnimatorUtils.UpdateAnimatorBool(CharacterAnimator, attackMoveAnimationParameter, currentWeapon.weaponState.CurrentState == YisoWeapon.WeaponStates.Use && controller.currentMovement.magnitude > 0f, character.AnimatorParameters);
                }

                YisoAnimatorUtils.UpdateAnimatorInteger(CharacterAnimator, attackTypeAnimationParameter, (int) currentWeapon.GetAttackType() % 3, character.AnimatorParameters);
                YisoAnimatorUtils.UpdateAnimatorFloat(CharacterAnimator, attackSpeedAnimationParameter, AttackSpeed, character.AnimatorParameters);
            }
        }

        #endregion

        #region Health

        protected override void OnHit(GameObject attacker) {
            base.OnHit(attacker);
            if (currentWeapon != null) currentWeapon.Interrupt();
        }

        protected override void OnDeath() {
            base.OnDeath();
            AttackStop();
            if (currentWeapon != null) {
                weaponBeforeDeath = currentWeapon;
                ChangeWeapon(null);
            }
        }

        protected override void OnRespawn() {
            base.OnRespawn();
            Setup();
        }

        #endregion

        #region Weapon Visible

        protected virtual void UpdateWeaponVisibility() {
            if (conditionState.CurrentState is YisoCharacterStates.CharacterConditions.Dead or YisoCharacterStates.CharacterConditions.Frozen) {
                HideCurrentWeapon();
                return;
            }

            if (movementState.CurrentState is YisoCharacterStates.MovementStates.Running or YisoCharacterStates.MovementStates.Dodging) {
                HideCurrentWeapon();
                return;
            }
            
            ShowCurrentWeapon();
        }

        protected virtual void ShowCurrentWeapon() {
            if (currentWeapon == null) return;
            currentWeapon.ShowWeapon();
        }

        protected virtual void HideCurrentWeapon() {
            if (currentWeapon == null) return;
            currentWeapon.HideWeapon();
        }

        #endregion
    }
}