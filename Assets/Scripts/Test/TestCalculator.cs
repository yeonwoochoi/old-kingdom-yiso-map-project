using System;
using Core.Domain.Actor.Player.Modules.Quest;
using TMPro;
using UnityEngine;
using Utils.Extensions;

namespace Test {
    public class TestCalculator : MonoBehaviour {
        public TextMeshProUGUI resultText;
        [Range(1, 10)]
        public int maxDigits = 3;
        private string currentInput = "0";

        private void Start() {
            UpdateDisplay();
        }

        public void OnClickButton(string value) {
            if (currentInput.Length < maxDigits) {
                if (currentInput == "0") {
                    if (value != "0") {
                        currentInput = value;
                    }
                } else {
                    currentInput += value;
                }
            }
            
            UpdateDisplay();
        }

        public void OnClickRemoveButton() {
            if (currentInput.Length > 1) {
                currentInput = currentInput[..^1];
            } else {
                currentInput = "0";
            }
            
            UpdateDisplay();
        }

        public void OnClickOkButton() {
            Debug.Log($"Input Accepted: {double.Parse(currentInput)}");
        }

        private void UpdateDisplay() {
            resultText.SetText(double.Parse(currentInput).ToCommaString());
        }
    }
}