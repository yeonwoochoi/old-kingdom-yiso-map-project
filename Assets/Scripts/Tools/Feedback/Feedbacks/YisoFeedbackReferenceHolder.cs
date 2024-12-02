using Tools.Feedback.Core;
using UnityEngine;

namespace Tools.Feedback.Feedbacks {
    public class YisoFeedbackReferenceHolder: YisoFeedback {
        
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return YisoFeedbackInspectorColors.FeedbacksColor; } }
        public override string RequiredTargetText => GameObjectReference != null ? GameObjectReference.name : "";  
#endif
        
        public GameObject GameObjectReference;

    }
}