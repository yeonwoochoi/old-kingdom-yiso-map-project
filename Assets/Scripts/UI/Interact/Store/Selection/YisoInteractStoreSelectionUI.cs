using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Item;
using Core.Service;
using Core.Service.ObjectPool;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Interact.Store.Event;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Store.Selection {
    public class YisoInteractStoreSelectionUI : YisoUIController {
        [SerializeField, Title("Prefab")] private GameObject itemPrefab;
        [SerializeField] private GameObject content;
        [SerializeField, Title("Info")] private TextMeshProUGUI totalPriceText;
        [SerializeField] private TextMeshProUGUI selectedCountText;
        [SerializeField] private Button sellButton;
        [SerializeField] private YisoInteractStoreSelectionItemHolderUI[] defaultItems;

        public event UnityAction<List<string>, double> OnSellEvent;

        private readonly List<YisoInteractStoreSelectionItemHolderUI> items = new();
        private IYisoObjectPoolService objectPoolService;

        private CanvasGroup canvasGroup;

        protected override void Start() {
            canvasGroup = GetComponent<CanvasGroup>();
            objectPoolService = YisoServiceProvider.Instance.Get<IYisoObjectPoolService>();
            objectPoolService.WarmPool(itemPrefab, 15, content);

            foreach (var item in defaultItems) {
                item.Init();
                item.gameObject.SetActive(false);
                items.Add(item);
            }

            this.UpdateAsObservable().Select(_ => items.Count(item => item.Active) > 0)
                .SubscribeToInteractable(sellButton)
                .AddTo(this);
        }

        public void Visible(bool flag) {
            canvasGroup.Visible(flag);
        }

        public void OnSelected(StoreUIInventoryItemSelectedEventArgs args) {
            var item = args.Item;
            YisoInteractStoreSelectionItemHolderUI holder;
            var index = -1;
            if (args.Selected) {
                index = GetNonActiveIndex();
                holder = items[index];
                holder.SetItem(item, item.SellPrice, CurrentLocale);
                holder.gameObject.SetActive(true);
            }
            else {
                index = GetIndexById(item.ObjectId);
                holder = items[index];
                holder.Clear();
                holder.gameObject.SetActive(false);
            }

            totalPriceText.SetText(GetPrice().ToCommaString());
            selectedCountText.SetText($"({items.Count(i => i.Active)})");
        }

        public void OnClickSell() {
            var ids = items.Where(item => item.Active).Select(item => item.Item.ObjectId).ToList();
            OnSellEvent?.Invoke(ids, GetPrice());
        }

        private int GetNonActiveIndex() {
            for (var i = 0; i < items.Count; i++) {
                if (items[i].Active) continue;
                return i;
            }

            throw new Exception("All holders are active!");
        }

        private int GetIndexById(string objectId) {
            for (var i = 0; i < items.Count; i++) {
                if (!items[i].Active) continue;
                if (items[i].Item.ObjectId != objectId) continue;
                return i;
            }

            throw new Exception($"Item(id={objectId}) not exist!");
        }

        private double GetPrice() {
            return items.Where(item => item.Active).Sum(item => item.Price);
        }

        public void Clear() {
            totalPriceText.SetText("0");
            selectedCountText.SetText("(0)");
            foreach (var holder in items) {
                holder.Clear();
                holder.gameObject.SetActive(false);
            }
        }
    }
}