using System.Collections.Generic;
using System.Linq;
using Core.Behaviour;
using Core.Domain.Actor;
using Core.Domain.Effect;
using Core.Domain.Entity;
using Core.Service.CoolDown;
using UnityEngine;
using UnityEngine.Events;
using Utils.ObjectId;

namespace Core.Service.Effect {
    public class YisoEffectService : IYisoEffectService {
        private event UnityAction<EffectEventArgs> OnEffectEvent;
        
        private readonly Dictionary<YisoObjectID, YisoEffect> effects = new();

        private readonly RunIBehaviour bootstrap;
        private readonly IYisoCoolDownService coolDownService;

        private YisoEffectService(RunIBehaviour bootstrap) {
            this.bootstrap = bootstrap;
            coolDownService = YisoServiceProvider.Instance.Get<IYisoCoolDownService>();
        }
        internal static YisoEffectService CreateService(RunIBehaviour boottrap) => new(boottrap);

        public void RegisterEvent(UnityAction<EffectEventArgs> handler) {
            OnEffectEvent += handler;
        }

        public void UnregisterEvent(UnityAction<EffectEventArgs> handler) {
            OnEffectEvent -= handler;
        }

        public YisoObjectID CreateEffect(IYisoEntity entity, IYisoEffect effect) {
            if (TryGetEffect(effect.SourceId, out var e)) {
                if (!effect.CanOverlap) {
                    e.Restart(entity);
                    RaiseEvent(new EffectRestartEventArgs(e));
                    return e.Id;
                }
            }

            var newId = YisoObjectID.Generate();
            var newEffect = new YisoEffect(newId, effect, bootstrap);
            effects[newId] = newEffect;

            newEffect.AddOnStartEffect(() => RaiseEvent(new EffectStartEventArgs(newEffect)));
            newEffect.AddOnCompleteEffect(() => {
                RaiseEvent(new EffectCompleteEventArgs(newEffect));
                effects.Remove(newId);
            });
            newEffect.Start(entity, false);

            if (effect.CoolDown > 0) {
                coolDownService.CreateCoolDown(effect.SourceId, effect.CoolDown);
            }
            
            return newId;
        }

        private bool TryGetEffect(int sourceId, out YisoEffect effect) {
            effect = effects.Values.FirstOrDefault(e => e.SourceId == sourceId);
            return effect != null;
        }

        public void OnDestroy() { }
        
        private void RaiseEvent(EffectEventArgs args) {
            OnEffectEvent?.Invoke(args);
        }
        
        public bool IsReady() => true;
    }
}