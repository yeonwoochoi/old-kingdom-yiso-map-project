using Manager_Temp_;

namespace Core.Service.Temp {
    public class YisoTempService : IYisoTempService {
        private GameManager gameManager;

        public void SetGameManager(GameManager gameManager) {
            if (this.gameManager != null) return;
            this.gameManager = gameManager;
        }

        public GameManager GetGameManager() => gameManager;
        public bool IsReady() => gameManager != null;
        public void OnDestroy() { }
        private YisoTempService() { }
        
        internal static YisoTempService CreateService() => new();
    }
}