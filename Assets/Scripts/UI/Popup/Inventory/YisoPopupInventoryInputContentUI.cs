using System;
using System.Text;
using Core.Domain.Item;
using Core.Domain.Locale;
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
    public class YisoPopupInventoryInputContentUI : YisoPopupBaseContentUI {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Button countButton;
        [SerializeField] private Button okButton;
        [SerializeField] private Button cancelButton;

        private YisoItem item;
        private int count = 1;
        private YisoPopupInventoryInputArgs cachedArgs;

        protected override void Start() {
            base.Start();
            okButton.onClick.AddListener(() => {
                if (cachedArgs == null) return;
                foreach (var onClick in cachedArgs.OnClickOkList)
                    onClick(count);
            });
            cancelButton.onClick.AddListener(() => {
                if (cachedArgs == null) return;
                foreach (var onClick in cachedArgs.OnClickCloseList)
                    onClick();
            });
            countButton.onClick.AddListener(OnClickCountButton);
        }

        protected override void HandleData(object data = null) {
            var args = (YisoPopupInventoryInputArgs)data!;

            cachedArgs = args;
            item = args.Item;

            var itemName = item.GetName(CurrentLocale);
            itemNameText.SetText(itemName);

            titleText.SetText(GetTitleString(args.Type));
            contentText.SetText(GetContentString(args.Type, item.InvType == YisoItem.InventoryType.EQUIP));
            
            var itemCount = args.Type switch {
                Types.PURCHASE => 1,
                Types.SELL => 1,
                Types.STORAGE_PULL => item.Quantity,
                Types.STORAGE_PUSH => item.Quantity,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            countText.SetText(GetCountText(itemCount, args));

            countButton.interactable = item.InvType != YisoItem.InventoryType.EQUIP;
        }

        protected override void ClearPanel() {
            titleText.SetText("");
            contentText.SetText("");
            countText.SetText("");
            itemNameText.SetText("");
            countButton.interactable = false;
            item = null;
            count = 1;
            countText.SetText("1");
        }

        private string GetCountText(int count, YisoPopupInventoryInputArgs args) {
            var itemCountText = count.ToCommaString();
            if (args.Type is Types.STORAGE_PULL or Types.STORAGE_PUSH) return itemCountText;
            var builder = new StringBuilder(itemCountText).Append(" (<sprite=0> ");
            var priceText = args.Price * count;
            builder.Append(priceText.ToCommaString());
            builder.Append(")");
            return builder.ToString();
        }

        private void OnClickCountButton() {
            var args = new YisoPopup2NumberInputArgs {
                MaxDigits = 5,
                MaxValue = 10000,
                StartValue = count,
            };
            args.OkCbList.Add(value => {
                count = value;
                okButton.interactable = value > 0;
                countText.SetText(GetCountText(value, cachedArgs));
            });
            YisoServiceProvider.Instance.Get<IYisoPopup2UIService>().ShowNumberInput(args);
        }

        private string GetContentString(Types type, bool equip) {
            if (equip) {
                return type switch {
                    Types.PURCHASE => CurrentLocale == YisoLocale.Locale.KR ? "구매할 아이템을 확인해 주세요" : "Item Purchase",
                    Types.SELL => CurrentLocale == YisoLocale.Locale.KR ? "판매할 아이템을 확인해 주세요" : "Item Sell",
                    Types.STORAGE_PUSH => CurrentLocale == YisoLocale.Locale.KR ? "보관할 아이템을 확인해 주세요" : "Storage Push",
                    Types.STORAGE_PULL => CurrentLocale == YisoLocale.Locale.KR ? "꺼내올 아이템을 확인해 주세요" : "Storage Pull",
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
            }

            return type switch {
                Types.PURCHASE => CurrentLocale == YisoLocale.Locale.KR ? "구매할 아이템의 개수를 입력해주세요" : "Item Purchase",
                Types.SELL => CurrentLocale == YisoLocale.Locale.KR ? "판매할 아이템의 개수를 입력해주세요" : "Item Sell",
                Types.STORAGE_PUSH => CurrentLocale == YisoLocale.Locale.KR ? "보관할 아이템의 개수를 입력해주세요" : "Storage Push",
                Types.STORAGE_PULL => CurrentLocale == YisoLocale.Locale.KR ? "꺼내올 아이템의 개수를 입력해주세요" : "Storage Pull",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private string GetTitleString(Types type) => type switch {
            Types.PURCHASE => CurrentLocale == YisoLocale.Locale.KR ? "아이템 구매" : "Item Purchase",
            Types.SELL => CurrentLocale == YisoLocale.Locale.KR ? "아이템 판매" : "Item Sell",
            Types.STORAGE_PUSH => CurrentLocale == YisoLocale.Locale.KR ? "아이템 보관" : "Storage Push",
            Types.STORAGE_PULL => CurrentLocale == YisoLocale.Locale.KR ? "아이템 꺼내기" : "Storage Pull",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.INVENTORY_INPUT;

        public enum Types {
            PURCHASE,
            SELL,
            STORAGE_PULL,
            STORAGE_PUSH
        }
    }
}