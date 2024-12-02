using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Item.Equip;
using Core.Domain.Locale;
using TMPro;
using UI.Base;
using UnityEngine;
using Utils.Extensions;

namespace UI.Menu.Inventory.V2.Description.Equip {
    public class YisoMenuInventoryV2EquipAdditionalStatsDescriptionUI : YisoUIController {
        [SerializeField] private TextMeshProUGUI option1ValueText;
        [SerializeField] private TextMeshProUGUI option2ValueText;
        [SerializeField] private TextMeshProUGUI option3ValueText;

        private CanvasGroup canvasGroup;

        protected override void Awake() {
            base.Awake();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Visible(bool flag) {
            canvasGroup.Visible(flag);
        }

        public void SetPotentials(YisoEquipItem item) {
            var potentials = item.Potentials;
            if (potentials.TryGetValue(1, out var potential1)) {
                SetPotential(option1ValueText, potential1);
            } else SetLockText(option1ValueText);

            if (potentials.TryGetValue(2, out var potential2)) {
                SetPotential(option2ValueText, potential2);
            } else SetLockText(option2ValueText);

            if (potentials.TryGetValue(3, out var potential3)) {
                SetPotential(option3ValueText, potential3);
            } else SetLockText(option3ValueText);
        }

        public void Clear() {
            SetLockText(option1ValueText);
            SetLockText(option2ValueText);
            SetLockText(option3ValueText);
        }

        private void SetPotential(TextMeshProUGUI valueText, YisoEquipPotential potential) {
            valueText.SetText(potential.ToUIText(CurrentLocale));
        }
        
        private void SetLockText(TextMeshProUGUI valueText) {
            var text = CurrentLocale == YisoLocale.Locale.KR ? "잠김" : "Lock";
            valueText.SetText(text);
        }
        
    }
}