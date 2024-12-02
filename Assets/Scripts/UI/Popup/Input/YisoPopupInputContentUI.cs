using System;
using System.Globalization;
using Core.Domain.Types;
using TMPro;
using UI.Popup.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Popup.Input {
    public class YisoPopupInputContentUI : YisoPopupBaseContentUI {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button okButton;
        [SerializeField] private Button cancelButton;

        private Image okButtonImage;
        private Color okButtonImageColor;
        
        protected override void Start() {
            base.Start();
            okButtonImage = okButton.GetComponent<Image>();
            okButtonImageColor = okButtonImage.color;
            inputField.onValueChanged.AddListener(OnInputValueChanged);
        }

        protected override void ClearPanel() {
            
        }

        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.INPUT_NUMBER;

        public void Show(string title, string content, UnityAction<int> onClickOk, UnityAction onClickCancel) {
            Visible(true);
            titleText.SetText(title);
            contentText.SetText(content);
            SetListeners(onClickOk, onClickCancel);
        }

        private void SetListeners(UnityAction<int> onClickOk, UnityAction onClickCancel) {
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(() => onClickOk(int.Parse(inputField.text)));
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(onClickCancel);
        }

        private void OnInputValueChanged(string value) {
            ActiveOkButton(value != string.Empty);
        }

        private void ActiveOkButton(bool flag) {
            okButtonImageColor.a = flag ? 1f : .5f;
            okButtonImage.color = okButtonImageColor;
        }
    }
}