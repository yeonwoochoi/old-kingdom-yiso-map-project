using Core.Domain.Types;
using UI.Base;
using UnityEngine;
using Utils.Extensions;

namespace UI.Interact.Base {
    public abstract class YisoInteractBasePanelUI : YisoPlayerUIController {
        private CanvasGroup canvasGroup;

        protected override void Start() {
            base.Start();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Visible(bool flag, object data = null) {
            var value = flag ? 1f : 0f;
            canvasGroup.DOVisible(value, .1f);
            if (flag) {
                if (data != null)
                    HandleData(data);
                RegisterEvents();
                OnVisible();
            }
            else {
                ClearPanel();
                UnregisterEvents();
            }
        }
        
        public virtual void HandleData(object data) { }
        
        public virtual void OnVisible() { }

        public abstract void ClearPanel();

        public virtual void Init() { }

        protected virtual void RegisterEvents() {
            
        }

        protected virtual void UnregisterEvents() {
            
        }

        public abstract YisoInteractTypes GetType();
    }
}