using System.Collections.Generic;
using Core.Domain.Event;
using UnityEngine.Events;

namespace UI.Popup.Alert {
    public sealed class YisoPopupAlertArgs : YisoBaseCallbackEventArgs {
        public string Title { get; set; }
        public string Content { get; set; }
        
        public bool HideCancel { get; set; }
    }
}