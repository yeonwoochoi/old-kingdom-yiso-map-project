using System;
using System.Collections.Generic;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace Tools.AI {
    /// <summary>
    /// Decision을 통해 True이면 trueState로, False면 falseState로
    /// </summary>
    [Serializable]
    public class YisoAITransition {
        public YisoAIDecision decision;
        [HorizontalGroup("TrueStateGroup")] public bool trueRandomStates = false;

        [ShowIf("@!trueRandomStates"), HorizontalGroup("TrueStateGroup"), HideLabel]
        public string trueState;

        [ShowIf("trueRandomStates"), HorizontalGroup("TrueStateGroup"), HideLabel]
        public List<string> trueStates;

        [HorizontalGroup("FalseStateGroup")] public bool falseRandomStates = false;

        [ShowIf("@!falseRandomStates"), HorizontalGroup("FalseStateGroup"), HideLabel]
        public string falseState;

        [ShowIf("falseRandomStates"), HorizontalGroup("FalseStateGroup"), HideLabel]
        public List<string> falseStates;
        private YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoAITransition>();

        public string GetNextState() {
            if (decision.Decide()) {
                if (trueRandomStates && trueStates.Count > 0) {
                    return RandomlySelectState(trueStates);
                }

                return trueState;
            }

            if (falseRandomStates && falseStates.Count > 0) {
                return RandomlySelectState(falseStates);
            }

            return falseState;
        }

        private string RandomlySelectState(IReadOnlyList<string> states) {
            if (states == null || states.Count == 0) {
                LogService.Error("[YisoAITransition] No states available for random selection.");
                return null;
            }

            return states[Random.Range(0, states.Count)];
        }
    }
}