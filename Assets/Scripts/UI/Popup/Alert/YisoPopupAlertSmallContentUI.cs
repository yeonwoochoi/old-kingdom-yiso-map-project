using Core.Domain.Types;
using TMPro;
using UI.Popup.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Popup.Alert {
    public class YisoPopupAlertSmallContentUI : YisoPopupBaseContentUI {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button okButton;
        [SerializeField] private Button cancelButton;

        private CanvasGroup cancelButtonCanvas;

        private YisoPopupAlertArgs args;

        protected override void Start() {
            base.Start();
            okButton.onClick.AddListener(() => { args.InvokeOkCbs(); });
            cancelButton.onClick.AddListener(() => { args.InvokeCancelCbs(); });
        }

        public override void GetComponentOnAwake() {
            cancelButtonCanvas = cancelButton.GetComponent<CanvasGroup>();
        }

        protected override void ClearPanel() {
            cancelButtonCanvas.Visible(false);
            args = null;
        }

        protected override void HandleData(object data = null) {
            args = (YisoPopupAlertArgs) data!;
            SetInfo(args.Title, args.Content);
            cancelButtonCanvas.Visible(!args.HideCancel);
        }

        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.ALERT_S;

        private void SetInfo(string title, string content) {
            titleText.SetText(title);
            contentText.SetText(content);
            cancelButtonCanvas.Visible(false);
        }
    }
}