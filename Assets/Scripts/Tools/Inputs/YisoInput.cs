using Core.Behaviour;
using Tools.StateMachine;

namespace Tools.Inputs {
    public class YisoInput : RunIBehaviour {
        public enum ButtonStates {
            Off,
            ButtonDown,
            ButtonPressed,
            ButtonUp
        }

        // Input 값으로 Button State 결정
        public static ButtonStates ProcessAxisAsButton(string axisName, float threshold, ButtonStates currentState) {
            var axisValue = UnityEngine.Input.GetAxis(axisName);
            ButtonStates returnState;

            var isPressed = axisValue >= threshold;

            if (isPressed) {
                returnState = currentState == ButtonStates.Off ? ButtonStates.ButtonDown : ButtonStates.ButtonPressed;
            }
            else {
                returnState = currentState == ButtonStates.ButtonPressed ? ButtonStates.ButtonUp : ButtonStates.Off;
            }

            return returnState;
        }

        /// <summary>
        /// IM button, short for InputManager button, a class used to handle button states, whether mobile or actual keys
        /// </summary>
        public class IMButton {
            public YisoStateMachine<ButtonStates> State { get; protected set; }

            public string buttonID;

            public delegate void ButtonDownMethodDelegate();

            public delegate void ButtonPressedMethodDelegate();

            public delegate void ButtonUpMethodDelegate();

            public ButtonDownMethodDelegate ButtonDownMethod;
            public ButtonPressedMethodDelegate ButtonPressedMethod;
            public ButtonUpMethodDelegate ButtonUpMethod;

            public IMButton(string playerID, string btnID, ButtonDownMethodDelegate btnDown = null,
                ButtonPressedMethodDelegate btnPressed = null, ButtonUpMethodDelegate btnUp = null) {
                buttonID = playerID + "_" + btnID;
                ButtonDownMethod = btnDown;
                ButtonUpMethod = btnUp;
                ButtonPressedMethod = btnPressed;
                State = new YisoStateMachine<ButtonStates>(null, false);
                State.ChangeState(ButtonStates.Off);
            }

            public virtual void TriggerButtonDown() {
                if (ButtonDownMethod != null) ButtonDownMethod();
                else State.ChangeState(ButtonStates.ButtonDown);
            }

            public virtual void TriggerButtonUp() {
                if (ButtonUpMethod != null) ButtonUpMethod();
                else State.ChangeState(ButtonStates.ButtonUp);
            }

            public virtual void TriggerButtonPressed() {
                if (ButtonPressedMethod != null) ButtonPressedMethod();
                else State.ChangeState(ButtonStates.ButtonPressed);
            }
        }
    }
}