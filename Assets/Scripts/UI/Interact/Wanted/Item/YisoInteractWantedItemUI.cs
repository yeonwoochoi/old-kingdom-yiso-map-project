using Core.Behaviour;
using Core.Domain.Locale;
using Core.Domain.Wanted;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Wanted.Item {
    public class YisoInteractWantedItemUI : RunIBehaviour, IInstantiatable {
        [SerializeField, Title("Basics")] private Image targetImage;
        [SerializeField] private TextMeshProUGUI targetNameText;
        [SerializeField] private TextMeshProUGUI targetPriceText;
        [SerializeField, Title("Toggle")] private Toggle toggle;

        public Toggle ItemToggle => toggle;

        private bool active = false;

        public YisoWanted Wanted { get; private set; }
        
        public bool Active {
            get => active;
            private set {
                gameObject.SetActive(value);
                active = value;
            }
        }

        public void SetItem(YisoWanted wanted, YisoLocale.Locale locale) {
            Wanted = wanted;
            targetImage.sprite = wanted.TargetImage;
            targetImage.enabled = true;
            targetNameText.SetText(wanted.GetTargetName(locale));
            targetPriceText.SetText(GetBounty(locale));
            Active = true;
        }

        private string GetBounty(YisoLocale.Locale locale) {
            var currency = locale == YisoLocale.Locale.KR ? "량" : "NYANG";
            return $"{Wanted.Bounty.ToCommaString()} {currency}";
        }

        public void Init() {
            targetImage.sprite = null;
            targetImage.enabled = false;
            targetNameText.SetText("");
            targetPriceText.SetText("");
            Active = false;
        }
    }
}