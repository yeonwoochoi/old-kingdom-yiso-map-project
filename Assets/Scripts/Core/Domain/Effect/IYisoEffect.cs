using Core.Domain.Actor;
using Core.Domain.Entity;
using Core.Domain.Locale;
using UnityEngine;

namespace Core.Domain.Effect {
    public interface IYisoEffect {
        int SourceId { get; }
        int Duration { get; }
        void OnStart(IYisoEntity entity);
        void OnProgress(IYisoEntity entity, float progress);
        void OnComplete(IYisoEntity entity);
        bool CanOverlap { get; }
        bool IsOverTime { get; }
        double TotalValue { get; }
        void OnOverTimeProgress(IYisoEntity entity, double valuePerSeconds);
        int CoolDown { get; }
        string GetDescription(YisoLocale.Locale locale);
        string GetName(YisoLocale.Locale locale);
        Sprite Icon { get; }
    }

    public interface IYisoBuff : IYisoEffect {
        
    }

    public interface IYisoDeBuff : IYisoEffect {
        
    }
}