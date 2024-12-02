using Core.Domain.Item;
using Core.Domain.Types;
using Core.Service;
using Core.Service.UI.Popup2;
using TMPro;
using UI.Popup.Base;
using UI.Popup2.Number;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Popup.Inventory {
    public class YisoPopupInventoryDropCountContentUI : YisoPopupBaseContentUI {
        [SerializeField] private TextMeshProUGUI itemInfoText;
        [SerializeField] private Button countButton;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button okButton;

        private YisoItem item;
        private UnityAction<int> onClickOk;
        private UnityAction onClickCancel;
        private int count;
        
        protected override void Start() {
            base.Start();
            okButton.onClick.AddListener(() => onClickOk?.Invoke(count));
            cancelButton.onClick.AddListener(() => onClickCancel?.Invoke());
            countButton.onClick.AddListener(OnClickCountButton);
        }

        protected override void HandleData(object data = null) {
            var (item, onClickOk, onClickCancel) = ((YisoItem, UnityAction<int>, UnityAction)) data;

            this.item = item;
            this.onClickOk = onClickOk;
            this.onClickCancel = onClickCancel;
            
            var itemName = item.GetName(CurrentLocale);
            var itemQuantity = item.Quantity;
            itemInfoText.SetText($"{itemName} (x{itemQuantity.ToCommaString()})");
        }

        private void OnClickCountButton() {
            var maxCount = item.Quantity;
            var maxDigits = maxCount.ToString().Length;
            var args = new YisoPopup2NumberInputArgs {
                MaxDigits = maxDigits,
                MaxValue = maxCount
            };
            args.OkCbList.Add(value => {
                count = value;
                okButton.interactable = value > 0;
                countText.SetText(value.ToCommaString());
            });
            YisoServiceProvider.Instance.Get<IYisoPopup2UIService>().ShowNumberInput(args);
        }
        
        protected override void ClearPanel() {
            item = null;
            onClickOk = null;
            onClickCancel = null;
            count = 0;
            countText.SetText("0");
        }

        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.DROP_ITEM_COUNT;
    }
}