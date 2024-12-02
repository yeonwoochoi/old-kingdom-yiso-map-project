using Core.Behaviour;

namespace Controller.Stage {
    public interface IYisoStageAction {
        public void PerformAction();
    }

    public abstract class YisoStageAction : RunIBehaviour, IYisoStageAction {
        public int priority;
        public abstract void PerformAction();
        protected bool initialized = false;

        public virtual void Initialization() {
        }
    }
}