using System;
using Core.Behaviour;
using Core.Domain.Actor.Player;
using Core.Domain.Types;
using UI.Base;
using UnityEngine;
using Utils.Extensions;

namespace UI.Menu.Base {
    public abstract class YisoMenuBasePanelUI : YisoPlayerUIController {

        private CanvasGroup canvasGroup;

        protected override void Start() {
            base.Start();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private bool registered = false;

        public void Visible(bool flag, object data = null) {
            var value = flag ? 1f : 0f;
            canvasGroup.DOVisible(value, .1f);
            if (flag) {
                Init();
                if (!registered) RegisterEvents();
                OnVisible();
                if (data != null) HandleData(data);
            }
            else {
                if (registered) UnregisterEvents();
                ClearPanel();
            }
        }

        public abstract void ClearPanel();

        protected virtual void HandleData(object data) { }

        protected virtual void OnVisible() { }
        
        public virtual void Init() { }

        protected virtual void RegisterEvents() {
            registered = true;
        }

        protected virtual void UnregisterEvents() {
            registered = false;
        }

        public abstract YisoMenuTypes GetMenuType();
    }
}