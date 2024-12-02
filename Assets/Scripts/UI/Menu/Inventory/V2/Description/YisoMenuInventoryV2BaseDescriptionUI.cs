using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Inventory.V2.Description {
    public abstract class YisoMenuInventoryV2BaseDescriptionUI<T> : YisoUIController where T : YisoItem {
        [SerializeField, Title("Basic")] protected TextMeshProUGUI itemNameText;
        [SerializeField] protected Image itemImage;
        [SerializeField] protected Image itemOutlineImage;
        [SerializeField] protected TextMeshProUGUI itemRankText;
        [SerializeField] protected TextMeshProUGUI slotOrTypeText;
        [SerializeField] protected TextMeshProUGUI combatRatingOrCountText;

        private RectTransform combatRatingTextRect;

        private CanvasGroup canvasGroup;

        public T Item { get; private set; }

        public bool Active { get; set; } = false;

        protected override void Start() {
            canvasGroup = GetComponent<CanvasGroup>();
            combatRatingTextRect = (RectTransform) combatRatingOrCountText.transform;
        }

        public void Visible(bool flag) {
            canvasGroup.Visible(flag);
        }

        public virtual void SetItem(T item) {
            Active = true;
            Item = item;
            itemNameText.SetText(item.GetName(CurrentLocale));
            itemImage.sprite = item.Icon;

            if (item is YisoEquipItem equipItem) {
                var rank = equipItem.Rank;
                itemRankText.SetText($"{rank.ToString(CurrentLocale)}({rank.ToString()})");
                slotOrTypeText.SetText(equipItem.Slot.ToString(CurrentLocale));
                combatRatingOrCountText.SetText(equipItem.CombatRating.ToCommaString());
                LayoutRebuilder.ForceRebuildLayoutImmediate(combatRatingTextRect);
            } else {
                itemRankText.SetText("-");
                slotOrTypeText.SetText(item.InvType.ToString(CurrentLocale));
                combatRatingOrCountText.SetText(item.Quantity.ToCommaString());
            }
        }

        public virtual void Clear() {
            itemNameText.SetText("");
            itemRankText.SetText("");
            slotOrTypeText.SetText("");
            combatRatingOrCountText.SetText("");
            Active = false;
        }
    }
}