using System;
using Core.Behaviour;
using UnityEngine;

namespace Tools.AI {
    /// <summary>
    /// Actions are behaviours and describe what your character is doing.
    /// Examples include patrolling, shooting, jumping, etc. 
    /// </summary>
    public abstract class YisoAIAction : RunIBehaviour {
        public abstract void PerformAction();

        public enum InitializationModes {
            EveryTime,
            OnlyOnce
        }

        public InitializationModes initializationMode;
        public string label;

        protected bool initialized = false;
        protected YisoAIBrain brain;

        public bool ActionInProgress { get; set; }

        public virtual bool ShouldInitialize {
            get {
                return initializationMode switch {
                    InitializationModes.EveryTime => true,
                    InitializationModes.OnlyOnce => initialized == false,
                    _ => true
                };
            }
        }

        protected override void Awake() {
            brain = gameObject.GetComponentInParent<YisoAIBrain>();
        }

        public virtual void Initialization() {
            initialized = true;
        }

        public virtual void OnEnterState() {
            ActionInProgress = true;
        }

        public virtual void OnExitState() {
            ActionInProgress = false;
        }
    }
}