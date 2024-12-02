using System;
using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Inventory.Holder {
    public class YisoMenuInventoryItemHolderUI : RunIBehaviour, IInstantiatable {
        [SerializeField, Title("Components")] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemCountText;
        [SerializeField] private TextMeshProUGUI equippedText;
        [SerializeField] private TextMeshProUGUI rankText;

        private Toggle toggle;

        public Toggle ItemToggle => toggle;

        public YisoItem Item { get; private set; } = null;

        public bool Active => Item != null;

        private bool init = false;

        private Material itemMaterial;

        public void Init() {
            itemImage.enabled = false;
            toggle = GetComponent<Toggle>();
            toggle.interactable = false;
            itemMaterial = itemImage.material;
            init = true;
        }

        public void UpdateCount(int count) {
            itemCountText.SetText($"x{count}");
        }

        public void UpdateEquip(bool equipped) {
            equippedText.enabled = equipped;
        }

        public void UpdateItem() {
            SetItem(Item);
        }
        
        public void SetItem(YisoItem item) {
            if (!init) Init();
            Item = item;
            var count = "";

            if (item is YisoEquipItem equipItem) {
                equippedText.enabled = equipItem.Equipped;
                SetRank(equipItem.Rank);
                
            } else {
                count = item.Quantity > 1 ? $"x{item.Quantity}" : "";
            }
            
            
            itemImage.sprite = item.Icon;
            itemImage.enabled = true;
            itemCountText.SetText(count);
            itemCountText.enabled = count != string.Empty;
            toggle.interactable = true;
        }

        private void SetRank(YisoEquipRanks rank) {
            itemMaterial.ActiveOutline(rank != YisoEquipRanks.N);
            if (rank == YisoEquipRanks.N) {
                rankText.SetText("");
                return;
            }
            
            var rankColor = rank.ToColor();
            itemMaterial.SetOutlineColor(rankColor);
            itemMaterial.ActiveOutlineDistortion(true);
            rankText.SetText(rank.ToString());
        }

        public void Clear() {
            Item = null;
            itemImage.enabled = false;
            itemImage.sprite = null;
            itemCountText.SetText("");
            toggle.isOn = false;
            toggle.interactable = false;
            equippedText.enabled = false;
            itemCountText.enabled = false;
            
            itemMaterial.DisableKeyword("OUTBASE_ON");
            rankText.SetText("");
        }
    }
}