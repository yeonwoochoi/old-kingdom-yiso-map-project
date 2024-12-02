using System;
using Core.Behaviour;
using Core.Domain.Actor.Player;
using Core.Domain.Item;
using Core.Domain.Quest;
using Core.Service;
using Core.Service.UI.Menu;
using Sirenix.OdinInspector;
using UI.Base;
using UI.Menu.Inventory.QuickSlot;
using UI.Menu.Quest;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu {
    public class YisoMenuOverlayUI : YisoPlayerUIController {
        [SerializeField, Title("Panels")] private YisoMenuInventoryQuickSlotsUI quickSlotsUI;
        [SerializeField] private YisoMenuQuestDrawPopupUI drawPopupUI;
        
        [SerializeField, Title("Background")] private Image touchImage;
        private CanvasGroup canvasGroup;
        
        private Action onTouch = null;
        private UnityAction callback = null;

        private bool activeBackgroundTouch = true;

        protected override void Start() {
            base.Start();
            canvasGroup = GetComponent<CanvasGroup>();
            touchImage.OnPointerClickAsObservable()
                .Subscribe(OnTouch)
                .AddTo(this);
        }

        protected override void OnEnable() {
            base.OnEnable();
            YisoServiceProvider.Instance.Get<IYisoMenuUIService>().RegisterOnVisibleOverlyUI(OnOverlayUIEvent);
        }

        protected override void OnDisable() {
            base.OnDisable();
            YisoServiceProvider.Instance.Get<IYisoMenuUIService>().UnregisterVisibleOverlayUI(OnOverlayUIEvent);
        }

        private void OnOverlayUIEvent(bool flag, UnityAction callback) {
            Active(flag);
            this.callback = callback;
        }

        private void OnTouch(PointerEventData data) {
            if (!activeBackgroundTouch) return;
            callback?.Invoke();
        }

        public void Active(bool flag) {
            canvasGroup.Visible(flag);
            touchImage.raycastTarget = flag;
        }
    }
}