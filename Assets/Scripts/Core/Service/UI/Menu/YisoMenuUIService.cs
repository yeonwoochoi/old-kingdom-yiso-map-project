using System;
using UnityEngine.Events;

namespace Core.Service.UI.Menu {
    public class YisoMenuUIService : IYisoMenuUIService {
        private event UnityAction<bool, UnityAction> OnVisibleOverlayUI;

        public void RegisterOnVisibleOverlyUI(UnityAction<bool, UnityAction> handler) {
            OnVisibleOverlayUI += handler;
        }

        public void UnregisterVisibleOverlayUI(UnityAction<bool, UnityAction> handler) {
            OnVisibleOverlayUI -= handler;
        }

        public void RaiseOnVisibleOverlayUI(bool flag, UnityAction callback = null) {
            OnVisibleOverlayUI?.Invoke(flag, callback);
        }

        public bool IsReady() => true;

        public void OnDestroy() { }

        internal static YisoMenuUIService CreateService() => new();
    }
}