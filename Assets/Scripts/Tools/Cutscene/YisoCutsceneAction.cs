using System.Collections.Generic;
using Core.Behaviour;
using UnityEngine;

namespace Tools.Cutscene {
    public interface IYisoCutsceneAction {
        public void PerformAction();
    }

    public abstract class YisoCutsceneAction : RunIBehaviour, IYisoCutsceneAction {
        public int priority;

        public abstract void PerformAction();

        protected bool initialized = false;

        public virtual void Initialization() {
        }
    }
}