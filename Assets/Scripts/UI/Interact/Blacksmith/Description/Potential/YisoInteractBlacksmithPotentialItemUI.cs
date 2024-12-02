using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Item.Equip;
using Core.Domain.Locale;
using TMPro;
using UI.Base;
using UnityEngine;

namespace UI.Interact.Blacksmith.Description.Potential {
    public class YisoInteractBlacksmithPotentialItemUI : RunIBehaviour {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI valueText;

        private CanvasGroup canvasGroup;

        protected override void Awake() {
            base.Awake();
            canvasGroup = GetOrAddComponent<CanvasGroup>();
        }

        public CanvasGroup ItemCanvas => canvasGroup;

        public void SetPotential(YisoEquipPotential potential = null, YisoLocale.Locale locale = YisoLocale.Locale.KR) {
            titleText.SetText(potential == null ? "-" : $"{potential.ToUITitle(locale)}: ");
            valueText.SetText(potential == null ? string.Empty : potential.ToUIValue());
        }
    }
}