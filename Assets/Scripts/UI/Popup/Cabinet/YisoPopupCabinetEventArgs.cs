using System;
using Core.Domain.Cabinet;
using Core.Domain.Event;

namespace UI.Popup.Cabinet {
    public class YisoPopupCabinetEventArgs : YisoBaseCallbackEventArgs {
        public YisoCabinet Cabinet { get; }

        public string OkButtonText { get; set; } = string.Empty;
        public string CancelButtonText { get; set; } = string.Empty;

        public YisoPopupCabinetEventArgs(YisoCabinet cabinet) {
            Cabinet = cabinet;
        }
    }
}