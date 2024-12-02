using System;
using Sirenix.OdinInspector;
using Tools.Feedback.Feedbacks;

namespace Tools.Feedback.Core {
    /// <summary>
    /// 피드백 시스템 내에서 어떤 타겟에 피드백을 적용할지를 결정하는 역할을 하며, 다양한 방법으로 타겟을 찾을 수 있는 기능을 제공
    /// </summary>
    [Serializable]
    public class YisoFeedbackTargetAcquisition {
        public enum Modes {
            None, // 아무것도 선택하지 않음
            Self, // YisoFeedbacks의 게임 오브젝트를 타겟으로 선택
            AnyChild, // YisoFeedbacks의 자식 오브젝트 중 임의의 오브젝트를 타겟으로 선택
            ChildAtIndex, // YisoFeedbacks의 자식 오브젝트 중 지정된 인덱스에 해당하는 자식을 타겟으로 선택
            Parent, // YisoFeedbacks의 부모 오브젝트를 타겟으로 선택
            FirstReferenceHolder, // 참조 홀더 목록에서 첫 번째 참조 홀더를 타겟으로 선택
            PreviousReferenceHolder, // 현재 피드백 이전에 발견된 참조 홀더를 타겟으로 선택
            ClosestReferenceHolder, // 현재 피드백에서 가장 가까운 참조 홀더를 타겟으로 선택
            NextReferenceHolder, // 현재 피드백 이후에 발견된 참조 홀더를 타겟으로 선택
            LastReferenceHolder // 참조 홀더 목록에서 마지막 참조 홀더를 타겟으로 선택
        }

        public Modes mode = Modes.None;
        [ShowIf("mode", Modes.ChildAtIndex)] public int childIndex = 0;
        private static YisoFeedbackReferenceHolder referenceHolder;
    }
}