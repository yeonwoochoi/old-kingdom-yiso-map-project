using System.Collections;
using Core.Domain.Types;
using DG.Tweening;
using UI.Popup.Base;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace UI.Popup.BountyClear {
    public class YisoPopupBountyClearContentUI : YisoPopupBaseContentUI {
        private const float to11Duration = 0.2f;
        private const float to1Duration = 0.1f;

        private RectTransform rectTransform;

        private UnityAction onComplete = null;

        public override void GetComponentOnAwake() {
            base.GetComponentOnAwake();
            rectTransform = (RectTransform) transform;
        }

        protected override void ClearPanel() {
            rectTransform.localScale = Vector3.zero;
        }

        protected override void HandleData(object data = null) {
            onComplete = (UnityAction) data;
            StartCoroutine(DOSuccess());
        }

        private IEnumerator DOSuccess() {
            yield return YieldInstructionCache.WaitForSeconds(0.2f);
            yield return rectTransform.DOScale(1.1f, to11Duration).WaitForCompletion();
            yield return rectTransform.DOScale(1f, to1Duration).WaitForCompletion();
            yield return YieldInstructionCache.WaitForSeconds(1f);
            yield return rectTransform.DOScale(1.1f, to1Duration).WaitForCompletion();
            yield return rectTransform.DOScale(Vector3.zero, to11Duration).WaitForCompletion();
            yield return YieldInstructionCache.WaitForSeconds(0.5f);
            onComplete?.Invoke();
        }

        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.BOUNTY_CLEAR;
    }
}