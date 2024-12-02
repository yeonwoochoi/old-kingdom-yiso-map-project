using UnityEngine.Events;

namespace Core.Service.UI.Menu {
    public interface IYisoMenuUIService : IYisoService {
        public void RegisterOnVisibleOverlyUI(UnityAction<bool, UnityAction> handler);
        public void UnregisterVisibleOverlayUI(UnityAction<bool, UnityAction> handler);
        public void RaiseOnVisibleOverlayUI(bool flag, UnityAction callback = null);
    }
}