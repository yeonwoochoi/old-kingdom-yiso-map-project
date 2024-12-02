using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Test.UI.Bounty {
    public class TestSuccessBountyUI : MonoBehaviour {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private float to11Duration = 0.2f;
        [SerializeField] private float to1Duration = 0.1f;

        [Button]
        public void OnSuccess() {
            StartCoroutine(DOSuccess());
        }

        [Button]
        public void ResetRect() {
            rectTransform.localScale= Vector3.zero;
        }

        private IEnumerator DOSuccess() {
            yield return rectTransform.DOScale(1.1f, to11Duration).WaitForCompletion();
            yield return rectTransform.DOScale(1f, to1Duration).WaitForCompletion();
            yield return YieldInstructionCache.WaitForSeconds(1f);
            yield return rectTransform.DOScale(1.1f, to1Duration).WaitForCompletion();
            yield return rectTransform.DOScale(Vector3.zero, to11Duration).WaitForCompletion();
            yield return YieldInstructionCache.WaitForSeconds(0.5f);
            Debug.Log("Done");
        }
    }
}