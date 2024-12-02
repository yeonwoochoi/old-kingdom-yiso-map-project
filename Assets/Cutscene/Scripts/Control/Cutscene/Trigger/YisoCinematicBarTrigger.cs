using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Cutscene.Scripts.Control.Cutscene.Trigger
{
    public class YisoCinematicBarTrigger: YisoBaseCutsceneTrigger
    {
        [SerializeField] private RectTransform topBar;
        [SerializeField] private RectTransform bottomBar;

        private readonly float duration = 0.8f;

        public void InvokeShowCinematicBar()
        {
            if (!gameObject.activeInHierarchy) return;
            Pause();
            StartCoroutine(ShowCinematicBar(true));
        }

        public void InvokeHideCinematicBar()
        {
            if (!gameObject.activeInHierarchy) return;
            Pause();
            StartCoroutine(ShowCinematicBar(false));
        }

        private IEnumerator ShowCinematicBar(bool isShow)
        {
            var topBarMoved = false;
            var bottomBarMoved = false;

            DORectMoveY(topBar, isShow ? 0f : topBar.rect.height, duration).OnComplete(() => topBarMoved = true);
            DORectMoveY(bottomBar, isShow ? 0f : -bottomBar.rect.height, duration).OnComplete(() => bottomBarMoved = true);

            while (!topBarMoved || !bottomBarMoved)
            {
                yield return null;
            }

            Resume();
        }

        private TweenerCore<Vector2, Vector2, VectorOptions> DORectMoveY(RectTransform target, float endValue,
            float duration)
        {
            return target.DOAnchorPosY(endValue, duration).SetEase(Ease.Linear).SetUpdate(true);
        }
    }
}