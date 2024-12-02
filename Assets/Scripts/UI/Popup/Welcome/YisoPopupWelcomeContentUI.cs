using Character.Weapon;
using Core.Domain.Item;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Data.Item;
using TMPro;
using UI.Popup.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Popup.Welcome {
    public class YisoPopupWelcomeContentUI : YisoPopupBaseContentUI {
        [SerializeField] private Image itemImage;
        [SerializeField] private Image itemOutlineImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemRankText;
        [SerializeField] private Button okButton;

        private UnityAction onClickOk = null;
        private Material itemMaterial;
        private YisoItem randomItem;

        protected override void Start() {
            base.Start();
            okButton.onClick.AddListener(() => onClickOk?.Invoke());
        }

        public override void GetComponentOnAwake() {
            itemMaterial = itemImage.material;
        }

        protected override void HandleData(object data = null) {
            onClickOk = (UnityAction) data!;
            var itemService = YisoServiceProvider.Instance.Get<IYisoItemService>();
            randomItem = itemService.CreateRandomWeapon(YisoWeapon.AttackType.None, YisoEquipRanks.S);
            var randomEquipItem = (YisoEquipItem) randomItem;
            itemImage.sprite = randomItem.Icon;
            itemNameText.SetText(randomItem.GetName());
            SetRank(randomEquipItem);
        }

        private void SetRank(YisoEquipItem item) {
            var (outlineColor, patternColor) = item.Rank.GetRankColorPair();
            itemOutlineImage.color = outlineColor;

            itemNameText.color = patternColor;
            itemRankText.color = patternColor;
            
            itemMaterial.ActiveOutline(true);
            itemMaterial.SetOutlineColor(patternColor);
            itemMaterial.ActiveOutlineDistortion(true);
            itemRankText.SetText($"{item.Rank.ToString(CurrentLocale)}({item.Rank.ToString()})");
        }
        
        protected override void ClearPanel() {
            randomItem = null;
            itemNameText.SetText("");
            itemRankText.SetText("");
            itemMaterial.ActiveOutline(false);
            itemMaterial.ActiveOutlineDistortion(false);
            itemImage.sprite = null;
        }

        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.WELCOME;
    }
}