using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Core.Domain.Dialogue;
using UnityEngine.Events;

namespace UI.Popup.Npc.Dialogue {
    public class YisoPopupNpcDialogueArgs {
        public YisoDialogue Dialogue { get; }
        private readonly List<UnityAction> onClickCloses = new();
        public YisoPopupNpcDialogueArgs(YisoDialogue dialogue) {
            Dialogue = dialogue;
        }

        public void AddOnClickClose([NotNull] UnityAction cb) {
            onClickCloses.Add(cb);
        }

        public void InvokeOnClose() {
            foreach (var cb in onClickCloses) cb();
        }
    }
}