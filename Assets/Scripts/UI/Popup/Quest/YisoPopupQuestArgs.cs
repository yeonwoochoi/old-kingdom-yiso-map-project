using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Core.Domain.Quest;
using UI.Popup.Quest.Detail;
using UnityEngine.Events;

namespace UI.Popup.Quest {
    public sealed class YisoPopupQuestArgs {
        public YisoPopupQuestContentUI.Types Type { get; }
        public YisoQuest Quest { get; }
        
        public int DeathCount { get; set; } = -1;

        public YisoPopupQuestArgs(YisoPopupQuestContentUI.Types type, YisoQuest quest) {
            Type = type;
            Quest = quest;
        }

        private readonly List<UnityAction> onClickOks = new();
        private readonly List<UnityAction> onClickCancels = new();

        public void AddOnClickOk([NotNull] UnityAction cb) {
            onClickOks.Add(cb);
        }

        public void AddOnClickCancel([NotNull] UnityAction cb) {
            onClickCancels.Add(cb);
        }

        public void InvokeOkCallbacks() {
            foreach (var onClick in onClickOks) onClick();
        }

        public void InvokeCancelCallbacks() {
            foreach (var onClick in onClickCancels) onClick();
        }

        public void ClearOkCallbacks() {
            onClickOks.Clear();
        }
    }
}