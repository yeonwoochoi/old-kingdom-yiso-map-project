using System;
using Core.Behaviour;
using Core.Domain.Actor.Player;
using Core.Domain.Actor.Player.Modules.UI;
using Core.Domain.Item;
using Sirenix.OdinInspector;
using UI.Base;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace UI.Menu.Inventory.QuickSlot {
    public class YisoMenuInventoryQuickSlotsUI : YisoPlayerUIController {
        [SerializeField] private YisoMenuInventoryQuickSlotUI[] slotUIs;
        
        public event UnityAction OnCompleteWork; 
        
        private CanvasGroup canvasGroup;

        private YisoItem item = null;

        protected override void Start() {
            base.Start();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Active(bool flag, YisoItem item = null) {
            this.item = item;
            canvasGroup.Visible(flag);
        }

        /*protected override void OnEnable() {
            base.OnEnable();
            player.UIModule.OnSlotItemEvent += OnSlotItem;
            foreach (var slot in slotUIs) {
                slot.OnClickRaised += OnClickButton;
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            player.UIModule.OnSlotItemEvent -= OnSlotItem;
            foreach (var slot in slotUIs) {
                slot.OnClickRaised -= OnClickButton;
            }
        }

        private void OnClickButton(int index) {
            player.UIModule.SetItem(index, item as YisoUseItem);
            OnCompleteWork?.Invoke();
        }

        private void OnSlotItem(SlotItemEventArgs args) {
            var position = args.Position;
            switch (args) {
                case SlotItemSetEventArgs setArgs:
                    slotUIs[position].SetItem(setArgs.Item);
                    break;
                case SlotItemUnSetEventArgs:
                    slotUIs[position].Clear();
                    break;
                case SlotItemReplaceEventArgs replaceArgs:
                    slotUIs[position].SetItem(replaceArgs.Item);
                    break;
                case SlotItemUpdateEventArgs:
                    slotUIs[position].UpdateCount();
                    break;
            }
        }*/
    }
}