using System.Collections.Generic;
using Core.Domain.Types;
using Core.Service;
using Core.Service.UI;
using Core.Service.UI.HUD;
using Tools.Inputs;
using Tools.Singleton;
using UnityEngine;
using Utils.Beagle;

namespace Manager {
    /// <summary>
    /// 등록된 버튼의 State를 관리해줌 -> 그에 맞는 Action은 Button State를 Get해서 각 script에서 짜야함.
    /// </summary>
    public class InputManager : YisoTempSingleton<InputManager> {
        [Header("Settings")]
        public bool inputDetectionActive = true; // Input Manager가 입력을 읽지 못하도록 하려면 이 값을 false로 설정하면 됨

        public bool
            resetButtonStatesOnFocusLoss = true; // Focus Loss 즉, 디바이스에서 겜 직접 종료하지 않고 이탈 복귀하는 경우 버튼 상태를 모두 리셋시킬건지 말지

        [Header("Player")] public string playerID = "Yiso";

        [Header("Mode")] public bool autoMobileDetection = true;
        public InputForcedModes inputForcedModes = InputForcedModes.Desktop;
        public MovementControls movementControl = MovementControls.Mouse;

        [Header("Movement")] public bool smoothMovement = true; // 가속, 감속 적용할건지 말건지
        public float threshold = 0.1f; // movement를 Trigger 시키기 위한 최소한의 경계값 (Analog input ..joystick 같은 경우 필요)
        public float slowingRadius = 2.0f; // 마우스로 이동할때 도착 지점에 가까워지면 속도 감속 (캐릭터가 초과이동 -> 재조정을 반복하면서 진동하는 것을 방지)
        public float decelerationPower = 5.0f; // 얼마나 감속시킬지

        public enum InputForcedModes {
            Mobile,
            Desktop
        }

        public enum MovementControls {
            Joystick,
            Mouse,
            Keyboard
        }


        public bool IsMobile { get; protected set; }
        public Vector2 Movement => movement; // 최종 Input 값. CharacterMovement로 넘어갈 값
        public Vector2 SecondaryMovement => secondaryMovement; // 최종 Secondary Input 값. CharacterMovement로 넘어갈 값
        public Vector2 LastMovement { get; set; }
        public Vector2 LastSecondaryMovement { get; set; }

        public IYisoHUDUIService HUDService => YisoServiceProvider.Instance.Get<IYisoHUDUIService>();
        public IYisoUIService UIService => YisoServiceProvider.Instance.Get<IYisoUIService>();
        public YisoInput.IMButton LeftMouseClick { get; protected set; }
        public YisoInput.IMButton RightMouseClick { get; protected set; }
        public YisoInput.IMButton AttackButton { get; protected set; }
        public YisoInput.IMButton DashButton { get; protected set; }
        public YisoInput.IMButton DodgeButton { get; protected set; }
        public YisoInput.IMButton InteractButton { get; protected set; }
        public YisoInput.IMButton PickButton { get; protected set; }
        public YisoInput.IMButton RunButton { get; protected set; }
        public YisoInput.IMButton SwitchWeaponButton { get; protected set; }
        public YisoInput.IMButton QuickSlot1Button { get; protected set; }
        public YisoInput.IMButton QuickSlot2Button { get; protected set; }
        public YisoInput.IMButton QuickSlot3Button { get; protected set; }
        public YisoInput.IMButton QuickSlot4Button { get; protected set; }
        public YisoInput.IMButton Skill1Button { get; protected set; }
        public YisoInput.IMButton Skill2Button { get; protected set; }
        public YisoInput.IMButton Skill3Button { get; protected set; }
        public YisoInput.IMButton Skill4Button { get; protected set; }
        public YisoInput.IMButton Skill5Button { get; protected set; }
        public YisoInput.IMButton Skill6Button { get; protected set; }

        protected Vector2 movement = Vector2.zero;
        protected Vector2 secondaryMovement = Vector2.zero;
        protected Vector2 lastMouseRightClickPosition = Vector2.zero;
        protected Vector2 lastMouseLeftClickPosition = Vector2.zero;
        protected List<YisoInput.IMButton> buttonList;
        protected string axisHorizontal;
        protected string axisVertical;
        protected string secondaryAxisHorizontal;
        protected string secondaryAxisVertical;
        protected bool isMobileMovementEnabled = false;
        protected bool isMobileSecondaryMovementEnabled = false;

