using System;
using System.Collections.Generic;
using Core.Domain.Actor.Player.Modules.Inventory.Reinforce;
using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Interact.Blacksmith.Description;
using UI.Menu.Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Blacksmith {
    public class YisoInteractBlacksmithDescriptionUI : YisoPlayerUIController {
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemTitleText;
        [SerializeField] private TextMeshProUGUI itemRankText;
        [SerializeField] private YisoMenuInventoryButtonUI upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeCostText;

        [SerializeField, Title("Descriptions")]
        private YisoInteractBlacksmithBaseDescriptionUI[] descriptionUis;

        private YisoInteractBlacksmithBaseDescriptionUI.Types currentType =
            YisoInteractBlacksmithBaseDescriptionUI.Types.NORMAL;

        private readonly
            Dictionary<YisoInteractBlacksmithBaseDescriptionUI.Types, YisoInteractBlacksmithBaseDescriptionUI>
            descriptions = new();

        private YisoItem item = null;
        
        private Material itemMaterial;
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
        public event UnityAction<YisoEquipItem, YisoPlayerInventoryReinforceResult> OnReinforceResultEvent; 

        protected override void Awake() {
            base.Awake();
            itemMaterial = itemImage.material;

            foreach (var description in descriptionUis) {
                descriptions[description.GetDescriptionType()] = description;
            }
            
            upgradeButton.InventoryButton.onClick.AddListener(() => {
                upgradeButton.InventoryButton.interactable = false;
                descriptions[currentType].OnClickReinforce(() => {
                    upgradeButton.InventoryButton.interactable = true;
                });
            });
        }
        
        protected override void OnEnable() {
            base.OnEnable();
            foreach (var description in descriptions.Values) {
                description.OnReinforceEvent += OnReinforce;
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            foreach (var description in descriptions.Values) {
                description.OnReinforceEvent -= OnReinforce;
            }
        }

        public void SetItem(YisoItem item) {
            this.item = item;
            var equipItem = (YisoEquipItem) item;
            
            itemImage.sprite = item.Icon;
            itemImage.enabled = true;
            itemTitleText.SetText(item.GetName(CurrentLocale));
            YisoPlayerInventoryReinforceResult result;
            if (currentType == YisoInteractBlacksmithBaseDescriptionUI.Types.NORMAL) result = player.InventoryModule.GetReinforceNormalResult(equipItem);
            else result = player.InventoryModule.GetReinforcePotentialResult(equipItem);
            
            descriptions[currentType].SetItem(equipItem, result, itemImage);
            SetRank(equipItem.Rank);
            var activeButton = descriptions[currentType].CanUpgrade() && player.InventoryModule.Money >= result.Price &&
                               !equipItem.Equipped;
            upgradeButton.Active(activeButton);
            upgradeCostText.SetText(ToPriceString(result.Price));
        }

        public void Clear() {
            item = null;
            itemImage.enabled = false;
            itemTitleText.SetText("");
            itemRankText.SetText("");
            
            itemMaterial.ActiveOutline(false);
            itemMaterial.ActiveOutlineDistortion(false);
            descriptions[currentType].Clear();
            descriptions[currentType].Visible(false);
            upgradeButton.Active(false);
        }

        private void OnReinforce(UnityAction<UnityAction<YisoPlayerInventoryReinforceResult>> onReinforce) {
            onReinforce?.Invoke(RaiseReinforceResult);
        }

        private void RaiseReinforceResult(YisoPlayerInventoryReinforceResult result) {
            OnReinforceResultEvent?.Invoke(item as YisoEquipItem, result);
        }

        private string ToPriceString(double cost) => $"<sprite=0> {cost.ToCommaString()}";

        private void SetRank(YisoEquipRanks rank) {
            if (rank == YisoEquipRanks.N || currentType == YisoInteractBlacksmithBaseDescriptionUI.Types.NORMAL) return;
            
            itemMaterial.ActiveOutline(true);
            var rankColor = rank.ToColor();
            itemMaterial.SetOutlineColor(rankColor);
            itemMaterial.ActiveOutlineDistortion(true);
            itemRankText.SetText($"{rank.ToString(CurrentLocale)}({rank.ToString()})");
        }
        
        public void SetDescription(YisoInteractBlacksmithBaseDescriptionUI.Types type) {
            currentType = type;
        }
    }
}