using Core.Behaviour;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Feedback.Core {
    public class YisoFeedBacks : RunIBehaviour {
        
        public bool forceTimescaleMode = false;
        [ShowIf("forceTimescaleMode")] public TimescaleModes forcedTimescaleMode = TimescaleModes.Unscaled;
        
        public virtual bool SkippingToTheEnd { get; protected set; }
        
        // TODO
        public virtual void Initialization(GameObject owner) {
        }

        public virtual void PlayFeedbacks() {
        }

        public virtual void PlayFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false) {
        }

        public virtual void StopFeedbacks() {
        }

        public virtual void StopFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f,
            bool stopAllFeedbacks = true) {
        }

        public enum AccessMethods { First, Previous, Closest, Next, Last }
        
        /// <summary>
        /// Returns the first feedback found in this player's list based on the chosen method and type
        /// First : first feedback of the matching type in the list, from top to bottom
        /// Previous : first feedback of the matching type located before (so above) the feedback at the reference index
        /// Closest : first feedback of the matching type located before or after the feedback at the reference index
        /// Next : first feedback of the matching type located after (so below) the feedback at the reference index
        /// First : last feedback of the matching type in the list, from top to bottom
        /// </summary>
        /// <param name="method"></param>
        /// <param name="referenceIndex"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T GetFeedbackOfType<T>(AccessMethods method, int referenceIndex) where T : YisoFeedback {
            return null;
        }

        public virtual float ComputeRangeIntensityMultiplier(Vector3 position) {
            return 0f;
        }
    }
}