        public virtual Vector2 MousePosition => Input.mousePosition;

        public virtual Vector2 PlayerPosition {
            get {
                if (!GameManager.HasInstance || GameManager.Instance.Player == null) return Vector2.zero;
                return GameManager.Instance.Player.transform.position;
            }
        }

        #region Initialization

        /// <summary>
        /// 게임이 실행될 때 초기화 작업 위해 추가한 것
        /// Awake 이전에 실행됨
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        protected static void InitializeStatics() {
            instance = null;
        }

        protected override void Awake() {
            base.Awake();
            PreInitialization();
        }

        protected virtual void Start() {
            Initialization();
        }

        protected virtual void PreInitialization() {
            InitializeButtons();
            InitializeAxis();
        }

        protected virtual void Initialization() {
            ControlModeDetection();
            RegisterMobileControlEvents();
        }

        protected virtual void InitializeButtons() {
            buttonList = new List<YisoInput.IMButton>();
            LeftMouseClick = new YisoInput.IMButton(playerID, "LeftMouse", LeftMouseDown, LeftMousePressed, LeftMouseUp);
            RightMouseClick = new YisoInput.IMButton(playerID, "RightMouse", RightMouseDown, RightMousePressed, RightMouseUp);
            buttonList.Add(AttackButton = new YisoInput.IMButton(playerID, "Attack", AttackButtonDown, AttackButtonPressed, AttackButtonUp));
            buttonList.Add(DashButton = new YisoInput.IMButton(playerID, "Dash", DashButtonDown, DashButtonPressed, DashButtonUp));
            buttonList.Add(DodgeButton = new YisoInput.IMButton(playerID, "Dodge", DodgeButtonDown, DodgeButtonPressed, DodgeButtonUp));
            buttonList.Add(InteractButton = new YisoInput.IMButton(playerID, "Interact", InteractButtonDown, InteractButtonPressed, InteractButtonUp));
            buttonList.Add(PickButton = new YisoInput.IMButton(playerID, "Pick", PickButtonDown, PickButtonPressed, PickButtonUp));
            buttonList.Add(RunButton = new YisoInput.IMButton(playerID, "Run", RunButtonDown, RunButtonPressed, RunButtonUp));
            buttonList.Add(SwitchWeaponButton = new YisoInput.IMButton(playerID, "Switch", SwitchWeaponButtonDown, SwitchWeaponButtonPressed, SwitchWeaponButtonUp));
            buttonList.Add(QuickSlot1Button = new YisoInput.IMButton(playerID, "Quick_Slot_1", QuickSlot1ButtonDown, QuickSlot1ButtonPressed, QuickSlot1ButtonUp));
            buttonList.Add(QuickSlot2Button = new YisoInput.IMButton(playerID, "Quick_Slot_2", QuickSlot2ButtonDown, QuickSlot2ButtonPressed, QuickSlot2ButtonUp));
            buttonList.Add(QuickSlot3Button = new YisoInput.IMButton(playerID, "Quick_Slot_3", QuickSlot3ButtonDown, QuickSlot3ButtonPressed, QuickSlot3ButtonUp));
            buttonList.Add(QuickSlot4Button = new YisoInput.IMButton(playerID, "Quick_Slot_4", QuickSlot4ButtonDown, QuickSlot4ButtonPressed, QuickSlot4ButtonUp));
            buttonList.Add(Skill1Button = new YisoInput.IMButton(playerID, "Skill_1", Skill1ButtonDown, Skill1ButtonPressed, Skill1ButtonUp));
            buttonList.Add(Skill2Button = new YisoInput.IMButton(playerID, "Skill_2", Skill2ButtonDown, Skill2ButtonPressed, Skill2ButtonUp));
            buttonList.Add(Skill3Button = new YisoInput.IMButton(playerID, "Skill_3", Skill3ButtonDown, Skill3ButtonPressed, Skill3ButtonUp));
            buttonList.Add(Skill4Button = new YisoInput.IMButton(playerID, "Skill_4", Skill4ButtonDown, Skill4ButtonPressed, Skill4ButtonUp));
            buttonList.Add(Skill5Button = new YisoInput.IMButton(playerID, "Skill_5", Skill5ButtonDown, Skill5ButtonPressed, Skill5ButtonUp));
            buttonList.Add(Skill6Button = new YisoInput.IMButton(playerID, "Skill_6", Skill6ButtonDown, Skill6ButtonPressed, Skill6ButtonUp));
        }

