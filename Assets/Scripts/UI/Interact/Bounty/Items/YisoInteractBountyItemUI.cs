using Core.Behaviour;
using Core.Domain.Bounty;
using Core.Domain.Locale;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Bounty.Items {
    public class YisoInteractBountyItemUI : YisoUIController, IInstantiatable {
        [SerializeField, Title("Basics")] private Image targetImage;
        [SerializeField] private TextMeshProUGUI targetNameText;
        [SerializeField] private TextMeshProUGUI targetPriceText;
        [SerializeField, Title("Toggle")] private Toggle toggle;
        [SerializeField, Title("Complete")] private CanvasGroup completeCanvas;

        public Toggle ItemToggle => toggle;

        private bool active = false;
        
        public YisoBounty Bounty { get; private set; }
        public YisoBountyStatus Status { get; private set; }

        public bool Active {
            get => active;
            set {
                gameObject.SetActive(value);
                active = value;
            }
        }

        public void SetBounty(YisoBounty bounty, YisoBountyStatus status, YisoLocale.Locale locale) {
            Bounty = bounty;
            Status = status;
            targetImage.sprite = bounty.TargetIcon;
            targetImage.enabled = true;
            targetNameText.SetText(bounty.GetTargetName(locale));
            targetPriceText.SetText(GetBountyPrice(locale));
            completeCanvas.Visible(status == YisoBountyStatus.COMPLETE);
            Active = true;
        }

        private string GetBountyPrice(YisoLocale.Locale locale) {
            var currency = locale == YisoLocale.Locale.KR ? "량" : "NYANG";
            return $"{Bounty.BountyReward.ToCommaString()} {currency}";
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