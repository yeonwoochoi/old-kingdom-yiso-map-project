using System;
using Core.Domain.Locale;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Character;
using Sirenix.OdinInspector;
using TMPro;
using UI.Popup.Base;
using UI.Popup.Quest.Detail;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Popup.Quest {
    public class YisoPopupQuestContentUI : YisoPopupBaseContentUI {
        [SerializeField, Title("Title")] private TextMeshProUGUI popupTitleText;
        [SerializeField] private TextMeshProUGUI questTitleText;
        [SerializeField, Title("Contents")] private YisoPopupQuestDetailContentUI contentUI;
        [SerializeField, Title("Buttons")] private Button okButton;
        [SerializeField] private Button cancelButton;

        [SerializeField, Title("Block")] private CanvasGroup warningPopupCanvas;
        [SerializeField] private CanvasGroup warningImageCanvas;
        
        private YisoPopupQuestArgs args;

        private CanvasGroup cancelButtonCanvas;

        protected override void Start() {
            base.Start();
            okButton.onClick.AddListener(() => args?.InvokeOkCallbacks());
            cancelButton.onClick.AddListener(() => args?.InvokeCancelCallbacks());
        }

        protected override void HandleData(object data = null) {
            args = (YisoPopupQuestArgs) data!;
            
            questTitleText.SetText(args.Quest.GetName(CurrentLocale));
            popupTitleText.SetText(GetTitle());
            
            contentUI.SetQuest(args.Quest);
            
            if (args.Type != Types.COMPLETE) return;
            var player = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer();
            if (args.Quest.CanGetReward(player, out var reason)) return;
            warningImageCanvas.Visible(true);
            cancelButtonCanvas.Visible(true);
            args.ClearOkCallbacks();
            args.AddOnClickOk(() => warningPopupCanvas.Visible(true));
        }

        public override void GetComponentOnAwake() {
            cancelButtonCanvas = cancelButton.GetComponent<CanvasGroup>();
        }

        protected override void ClearPanel() {
            args = null;
            contentUI.Clear();
            cancelButtonCanvas.Visible(false);
            warningImageCanvas.Visible(false);
            warningPopupCanvas.Visible(false);
        }

        private string GetTitle() =>
            args.Type switch {
                Types.START => CurrentLocale == YisoLocale.Locale.KR ? "퀘스트 알림" : "Quest Info",
                Types.RE_START => CurrentLocale == YisoLocale.Locale.KR ? "퀘스트 재도전" : "Quest Restart",
                Types.COMPLETE => CurrentLocale == YisoLocale.Locale.KR ? "퀘스트 종료" : "Quest Complete",
                _ => throw new ArgumentOutOfRangeException()
            };

        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.QUEST;
        
        public enum Types {
            START, RE_START, COMPLETE
        }
    }
}