        protected virtual void InitializeAxis() {
            axisHorizontal = playerID + "_Horizontal";
            axisVertical = playerID + "_Vertical";
            secondaryAxisHorizontal = playerID + "_SecondaryHorizontal";
            secondaryAxisVertical = playerID + "_SecondaryVertical";
        }

        /// <summary>
        /// Platform 체크한 후 Control키 보여줄지 말지 설정
        /// </summary>
        protected virtual void ControlModeDetection() {
            IsMobile = false;

            if (autoMobileDetection) {
                if (Application.platform == RuntimePlatform.Android ||
                    Application.platform == RuntimePlatform.IPhonePlayer) {
                    IsMobile = true;
                    return;
                }
            }

            IsMobile = inputForcedModes switch {
                InputForcedModes.Mobile => true,
                InputForcedModes.Desktop => false,
                _ => IsMobile
            };
        }

        protected virtual void OnDisable() {
            UnregisterMobileControlEvents();
        }

        protected virtual void RegisterMobileControlEvents() {
            if (!IsMobile) return;
            if (!HUDService.IsReady()) return;
            HUDService.RegisterOnJoystickMovement(SetMobileMovement);
            HUDService.RegisterOnJoystickInput(SetMobileMovementState);
            // TODO Secondary Joystick 도 등록
            HUDService.RegisterAttackInput(SetMobileAttack);
            HUDService.RegisterDashInput(SetMobileDash);
            // TODO : Mobile HUD Joystick event에 Dodge 등록
            // HUDService.RegisterDodgeInput(SetMobileDodge);
        }

        protected virtual void UnregisterMobileControlEvents() {
            if (!IsMobile) return;
            if (!HUDService.IsReady()) return;
            HUDService.UnregisterOnJoystickMovement(SetMobileMovement);
            HUDService.UnregisterOnJoystickInput(SetMobileMovementState);
            HUDService.UnregisterAttackInput(SetMobileAttack);
            HUDService.UnregisterDashInput(SetMobileDash);
            // TODO : Mobile HUD Joystick event에 Dodge 등록
            // HUDService.UnregisterDodgeInput(SetMobileDodge);
        }

        #endregion

        public virtual void Update() {
            ProcessButtonStates();
            ProcessMouseState();

            if (!IsMobile && inputDetectionActive) {
                GetDesktopMouseClick();
                GetDesktopInputButtons();
                SetDesktopMovement();
                SetDesktopLastMovement();
            }
        }

        #region Update (Button State)

        /// <summary>
        /// Update 에서 호출
        /// Button State 체크해서 이전 프레임에 Button Down 상태였으면 Pressed로
        /// Down -> Pressed
        /// Up -> Off
        /// </summary>
        protected virtual void ProcessButtonStates() {
            foreach (var button in buttonList) {
                if (button.State.CurrentState == YisoInput.ButtonStates.ButtonDown) {
                    button.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
                }

                if (button.State.CurrentState == YisoInput.ButtonStates.ButtonUp) {
                    button.State.ChangeState(YisoInput.ButtonStates.Off);
                }
            }
        }

        protected virtual void ProcessMouseState() {
            if (LeftMouseClick.State.CurrentState == YisoInput.ButtonStates.ButtonDown) {
                LeftMouseClick.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
            }

            if (LeftMouseClick.State.CurrentState == YisoInput.ButtonStates.ButtonUp) {
                LeftMouseClick.State.ChangeState(YisoInput.ButtonStates.Off);
            }

            if (RightMouseClick.State.CurrentState == YisoInput.ButtonStates.ButtonDown) {
                RightMouseClick.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
            }

            if (RightMouseClick.State.CurrentState == YisoInput.ButtonStates.ButtonUp) {
                RightMouseClick.State.ChangeState(YisoInput.ButtonStates.Off);
            }
        }

