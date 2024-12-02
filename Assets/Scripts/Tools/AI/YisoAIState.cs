using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Tools.ReorderableList;
using Tools.ReorderableList.Attribute;
using UnityEngine;

namespace Tools.AI {
    [Serializable]
    public class AIActionList : ReorderableArray<YisoAIAction> {
    }

    [Serializable]
    public class AITransitionList : ReorderableArray<YisoAITransition> {
    }

    [Serializable]
    public class YisoAIState {
        public string stateName;

        [ReorderableAttribute(null, "Action", null)]
        public AIActionList actions;

        [ReorderableAttribute(null, "Transition", null)]
        public AITransitionList transitions;

        [ReadOnly] public float lastStateExecutionTime;

        protected YisoAIBrain brain;

        public virtual void SetBrain(YisoAIBrain newBrain) {
            brain = newBrain;
        }

        public virtual void EnterState() {
            lastStateExecutionTime = Time.time;

            // Actions (Decision)
            foreach (var action in actions) {
                action.OnEnterState();
            }

            // Transitions (Decision)
            foreach (var transition in transitions) {
                if (transition.decision != null) {
                    transition.decision.OnEnterState();
                }
            }
        }

        public virtual void ExitState() {
            // Actions (Decision)
            foreach (var action in actions) {
                action.OnExitState();
            }

            // Transitions (Decision)
            foreach (var transition in transitions) {
                if (transition.decision != null) {
                    transition.decision.OnExitState();
                }
            }
        }

        public virtual void PerformActions() {
            if (actions.Count == 0) return;
            foreach (var action in actions) {
                if (action != null) {
                    action.PerformAction();
                }
                else {
                    Debug.LogError("An action in " + brain.gameObject.name + " on state " + stateName + " is null.");
                }
            }
        }

        public virtual void EvaluateTransitions() {
            if (transitions.Count == 0) return;
            foreach (var transition in transitions.Where(transition => transition.decision != null)) {
                var nextState = transition.GetNextState();

                if (!string.IsNullOrEmpty(nextState)) {
                    brain.TransitionToState(nextState);
                    break;
                }
            }
        }
    }
}