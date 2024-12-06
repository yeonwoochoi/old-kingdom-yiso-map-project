using Core.Behaviour;
using Tools.Event;

namespace Controller.Stage {
    public interface IYisoStageAction {
        public void PerformAction();
    }

    public abstract class YisoStageAction : RunIBehaviour, IYisoStageAction {
        public YisoInGameEventTypes eventTypes;
        public int priority;
        
        protected bool initialized = false;
        
        public abstract void PerformAction();
        public virtual void Initialization() { }
    }
}