        #endregion

        #region Update(Mobile)

        protected virtual void SetMobileMovement(Vector2 inputMovement) {
            if (IsMobile && inputDetectionActive) {
                if (isMobileMovementEnabled) {
                    movement.x = inputMovement.x;
                    movement.y = inputMovement.y;
                }
                else {
                    movement = Vector2.zero;
                }
            }
        }

        protected virtual void SetMobileMovementState(bool canMove) {
            if (!IsMobile || !inputDetectionActive) {
                isMobileMovementEnabled = false;
                return;
            }

            isMobileMovementEnabled = canMove;
        }

        public virtual void SetMobileSecondaryMovement(Vector2 inputMovement) {
            if (IsMobile && inputDetectionActive) {
                if (isMobileSecondaryMovementEnabled) {
                    secondaryMovement.x = inputMovement.x;
                    secondaryMovement.y = inputMovement.y;
                }
                else {
                    secondaryMovement = Vector2.zero;
                }
            }
        }

        protected virtual void SetMobileSecondaryMovementState(bool canMove) {
            if (!IsMobile || !inputDetectionActive) {
                isMobileSecondaryMovementEnabled = false;
                return;
            }

            isMobileSecondaryMovementEnabled = canMove;
        }

        protected virtual void SetMobileAttack(YisoSelectionInputStates inputState) {
            if (!IsMobile || !inputDetectionActive) return;
            switch (inputState) {
                case YisoSelectionInputStates.DOWN:
                    AttackButton.TriggerButtonDown();
                    break;
                case YisoSelectionInputStates.UP:
                    AttackButton.TriggerButtonUp();
                    break;
                case YisoSelectionInputStates.HOLD:
                    AttackButton.TriggerButtonPressed();
                    break;
                default:
                    AttackButton.TriggerButtonUp();
                    break;
            }
        }

        protected virtual void SetMobileDash(YisoSelectionInputStates inputState) {
            if (!IsMobile || !inputDetectionActive) return;
            DashButton.TriggerButtonPressed();
            switch (inputState) {
                case YisoSelectionInputStates.DOWN:
                    DashButton.TriggerButtonDown();
                    break;
                case YisoSelectionInputStates.UP:
                    DashButton.TriggerButtonUp();
                    break;
                case YisoSelectionInputStates.HOLD:
                    DashButton.TriggerButtonPressed();
                    break;
                default:
                    DashButton.TriggerButtonUp();
                    break;
            }
        }

        protected virtual void SetMobileDodge(YisoSelectionInputStates inputState) {
            if (!IsMobile || !inputDetectionActive) return;
            DodgeButton.TriggerButtonPressed();
            switch (inputState) {
                case YisoSelectionInputStates.DOWN:
                    DodgeButton.TriggerButtonDown();
                    break;
                case YisoSelectionInputStates.UP:
                    DodgeButton.TriggerButtonUp();
                    break;
                case YisoSelectionInputStates.HOLD:
                    DodgeButton.TriggerButtonPressed();
                    break;
                default:
                    DodgeButton.TriggerButtonUp();
                    break;
            }
        }

        #endregion

        #region Update (Desktop)

        protected virtual void GetDesktopMouseClick() {
            if (IsMobile || !inputDetectionActive) return;
            if (UIService.IsReady() && UIService.IsUIShowed()) {
                LeftMouseClick.TriggerButtonUp();
                RightMouseClick.TriggerButtonUp();
                return;
            }

            if (Input.GetMouseButton(0) && !YisoInputUtils.IsPointerOverUI()) {
                MouseLeftClick();
                LeftMouseClick.TriggerButtonPressed();
            }

            if (Input.GetMouseButtonDown(0) && !YisoInputUtils.IsPointerOverUI()) {
                MouseLeftClick();
                LeftMouseClick.TriggerButtonDown();
            }

            if (Input.GetMouseButtonUp(0)) {
                LeftMouseClick.TriggerButtonUp();
            }

            if (Input.GetMouseButton(1) && !YisoInputUtils.IsPointerOverUI()) {
                MouseRightClick();
                RightMouseClick.TriggerButtonPressed();
            }

            if (Input.GetMouseButtonDown(1) && !YisoInputUtils.IsPointerOverUI()) {
                MouseRightClick();
                RightMouseClick.TriggerButtonDown();
            }

            if (Input.GetMouseButtonUp(1)) {
                MouseRightClick();
                RightMouseClick.TriggerButtonUp();
            }

            void MouseRightClick() {
                if (UnityEngine.Camera.main is null) return;
                lastMouseRightClickPosition = UnityEngine.Camera.main.ScreenToWorldPoint(MousePosition);
            }

            void MouseLeftClick() {
                if (UnityEngine.Camera.main is null) return;
                lastMouseLeftClickPosition = UnityEngine.Camera.main.ScreenToWorldPoint(MousePosition);
            }
        }

