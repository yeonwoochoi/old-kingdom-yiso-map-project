using UnityEngine;

namespace Tools.AI.Actions {
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionDoNothing")]
    public class YisoAIActionDoNothing : YisoAIAction {
        /// <summary>
        /// On PerformAction we do nothing
        /// </summary>
        public override void PerformAction() {
        }
    }
}