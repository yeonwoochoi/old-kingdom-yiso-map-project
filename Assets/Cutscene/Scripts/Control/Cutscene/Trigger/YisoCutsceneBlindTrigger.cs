using System.Collections;
using DG.Tweening;
using UnityEngine;
using Utils.Extensions;

namespace Cutscene.Scripts.Control.Cutscene.Trigger
{
    public class YisoCutsceneBlindTrigger: YisoBaseCutsceneTrigger
    {
        [SerializeField] private CanvasGroup cGroup;

        public void InvokeFadeOutAndIn()
        {
            Pause();
            StartCoroutine(FadeOutAndIn());
        }

        public void InvokeFadeIn()
        {
            Pause();
            StartCoroutine(FadeIn());
        }

        public void InvokeFadeOut()
        {
            Pause();
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOutAndIn()
        {
            yield return cGroup.DOVisible(1f, 1f).WaitForCompletion();
            yield return new WaitForSeconds(0.5f);
            yield return cGroup.DOVisible(0f, 1f).WaitForCompletion();
            Resume();
        }

        private IEnumerator FadeOut()
        {
            yield return cGroup.DOVisible(1f, 1f).WaitForCompletion();
            Resume();
        }

        private IEnumerator FadeIn()
        {
            yield return cGroup.DOVisible(0f, 1f).WaitForCompletion();
            Resume();
        }
    }
}