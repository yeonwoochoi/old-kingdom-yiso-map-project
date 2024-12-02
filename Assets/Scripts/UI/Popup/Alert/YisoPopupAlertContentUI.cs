using System;
using Core.Domain.Types;
using TMPro;
using UI.Popup.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Popup.Alert {
    public class YisoPopupAlertContentUI : YisoPopupBaseContentUI {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button okButton;
        [SerializeField] private Button cancelButton;

        private CanvasGroup cancelButtonCanvas;

        private UnityAction onClickOk = null;
        private UnityAction onClickCancel = null;

        protected override void Start() {
            base.Start();
            okButton.onClick.AddListener(() => {
                onClickOk?.Invoke();
            });
            cancelButton.onClick.AddListener(() => {
                onClickCancel?.Invoke();
            });
        }

        public override void GetComponentOnAwake() {
            cancelButtonCanvas = cancelButton.GetComponent<CanvasGroup>();
        }

        protected override void ClearPanel() {
            cancelButtonCanvas.Visible(false);
            onClickOk = null;
            onClickCancel = null;
        }

        protected override void HandleData(object data = null) {
            var (title, content, onClickOk, onClickCancel) = ((string, string, UnityAction, UnityAction)) data;
            SetInfo(title, content);
            this.onClickOk = onClickOk;
            if (onClickCancel == null) return;
            cancelButtonCanvas.Visible(true);
            this.onClickCancel = onClickCancel;
        }

        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.ALERT;
        
        private void SetInfo(string title, string content) {
            titleText.SetText(title);
            contentText.SetText(content);
            cancelButtonCanvas.Visible(false);
        }
    }
}