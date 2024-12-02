using System;
using Character.Weapon;
using Core.Behaviour;
using Core.Domain.Actor.Player;
using Core.Domain.Actor.Player.Modules.Inventory;
using Core.Domain.Item;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Character;
using Core.Service.Scene;
using Core.Service.Stage;
using Core.Service.UI;
using Core.Service.UI.Event;
using Core.Service.UI.HUD;
using Core.Service.UI.Popup;
using Sirenix.OdinInspector;
using UI.HUD.Interact;
using UI.HUD.RegularButton;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.HUD {
    public class YisoPlayerInputsUI : RunIBehaviour {
        [SerializeField] private YisoPlayerInteractButtonsUI interactButtonsUI;
        [SerializeField, Title("Regular Button")] private Button regularButton;
        [SerializeField] private Image weaponImage;
        [SerializeField] private Image reviveImage;
        [SerializeField] private YisoPlayerHUDArrowButtonUI arrowButtonUI;
        [SerializeField, Title("Buttons")] private Button menuButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button dashButton;
        [SerializeField] private Button weaponSwitchButton;
        [SerializeField, Title("Sprites")] private Sprite swordSprite;
        [SerializeField] private Sprite spearSprite;
        [SerializeField] private Sprite bowSprite;
        [SerializeField] private Sprite grabSprite;
        [SerializeField] private Sprite reviveSprite;
        
        private bool isAttackMode = true;

        private IYisoHUDUIService uiService = null;
        private YisoPlayer player = null;
        
        private bool holdAttackButton = false;
        private bool holdDashButton = false;
        private bool holdReviveButton = false;
        
        private float reviveDuration = 3f;
        private float reviveElapsedTime = 0f;
        private UnityAction<float, bool, bool> onRevive = null;
        private bool revived = false;
        private float reviveProgress = 0f;

        private CanvasGroup reviveImageCanvas;

        protected override void Awake() {
            base.Awake();
            uiService = YisoServiceProvider.Instance.Get<IYisoHUDUIService>();
            uiService.SetInputsUI(this);
            reviveImageCanvas = reviveImage.GetComponent<CanvasGroup>();
        }

        protected override void Start() {
            regularButton.OnPointerDownAsObservable().Subscribe(OnAttackDown).AddTo(this);
            regularButton.OnPointerUpAsObservable().Subscribe(OnAttackUp).AddTo(this);

            dashButton.OnPointerDownAsObservable().Subscribe(OnDashDown).AddTo(this);
            dashButton.OnPointerUpAsObservable().Subscribe(OnDashUp).AddTo(this);
            
            weaponSwitchButton.onClick.AddListener(OnClickSwitchWeapon);
            
            menuButton.onClick.AddListener(() => {
                YisoServiceProvider.Instance.Get<IYisoUIService>().ShowMenuUI(YisoMenuTypes.INVENTORY);
            });
            
            settingButton.onClick.AddListener(() => {
                YisoServiceProvider.Instance.Get<IYisoUIService>().ShowMenuUI(YisoMenuTypes.SETTINGS);
            });
            
            exitButton.onClick.AddListener(() => {
                var popupUIService = YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
                var stageService = YisoServiceProvider.Instance.Get<IYisoStageService>();
                var sceneService = YisoServiceProvider.Instance.Get<IYisoSceneService>();
                popupUIService.AlertS("베이스캠프 이동", "데이터가 저장되지 않습니다\n베이스캠프로 이동하시겠습니까?", () => {
                    if (stageService.GetCurrentStageId() == 1) {
                        sceneService.LoadScene(YisoSceneTypes.INIT);
                    } else sceneService.LoadScene(YisoSceneTypes.BASE_CAMP);
                    player.QuestModule.DrawStage(stageService.GetCurrentStageId());
                }, () => { });
            });

            player ??= YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer();
            var equipItem = player.InventoryModule.GetCurrentEquippedWeaponItem();
            SetRegularButtonSprite(equipItem);
            
            OnStageChanged();
        }

        protected override void OnEnable() {
            base.OnEnable();
            player = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer();
            player.InventoryModule.OnInventoryEvent += OnInventoryEvent;
            YisoServiceProvider.Instance.Get<IYisoStageService>().RegisterOnStageChanged(OnStageChanged);
        }

        protected override void OnDisable() {
            base.OnDisable();
            player.InventoryModule.OnInventoryEvent -= OnInventoryEvent;
            YisoServiceProvider.Instance.Get<IYisoStageService>().UnregisterOnStageChanged(OnStageChanged);
        }
        
        private void OnInventoryEvent(YisoPlayerInventoryEventArgs args) {
            var equipItem = player.InventoryModule.GetCurrentEquippedWeaponItem();
            switch (args) {
                case YisoPlayerInventoryEquipEventArgs:
                case YisoPlayerInventoryUnEquipEventArgs:
                case YisoPlayerInventorySwitchWeaponEventArgs:
                    SetRegularButtonSprite(equipItem);
                    break;
                case YisoPlayerInventoryCountEventArgs countArgs:
                    if (equipItem != null && equipItem.AttackType != YisoWeapon.AttackType.Shoot) return;
                    arrowButtonUI.SetArrowCount(countArgs.AfterCount);
                    break;
            }
        }

        public void SwitchToRevive(UnityAction<float, bool, bool> onRevive) {
            isAttackMode = false;
            weaponImage.sprite = reviveSprite;
            ResetRevive();
            this.onRevive = onRevive;
            reviveImage.fillAmount = 0f;
            reviveImageCanvas.Visible(true);

            var equipItem = player.InventoryModule.GetCurrentEquippedWeaponItem();
            if (equipItem is not { AttackType: YisoWeapon.AttackType.Shoot }) return;
            arrowButtonUI.VisibleButton(false);
        }

        public void SwitchToAttack() {
            isAttackMode = true;
            holdReviveButton = false;
            var equipItem = player.InventoryModule.GetCurrentEquippedWeaponItem();
            SetRegularButtonSprite(equipItem);
            ResetRevive();
        }

        private void ResetRevive() {
            revived = false;
            reviveElapsedTime = 0f;
            onRevive = null;
            reviveImage.fillAmount = 0f;
            reviveImageCanvas.Visible(false);
            reviveProgress = 0f;
        }

        private void OnClickSwitchWeapon() {
            player.InventoryModule.SwitchWeapon();
        }

        private void OnStageChanged() {
            var currentStageId = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();
            exitButton.gameObject.SetActive(currentStageId > 1);
        }

        private void SetRegularButtonSprite(YisoEquipItem equipItem) {
            if (equipItem == null) {
                weaponImage.sprite = grabSprite;
                arrowButtonUI.VisibleButton(false);
                return;
            }

            var weaponType = equipItem.AttackType;

            if (weaponType == YisoWeapon.AttackType.None) {
                weaponType = equipItem.GetName().Contains("창") ? YisoWeapon.AttackType.Thrust : YisoWeapon.AttackType.Slash;
            }

            weaponImage.sprite = weaponType switch {
                YisoWeapon.AttackType.Shoot => bowSprite,
                YisoWeapon.AttackType.Thrust => spearSprite,
                YisoWeapon.AttackType.Slash => swordSprite,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            arrowButtonUI.VisibleButton(weaponType == YisoWeapon.AttackType.Shoot);
            
            if (weaponType != YisoWeapon.AttackType.Shoot) return;
            var arrowCount = player.InventoryModule.GetArrowCount();
            arrowButtonUI.SetArrowCount(arrowCount);
        }
        
        private void OnAttackUp(PointerEventData data) {
            if (isAttackMode) {
                uiService.RaiseAttackInput(YisoSelectionInputStates.UP);
                if (holdAttackButton) {
                    holdAttackButton = false;
                }
                return;
            }
            
            uiService.RaiseReviveInput(YisoSelectionInputStates.UP);
            if (holdReviveButton) {
                holdReviveButton = false;
            }

            reviveElapsedTime = 0f;
            reviveImage.fillAmount = 0f;
            
            if (!revived) onRevive?.Invoke(reviveProgress * 100f, false, true);
        }

        private void OnDashUp(PointerEventData data) {
            uiService.RaiseDash(YisoSelectionInputStates.UP);
            if (holdDashButton) holdDashButton = false;
        }

        private void OnAttackDown(PointerEventData data) {
            if (isAttackMode) {
                uiService.RaiseAttackInput(YisoSelectionInputStates.DOWN);
                if (!holdAttackButton) holdAttackButton = true;
                return;
            }
            
            uiService.RaiseReviveInput(YisoSelectionInputStates.DOWN);
            if (!holdReviveButton) holdReviveButton = true;
        }

        private void OnDashDown(PointerEventData data) {
            uiService.RaiseDash(YisoSelectionInputStates.DOWN);
            if (!holdDashButton) holdDashButton = true;
        }

        public override void OnUpdate() {
            if (holdAttackButton) {
                if (isAttackMode)
                    uiService.RaiseAttackInput(YisoSelectionInputStates.HOLD);
            }
            
            if (holdDashButton)
                uiService.RaiseDash(YisoSelectionInputStates.HOLD);

            if (isAttackMode) return;
            
            if (holdReviveButton) {
                if (revived) return;
                if (reviveElapsedTime < reviveDuration) {
                    reviveElapsedTime += Time.deltaTime;
                    reviveProgress = Mathf.Clamp01(reviveElapsedTime / reviveDuration);
                    reviveImage.fillAmount = reviveProgress;
                    onRevive?.Invoke(reviveProgress * 100f, false, false);
                    if (reviveElapsedTime >= reviveDuration && !revived) {
                        reviveImage.fillAmount = 1f;
                        reviveProgress = 1f;
                        onRevive?.Invoke(reviveProgress * 100f, true, false);
                        revived = true;
                    } 
                }
            }
        }

        public void ShowInteractButton(YisoHudUIInteractTypes type, UnityAction onClick) {
            interactButtonsUI.Show(type, onClick);
        }

        public void HideInteractButton(YisoHudUIInteractTypes type) {
            interactButtonsUI.Hide(type);
        }
    }
}