using Core.Behaviour;
using Core.Domain.CoolDown;
using UnityEngine.Events;
using Utils.ObjectId;

namespace Core.Service.CoolDown {
    public interface IYisoCoolDownService : IYisoService {
        void RegisterEvent(UnityAction<CoolDownEventArgs> handler);
        void UnregisterEvent(UnityAction<CoolDownEventArgs> handler);
        YisoObjectID CreateCoolDown(int sourceId, double coolDown);
        bool RemoveCoolDown(YisoObjectID id);
        bool ExistCoolDown(int sourceId);
        bool ExistCoolDown(YisoObjectID id);
        bool TryGetCoolDown(int sourceId, out YisoPlayerCoolDownHolder holder);
        bool TryGetCoolDown(YisoObjectID id, out YisoPlayerCoolDownHolder holder);
    }
}