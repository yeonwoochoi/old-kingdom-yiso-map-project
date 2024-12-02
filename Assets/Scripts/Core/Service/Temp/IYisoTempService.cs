using Manager_Temp_;

namespace Core.Service.Temp {
    public interface IYisoTempService : IYisoService {
        public void SetGameManager(GameManager gameManager);
        public GameManager GetGameManager();
    }
}