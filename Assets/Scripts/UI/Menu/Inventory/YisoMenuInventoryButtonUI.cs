using System;
using Core.Behaviour;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Inventory {
    public class YisoMenuInventoryButtonUI : RunIBehaviour {
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button button;
        
        public Button InventoryButton => button;

        public void SetText(string text) {
            buttonText.SetText(text);
        }

        public void Active(bool flag) {
            canvasGroup.Visible(flag);
        }

        public enum Types {
            DROP, QUICK, 
            USE, EQUIP, UN_EQUIP
        }
    }
}