using System;
using Character.Weapon;
using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Inventory.V2.Holder {
    public class YisoMenuInventoryV2ItemHolderUI : RunIBehaviour, IInstantiatable {
        [SerializeField] private bool equipped = false;
        [SerializeField] private Image outlineImage;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemCountText;
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private Image upImage;

        private Toggle toggle;

        public Toggle ItemToggle => toggle;

        private YisoItem currentItem = null;

        public bool Active => currentItem != null;

        private Material itemMaterial;
        
        public void Init() {
            toggle = GetComponent<Toggle>();
            itemMaterial = itemImage.material;
            Clear();
        }

        public void UpdateCount(int count) {
            itemCountText.SetText($"x{count}");
        }

        public void SetItem(YisoItem item, YisoWeapon.AttackType currentWeapon) {
            currentItem = item;
            var count = string.Empty;
            
            if (item is YisoEquipItem equipItem) {
                SetRank(equipItem.Rank);
                if (!equipped) {
                    SetDiff(currentWeapon);
                }
            } else {
                count = item.Quantity > 1 ? $"x{item.Quantity.ToCommaString()}" : "";
            }

            itemImage.sprite = item.Icon;
            itemImage.enabled = true;
            itemCountText.SetText(count);
            itemCountText.enabled = count != string.Empty;
            toggle.interactable = true;
        }

        private void SetRank(YisoEquipRanks rank) {
            var (outlineColor, patternColor) = rank.GetRankColorPair();
            outlineImage.color = outlineColor;
            if (rank == YisoEquipRanks.N) {
                rankText.SetText("");
                return;
            }
            
            itemMaterial.ActiveOutline(true);
            itemMaterial.SetOutlineColor(patternColor);
            itemMaterial.ActiveOutlineDistortion(true);
            rankText.SetText(rank.ToString());
        }

        public void SetDiff() {
            if (currentItem is not YisoEquipItem equipItem) return;
            SetDiff(equipItem);
        }

        public void SetDiff(YisoWeapon.AttackType type) {
            if (currentItem is not YisoEquipItem equipItem) return;
            SetDiff(equipItem, type);
        }

        private void SetDiff(YisoEquipItem item) {
            var statModule = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().StatModule;
            var crDiff = statModule.GetCombatRatingDiff(item);
            upImage.gameObject.SetActive(crDiff < 0);
        }

        private void SetDiff(YisoEquipItem item, YisoWeapon.AttackType type) {
            if (item.Slot == YisoEquipSlots.WEAPON && item.AttackType != type) {
                upImage.gameObject.SetActive(false);
                return;
            }
            var statModule = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().StatModule;
            var crDiff = statModule.GetCombatRatingDiff(item);
            upImage.gameObject.SetActive(crDiff < 0);
        }

        public void Clear() {
            currentItem = null;
            itemImage.enabled = false;
            itemImage.sprite = null;
            itemCountText.SetText("");
            toggle.isOn = false;
            toggle.interactable = false;
            itemCountText.enabled = false;
            
            outlineImage.color = YisoEquipRanks.N.GetRankColorPair().outline;
            
            itemMaterial.ActiveOutline(false);
            itemMaterial.ActiveOutlineDistortion(false);
            rankText.SetText("");
            upImage.gameObject.SetActive(false);
        }

        public T GetItem<T>() where T : YisoItem => currentItem as T;
    }
}