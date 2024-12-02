using System.Text;
using Controller.Stage;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Stage;
using TMPro;
using UI.Popup.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Popup.SelectStage {
    public class YIsoPopupSelectStageContentUI : YisoPopupBaseContentUI {
        [SerializeField] private TextMeshProUGUI currentValueText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Slider slider;
        [SerializeField] private Button okButton;

        private UnityAction<int> onClickOk = null;

        private int selectedStageId = 0;
        
        protected override void Start() {
            base.Start();
            
            okButton.onClick.AddListener(() => onClickOk?.Invoke(selectedStageId));
            slider.onValueChanged.AddListener(value => SetCurrentValueText((int) value));
        }

        protected override void HandleData(object data = null) {
            onClickOk = (UnityAction<int>)data!;

            var stageService = YisoServiceProvider.Instance.Get<IYisoStageService>();
            var currentStage = stageService.GetCurrentStageId();
            selectedStageId = currentStage;
            var lastStage = stageService.GetLastStageId();

            slider.minValue = 1;
            slider.maxValue = lastStage;
            slider.value = currentStage;
        }

        private void SetCurrentValueText(int stageId) {
            var currentStageStr = $"선택된 스토리 ({stageId.ToCommaString()})";
            currentValueText.SetText(currentStageStr);
            selectedStageId = stageId;
        }

        private void SetDescriptionText(int lastStageId) {
            var builder = new StringBuilder("현재 스토리는 ");
            builder.Append("<size=140%><b>")
                .Append("\"")
                .Append(lastStageId.ToCommaString())
                .Append("\"")
                .Append("</b></size>");
            builder.Append("까지 구현되어 있습니다.");
            var descriptionStr = builder.ToString();
            descriptionText.SetText(descriptionStr);
        }

        protected override void ClearPanel() {
            selectedStageId = 1;
        }

        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.SELECT_STAGE;
    }
}