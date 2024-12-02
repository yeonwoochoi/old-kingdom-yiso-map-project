using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Core.Domain.Direction;
using Core.Domain.Event;
using Core.Domain.Locale;
using UnityEngine.Events;

namespace UI.Popup.Game.Dialogue {
    public sealed class YisoPopupGameDialogueArgs {
        public YisoGameDirection Direction { get; }

        private readonly List<UnityAction> onClicks = new();

        public YisoPopupGameDialogueArgs(YisoGameDirection direction) {
            Direction = direction;
        }

        public void AddOnClick([NotNull] UnityAction cb) {
            onClicks.Add(cb);
        }

        public void Invoke() {
            foreach (var cb in onClicks) cb();
        }
    }
}