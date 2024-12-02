using System;
using Core.Behaviour;
using Unity.VisualScripting;
using UnityEngine;

namespace Tools.AI {
    public abstract class YisoAIDecision : RunIBehaviour {
        /// <summary>
        /// Decide will be performed every frame while the Brain is in a state this Decision is in.
        /// Should return true or false, which will then determine the transition's outcome.
        /// </summary>
        /// <returns></returns>
        public abstract bool Decide();

        public string label;
        protected YisoAIBrain brain;
        public bool DecisionInProgress { get; set; }

        protected override void Awake() {
            brain = gameObject.GetComponentInParent<YisoAIBrain>();
        }

        /// <summary>
        /// Start때 호출됨
        /// </summary>
        public virtual void Initialization() {
        }

        /// <summary>
        /// Called when the Brain enters a State this Decision is in
        /// </summary>
        public virtual void OnEnterState() {
            DecisionInProgress = true;
        }

        /// <summary>
        /// Called when the Brain exits a State this Decision is in
        /// </summary>
        public virtual void OnExitState() {
            DecisionInProgress = false;
        }
    }
}