using Core.Domain.Types;
using UI.Base;
using UnityEngine;
using Utils.Extensions;

namespace UI.Popup2.Base {
    public abstract class YisoPopup2BaseContentUI : YisoUIController {
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
            var value = flag.FlagToFloat();
            canvasGroup.DOVisible(value, .1f);
            if (flag) {
                if (data != null) HandleData(data);
                RegisterEvents();
            } else {
                ClearPanel();
                UnregisterEvents();
            }
        }

        public abstract YisoPopup2Types GetPopupTypes();

        protected virtual void HandleData(object data = null) { }

        protected abstract void ClearPanel();

        protected virtual void GetComponentOnAwake() { }

        public virtual void RegisterEvents() { }
        public virtual void UnregisterEvents() { }
    }
}