        /// <summary>
        /// Mobile이 아닌 경우 Input 값 보고 Button State 처리
        /// </summary>
        protected virtual void GetDesktopInputButtons() {
            foreach (var button in buttonList) {
                if (Input.GetButton(button.buttonID)) {
                    button.TriggerButtonPressed();
                }

                if (Input.GetButtonDown(button.buttonID)) {
                    button.TriggerButtonDown();
                }

                if (Input.GetButtonUp(button.buttonID)) {
                    button.TriggerButtonUp();
                }
            }
        }

        protected virtual void SetDesktopMovement() {
            if (IsMobile || !inputDetectionActive) return;
            switch (movementControl) {
                case MovementControls.Joystick:
                    // Mobile Control (HUDService에 Register해서 처리)
                    break;
                case MovementControls.Mouse:
                    // Set Movement
                    var distanceToTarget = Vector2.Distance(PlayerPosition, lastMouseRightClickPosition);
                    if (distanceToTarget > threshold) {
                        movement = lastMouseRightClickPosition - PlayerPosition;

                        if (distanceToTarget < slowingRadius) {
                            var slowingFactor = Mathf.Pow(distanceToTarget / slowingRadius, decelerationPower);
                            movement *= slowingFactor; // Reduce movement speed near the target
                        }
                    }
                    else {
                        ResetMovement();
                    }

                    // Set Secondary Movement
                    secondaryMovement = lastMouseLeftClickPosition - PlayerPosition;
                    break;
                case MovementControls.Keyboard:
                    if (smoothMovement) {
                        movement.x = Input.GetAxis(axisHorizontal);
                        movement.y = Input.GetAxis(axisVertical);
                        secondaryMovement.x = Input.GetAxis(secondaryAxisHorizontal);
                        secondaryMovement.y = Input.GetAxis(secondaryAxisVertical);
                    }
                    else {
                        movement.x = Input.GetAxisRaw(axisHorizontal);
                        movement.y = Input.GetAxisRaw(axisVertical);
                        secondaryMovement.x = Input.GetAxisRaw(secondaryAxisHorizontal);
                        secondaryMovement.y = Input.GetAxisRaw(secondaryAxisVertical);
                    }

                    break;
            }
        }

        protected virtual void SetDesktopLastMovement() {
            if (movement.magnitude > threshold) {
                LastMovement = movement;
            }

            if (secondaryMovement.magnitude > threshold) {
                LastSecondaryMovement = movement;
            }
        }

        public virtual void ResetAllMovement() {
            ResetMovement();
            ResetSecondaryMovement();
        }

        public virtual void ResetMovement() {
            lastMouseRightClickPosition = PlayerPosition;
            movement = Vector2.zero;
        }

        public virtual void ResetMovement(Vector2 destination) {
            lastMouseRightClickPosition = destination;
            movement = Vector2.zero;
        }

        public virtual void ResetSecondaryMovement() {
            lastMouseLeftClickPosition = PlayerPosition;
            secondaryMovement = Vector2.zero;
        }

        #endregion

        #region Focus

        /// <summary>
        /// 디바이스에서 게임을 직접적으로 종료하지 않고 이탈/복귀하는 경우 실행됨
        /// </summary>
        /// <param name="hasFocus">겜 도중 이탈 시 false, 복귀시 true</param>
        protected void OnApplicationFocus(bool hasFocus) {
            if (!hasFocus && resetButtonStatesOnFocusLoss && buttonList != null) {
                ForceAllButtonStatesTo(YisoInput.ButtonStates.ButtonUp);
            }
        }

