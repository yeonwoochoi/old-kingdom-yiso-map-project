using UI.Popup2;
using UI.Popup2.Number;
using UnityEngine.Events;

namespace Core.Service.UI.Popup2 {
    public interface IYisoPopup2UIService : IYisoSubUIService {
        public void SetCanvasUI(YisoPopup2CanvasUI canvasUI);
        public void ShowNumberInput(YisoPopup2NumberInputArgs args);
    }
}