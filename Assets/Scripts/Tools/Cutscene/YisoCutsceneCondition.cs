using Core.Behaviour;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Cutscene {
    public interface IYisoCutsceneCondition {
        public bool CanPlay();
    }

    public abstract class YisoCutsceneCondition : RunIBehaviour, IYisoCutsceneCondition {
        public int priority;

        public abstract bool CanPlay();

        protected bool initialized = false;

        public virtual void Initialization() {
        }

        [Button]
        public void CheckCanPlay() {
            Debug.Log($"{GetType().FullName} : {CanPlay()}");
        }
    }
}