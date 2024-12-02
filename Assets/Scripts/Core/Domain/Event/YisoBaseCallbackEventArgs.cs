using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Events;

namespace Core.Domain.Event {
    public abstract class YisoBaseCallbackEventArgs : EventArgs {
        private readonly List<UnityAction> onClickOkCbs = new();
        private readonly List<UnityAction> onClickCancelCbs = new();

        public int OkCbCount => onClickOkCbs.Count;
        public int CancelCbCount => onClickCancelCbs.Count;

        public void AddOkCb([NotNull] UnityAction cb) {
            onClickOkCbs.Add(cb);
        }

        public void AddCancelCb([NotNull] UnityAction cb) {
            onClickCancelCbs.Add(cb);
        }

        public void InvokeOkCbs() {
            foreach (var cb in onClickOkCbs) cb();
        }

        public void InvokeCancelCbs() {
            foreach (var cb in onClickCancelCbs) cb();
        }
    }
}