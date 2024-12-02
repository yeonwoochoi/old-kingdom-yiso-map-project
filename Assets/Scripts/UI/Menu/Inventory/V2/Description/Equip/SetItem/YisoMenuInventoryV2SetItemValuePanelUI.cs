using Core.Behaviour;
using TMPro;
using UnityEngine;

namespace UI.Menu.Inventory.V2.Description.Equip.SetItem {
    public class YisoMenuInventoryV2SetItemValuePanelUI : RunIBehaviour {
        [SerializeField] private TextMeshProUGUI valueText;
        
        public bool Active { get; set; }

        public void SetText(string value, Color color) {
            valueText.SetText(value);
            valueText.color = color;
        }

        public void SetText(string value) { }
    }
}