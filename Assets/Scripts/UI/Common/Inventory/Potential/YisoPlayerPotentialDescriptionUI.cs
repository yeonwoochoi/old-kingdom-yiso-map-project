using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Item.Equip;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UnityEngine;

namespace UI.Common.Inventory.Potential {
    public class YisoPlayerPotentialDescriptionUI : YisoUIController {
        [SerializeField, Title("Objects")] private List<GameObject> descriptionObjects;
        [SerializeField, Title("Potentials")] 
        private TextMeshProUGUI potential1TitleText;
        [SerializeField] private TextMeshProUGUI potential1ValueText;
        [SerializeField] private TextMeshProUGUI potential2TitleText;
        [SerializeField] private TextMeshProUGUI potential2ValueText;
        [SerializeField] private TextMeshProUGUI potential3TitleText;
        [SerializeField] private TextMeshProUGUI potential3ValueText;

        public void ActiveDescription(bool flag) {
            foreach (var obj in descriptionObjects) {
                obj.SetActive(flag);
            }
        }

        public void SetPotentials(YisoEquipItem item) {
            if (item.Potentials.TryGetValue(1, out var potential1)) {
                SetPotential(potential1, potential1TitleText, potential1ValueText);
            } else SetNone(potential1TitleText, potential1ValueText);

            if (item.Potentials.TryGetValue(2, out var potential2)) {
                SetPotential(potential2, potential2TitleText, potential2ValueText);
            } else SetNone(potential2TitleText, potential2ValueText);

            if (item.Potentials.TryGetValue(3, out var potential3)) {
                SetPotential(potential3, potential3TitleText, potential3ValueText);
            } else SetNone(potential3TitleText, potential3ValueText);
        }

        private void SetPotential(YisoEquipPotential potential, TextMeshProUGUI titleText,
            TextMeshProUGUI valueText) {
            titleText.SetText(potential.ToUITitle(CurrentLocale));
            valueText.SetText(potential.ToUIValue());
        }

        private void SetNone(TextMeshProUGUI titleText, TextMeshProUGUI valueText) {
            titleText.SetText("-");
            valueText.SetText(string.Empty);
        }

        public void Clear() {
            potential1TitleText.SetText(string.Empty);
            potential2TitleText.SetText(string.Empty);
            potential3TitleText.SetText(string.Empty);
            
            potential1ValueText.SetText(string.Empty);
            potential2ValueText.SetText(string.Empty);
            potential3ValueText.SetText(string.Empty);
        }
    }
}