using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Core.Domain.Bounty;
using UnityEngine.Events;

namespace UI.Popup.Bounty {
    public sealed class YisoPopupBountyData {
        public YisoBounty Bounty { get; }
        private readonly List<UnityAction> onClickOks = new();

        public YisoPopupBountyData(YisoBounty bounty) {
            Bounty = bounty;
        }
        

        public void AddOnClickOK([NotNull] UnityAction handler) {
            onClickOks.Add(handler);
        }

        public void InvokeOkCallbacks() {
            foreach (var callback in onClickOks) callback();
        }
    }
}