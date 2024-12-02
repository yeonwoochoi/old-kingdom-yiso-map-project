using System;
using Core.Domain.Actor.Player.Modules.Inventory.Reinforce;
using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Types;
using UI.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Blacksmith.Description {
    public abstract class YisoInteractBlacksmithBaseDescriptionUI : YisoPlayerUIController {
        public event UnityAction<UnityAction<UnityAction<YisoPlayerInventoryReinforceResult>>> OnReinforceEvent;
        
        private CanvasGroup canvasGroup;
        protected YisoEquipItem item;
        protected YisoPlayerInventoryReinforceResult reinforceResult;
        protected Image itemImage;
        
        protected override void Start() {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Visible(bool flag) {
            canvasGroup.Visible(flag);
        }

        public virtual void SetItem(YisoEquipItem item, YisoPlayerInventoryReinforceResult result) {
            Visible(true);
            this.item = item;
            reinforceResult = result;
        }

        public virtual void SetItem(YisoEquipItem item, YisoPlayerInventoryReinforceResult result, Image itemImage) {
            Visible(true);
            this.item = item;
            reinforceResult = result;
            this.itemImage = itemImage;
            SetRank();
        }

        protected virtual void SetRank() {
            var rank = item.Rank;
            if (rank == YisoEquipRanks.N) return;
            var itemMaterial = itemImage.material;
            itemMaterial.ActiveOutline(true);
            var rankColor = item.Rank.ToColor();
            itemMaterial.SetOutlineColor(rankColor);
            itemMaterial.ActiveOutlineDistortion(true);
        }

        public virtual void Clear() {
            if (itemImage != null) {
                var itemMaterial = itemImage.material;
                itemMaterial.ActiveOutlineDistortion(false);
                itemMaterial.ActiveOutline(false);
            }
        }

        public abstract Types GetDescriptionType();

        public abstract bool CanUpgrade();

        public abstract void OnClickReinforce(UnityAction cb);

        protected void RaiseReinforceEvent(UnityAction<UnityAction<YisoPlayerInventoryReinforceResult>> onReinforce) {
            OnReinforceEvent?.Invoke(onReinforce);
        }

        public enum Types {
            NORMAL, POTENTIAL
        }
    }
}