using System;
using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Item;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Inventory.QuickSlot {
    public class YisoMenuInventoryQuickSlotUI : RunIBehaviour {
        [SerializeField] private int index;
        [SerializeField] private CanvasGroup itemCanvas;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemCountText;

        public event UnityAction<int> OnClickRaised; 
        
        private Button button;
        
        private YisoUseItem item;

        protected override void Start() {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => OnClickRaised?.Invoke(index));
        }

        public void SetItem(YisoUseItem item) {
            this.item = item;
            itemImage.sprite = item.Icon;
            SetCount();
            itemCanvas.Visible(true);
        }

        private void SetCount() {
            itemCountText.SetText($"x{item.Quantity}");
        }

        public void UpdateCount() {
            SetCount();
        }

        public void Clear() {
            item = null;
            itemCanvas.Visible(false);
        }
    }
}