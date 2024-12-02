using Core.Domain.Locale;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using TMPro;
using UI.Popup.Base;
using UI.Popup.Bounty.Detail;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup.Bounty {
    public class YisoPopupBountyContentUI : YisoPopupBaseContentUI {
        [SerializeField] private TextMeshProUGUI bountyTitleText;
        [SerializeField] private Image targetImage;
        [SerializeField] private YisoPopupBountyDetailContentUI contentUI;
        [SerializeField] private Button okButton;
        [SerializeField] private Button cancelButton;

        private YisoPopupBountyData bountyData;

        protected override void Start() {
            base.Start();
            okButton.onClick.AddListener(() => bountyData?.InvokeOkCallbacks());
        }

        protected override void HandleData(object data = null) {
            bountyData = (YisoPopupBountyData) data!;
            targetImage.sprite = bountyData.Bounty.TargetIcon;
            bountyTitleText.SetText(GetBountyTitle(bountyData.Bounty.GetTargetName(CurrentLocale)));
            contentUI.SetBounty(bountyData.Bounty);
        }

        protected override void ClearPanel() {
            contentUI.Clear();
        }

        private string GetBountyTitle(string targetName) => CurrentLocale == YisoLocale.Locale.KR ? $"{targetName} 처치" : $"Kill {targetName}";

        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.BOUNTY;
    }
}