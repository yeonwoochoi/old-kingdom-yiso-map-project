using Core.Domain.Types;
using UI.Popup2;
using UI.Popup2.Number;
using UnityEngine.Events;

namespace Core.Service.UI.Popup2 {
    public class YisoPopup2UIService : IYisoPopup2UIService {
        private YisoPopup2CanvasUI canvasUI = null;
        private bool isActive = false;

        public void SetCanvasUI(YisoPopup2CanvasUI canvasUI) {
            this.canvasUI = canvasUI;
        }

        public void ShowNumberInput(YisoPopup2NumberInputArgs args) {
            args.OkCbList.Add(_ => {
                canvasUI.HideCanvas();
                isActive = false;
            });
            canvasUI.ShowCanvas(YisoPopup2Types.NUMBER_INPUT, args);
            isActive = true;
        }

        private YisoPopup2UIService() {
            YisoServiceProvider.Instance.Get<IYisoUIService>().AddSubUIService(this);
        }

        internal static YisoPopup2UIService CreateService() => new();

        public void OnDestroy() { }
        public bool IsReady() => canvasUI != null;
        public bool IsActive() => isActive;
    }
}