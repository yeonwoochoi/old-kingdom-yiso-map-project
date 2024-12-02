using Core.Domain.Types;
using UI.Base;
using UnityEngine;
using Utils.Extensions;

namespace UI.Game.Base {
    public abstract class YisoGameBasePanelUI : YisoUIController {
        private CanvasGroup canvasGroup;

        protected override void Awake() {
            base.Awake();
            GetComponentOnAwake();
        }

        protected override void Start() {
            base.Start();
            canvasGroup = GetComponent<CanvasGroup>();
            Init();
        }

        public void Visible(bool flag, object data = null) {
            var value = flag.FlagToFloat();
            if (VisibleOnlyAlpha()) 
                canvasGroup.alpha = value;
            else 
                canvasGroup.DOVisible(value, .1f);
            if (flag) {
                if (data != null) HandleData(data);
                RegisterEvents();
            } else {
                ClearPanel();
                UnregisterEvents();
            }
        }

        protected virtual bool VisibleOnlyAlpha() => false;

        protected virtual void GetComponentOnAwake() { }
        
        protected virtual void HandleData(object data) { }

        protected virtual void RegisterEvents() { }

        protected virtual void UnregisterEvents() { }

        protected virtual void ClearPanel() { }

        protected virtual void Init() { }

        public abstract YisoGameUITypes GetUIType();
    }
}