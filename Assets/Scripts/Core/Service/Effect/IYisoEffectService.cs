using Core.Domain.Actor;
using Core.Domain.Effect;
using Core.Domain.Entity;
using UnityEngine.Events;
using Utils.ObjectId;

namespace Core.Service.Effect {
    public interface IYisoEffectService : IYisoService {
        void RegisterEvent(UnityAction<EffectEventArgs> handler);
        void UnregisterEvent(UnityAction<EffectEventArgs> handler);
        YisoObjectID CreateEffect(IYisoEntity entity, IYisoEffect effect);
    }
}