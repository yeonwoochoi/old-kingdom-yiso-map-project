using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Service;
using Core.Service.Character;
using Core.Service.Game;
using Core.Service.UI.Game;
using UnityEngine;

namespace Items.Pickable {
    [AddComponentMenu("Yiso/Items/Item Pickable Object")]
    public class YisoItemPickableObject : YisoPickableObject {
        protected YisoItem item;
        protected YisoLocale inventoryFullMessage;
        public YisoItem Item => item;

        public override void Initialization() {
            base.Initialization();
            inventoryFullMessage = new YisoLocale {
                kr = "인벤토리가 가득 찼습니다. 더 이상 아이템을 주울 수 없습니다.",
                en = "Inventory is full. You cannot pick up more items."
            };
        }

        public virtual void SetItem(YisoItem item) {
            this.item = item;
            itemSprite.sprite = item.Icon;
            itemNameText.SetText(item.GetName(YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale()));
        }

        protected override bool CheckIfPickable(GameObject picker) {
            if (!base.CheckIfPickable(picker)) return false;
            if (item == null) return false;
            if (YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule.CanAdd(item)) {
                return true;
            }
            else {
                var currentLocale = YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
                YisoServiceProvider.Instance.Get<IYisoGameUIService>()
                    .FloatingText(inventoryFullMessage[currentLocale]);
                return false;
            }
        }

        protected override void Pick(GameObject picker) {
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule.AddItem(item);
        }
    }
}