        protected virtual void ForceAllButtonStatesTo(YisoInput.ButtonStates newState) {
            foreach (var button in buttonList) {
                button.State.ChangeState(newState);
            }

            LeftMouseClick.State.ChangeState(newState);
            RightMouseClick.State.ChangeState(newState);
        }

        #endregion

        #region Buttons

        protected virtual void LeftMouseDown() {
            LeftMouseClick.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void LeftMousePressed() {
            LeftMouseClick.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void LeftMouseUp() {
            LeftMouseClick.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void RightMouseDown() {
            RightMouseClick.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void RightMousePressed() {
            RightMouseClick.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void RightMouseUp() {
            RightMouseClick.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void AttackButtonDown() {
            AttackButton.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void AttackButtonPressed() {
            AttackButton.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void AttackButtonUp() {
            AttackButton.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void DashButtonDown() {
            DashButton.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void DashButtonPressed() {
            DashButton.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void DashButtonUp() {
            DashButton.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void DodgeButtonDown() {
            DodgeButton.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void DodgeButtonPressed() {
            DodgeButton.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void DodgeButtonUp() {
            DodgeButton.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void InteractButtonDown() {
            InteractButton.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void InteractButtonPressed() {
            InteractButton.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void InteractButtonUp() {
            InteractButton.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void PickButtonDown() {
            PickButton.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void PickButtonPressed() {
            PickButton.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void PickButtonUp() {
            PickButton.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void RunButtonDown() {
            RunButton.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void RunButtonPressed() {
            RunButton.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void RunButtonUp() {
            RunButton.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void SwitchWeaponButtonDown() {
            SwitchWeaponButton.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void SwitchWeaponButtonPressed() {
            SwitchWeaponButton.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void SwitchWeaponButtonUp() {
            SwitchWeaponButton.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void QuickSlot1ButtonDown() {
            QuickSlot1Button.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void QuickSlot1ButtonPressed() {
            QuickSlot1Button.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void QuickSlot1ButtonUp() {
            QuickSlot1Button.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void QuickSlot2ButtonDown() {
            QuickSlot2Button.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void QuickSlot2ButtonPressed() {
            QuickSlot2Button.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void QuickSlot2ButtonUp() {
            QuickSlot2Button.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void QuickSlot3ButtonDown() {
            QuickSlot3Button.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void QuickSlot3ButtonPressed() {
            QuickSlot3Button.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void QuickSlot3ButtonUp() {
            QuickSlot3Button.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void QuickSlot4ButtonDown() {
            QuickSlot4Button.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void QuickSlot4ButtonPressed() {
            QuickSlot4Button.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void QuickSlot4ButtonUp() {
            QuickSlot4Button.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void Skill1ButtonDown() {
            Skill1Button.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void Skill1ButtonPressed() {
            Skill1Button.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void Skill1ButtonUp() {
            Skill1Button.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void Skill2ButtonDown() {
            Skill2Button.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void Skill2ButtonPressed() {
            Skill2Button.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void Skill2ButtonUp() {
            Skill2Button.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void Skill3ButtonDown() {
            Skill3Button.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void Skill3ButtonPressed() {
            Skill3Button.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void Skill3ButtonUp() {
            Skill3Button.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void Skill4ButtonDown() {
            Skill4Button.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void Skill4ButtonPressed() {
            Skill4Button.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void Skill4ButtonUp() {
            Skill4Button.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void Skill5ButtonDown() {
            Skill5Button.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void Skill5ButtonPressed() {
            Skill5Button.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void Skill5ButtonUp() {
            Skill5Button.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        protected virtual void Skill6ButtonDown() {
            Skill6Button.State.ChangeState(YisoInput.ButtonStates.ButtonDown);
        }

        protected virtual void Skill6ButtonPressed() {
            Skill6Button.State.ChangeState(YisoInput.ButtonStates.ButtonPressed);
        }

        protected virtual void Skill6ButtonUp() {
            Skill6Button.State.ChangeState(YisoInput.ButtonStates.ButtonUp);
        }

        #endregion
    }
}