using Core.Behaviour;
using Core.Domain.Bounty;
using Core.Domain.Locale;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Interact.Bounty.Description {
    public class YisoInteractBountyRewardItemUI : RunIBehaviour {
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI titleText;

        private bool active = false;

        public bool Active {
            get => active;
            private set {
                active = value;
                gameObject.SetActive(value);
            }
        }
        
        public void SetItem(YisoBounty.ItemUI item, YisoLocale.Locale locale) {
            itemImage.sprite = item.Icon;
            itemImage.enabled = true;
            titleText.SetText($"{item.GetName(locale)}");
            Active = true;
        }

        public void Clear() {
            itemImage.enabled = false;
            Active = false;
        }
    }
}