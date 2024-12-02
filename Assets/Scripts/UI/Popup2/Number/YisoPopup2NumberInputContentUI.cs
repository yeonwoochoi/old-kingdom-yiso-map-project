using Core.Domain.Types;
using DG.Tweening;
using TMPro;
using UI.Popup2.Base;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace UI.Popup2.Number {
    public class YisoPopup2NumberInputContentUI : YisoPopup2BaseContentUI {
        [SerializeField] private TextMeshProUGUI displayText;

        private const int ALLOW_MAX_DIGITS = 9;

        private int maxDigits = 3;
        private int maxValue = 0;
        private string currentInput = "0";
        private YisoPopup2NumberInputArgs cachedArgs;

        protected override void HandleData(object data = null) {
            cachedArgs = (YisoPopup2NumberInputArgs)data!;
            
            switch (cachedArgs.MaxDigits) {
                case < 1:
                case > ALLOW_MAX_DIGITS:
                    cachedArgs.MaxDigits= ALLOW_MAX_DIGITS;
                    break;
            }
            maxValue = cachedArgs.MaxValue;
            maxDigits = cachedArgs.MaxDigits;
            displayText.SetText(cachedArgs.StartValue.ToCommaString());
            currentInput = cachedArgs.StartValue.ToString();
        }

        protected override void ClearPanel() {
            currentInput = "0";
            maxDigits = ALLOW_MAX_DIGITS;
            cachedArgs = null;
            displayText.SetText(currentInput);
        }

        public void OnClickNumber(string number) {
            if (currentInput.Length <= maxDigits) {
                string newInput;
                if (currentInput == "0") newInput = number;
                else newInput = currentInput + number;

                if (maxValue != -1) {
                    if (int.Parse(newInput) <= maxValue) {
                        currentInput = newInput;
                    }
                    else {
                        currentInput = maxValue.ToString();
                    }
                }
                else currentInput = newInput;
            } else ShakeDisplay();
            

            UpdateDisplayText();
        }

        public void OnClickRemove() {
            if (currentInput.Length > 1)
                currentInput = currentInput[..^1];
            else
                currentInput = "0";

            UpdateDisplayText();
        }

        public void OnClickOk() {
            var inputNumber = int.Parse(currentInput);
            foreach (var cb in cachedArgs.OkCbList) cb(inputNumber);
        }

        private void UpdateDisplayText() {
            var display = int.Parse(currentInput).ToCommaString();
            displayText.SetText(display);
        }

        private void ShakeDisplay() {
            displayText.rectTransform.DOShakeAnchorPos(0.5f, new Vector2(5, 0), 10, 90, false, true);
        }

        public override YisoPopup2Types GetPopupTypes() => YisoPopup2Types.NUMBER_INPUT;
    }
}