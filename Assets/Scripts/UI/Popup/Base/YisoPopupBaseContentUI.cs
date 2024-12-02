using Core.Domain.Types;
using UI.Base;
using UnityEngine;
using Utils.Extensions;

namespace UI.Popup.Base {
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class YisoPopupBaseContentUI : YisoUIController {
        private CanvasGroup canvasGroup;

        protected override void Awake() {
            base.Awake();
            GetComponentOnAwake();
        }

        protected override void Start() {
            base.Start();
            canvasGroup = GetComponent<CanvasGroup>();
            ClearPanel();
        }

        public void Visible(bool flag, object data = null) {
            var value = flag ? 1f : 0f;
            canvasGroup.DOVisible(value, .1f);
            if (flag) {
                if (data != null) HandleData(data);
                OnVisible();
                RegisterEvents();
            } else {
                ClearPanel();
                UnregisterEvents();
            }
        }
        
        protected virtual void HandleData(object data = null) { }

        public virtual void GetComponentOnAwake() { }

        protected abstract void ClearPanel();

        protected virtual void RegisterEvents() { }
        protected virtual void UnregisterEvents() { }

        protected virtual void OnVisible() { }
        
        public abstract YisoPopupTypes GetPopupType();
    }
}