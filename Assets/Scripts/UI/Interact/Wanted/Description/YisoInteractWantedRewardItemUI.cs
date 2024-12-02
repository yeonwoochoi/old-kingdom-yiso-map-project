using Core.Behaviour;
using Core.Domain.Locale;
using Core.Domain.Wanted;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Interact.Wanted.Description {
    public class YisoInteractWantedRewardItemUI : RunIBehaviour {
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

        public void SetItem(YisoWanted.Item item, YisoLocale.Locale locale) {
            itemImage.sprite = item.Icon;
            itemImage.enabled = true;
            titleText.SetText($"{item.GetName(locale)} (x{item.Count})");
            Active = true;
        }

        public void Clear() {
            itemImage.enabled = false;
            Active = false;
        }
    }
}