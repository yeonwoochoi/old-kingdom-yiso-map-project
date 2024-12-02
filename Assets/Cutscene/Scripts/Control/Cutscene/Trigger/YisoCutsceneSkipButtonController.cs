using System.Collections;
using Core.Behaviour;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Utils.Extensions;

namespace Cutscene.Scripts.Control.Cutscene.Trigger
{
    public class YisoCutsceneSkipButtonController: RunIBehaviour
    {
        public PlayableDirector director;
        public bool showSkipButton = true;
        public float skipButtonPresentDelay = 2f;

        protected Button skipButton;
        protected bool isClicked = false;
        
        protected override void Start()
        {
            skipButton = GetComponent<Button>();
            if (showSkipButton)
            {
                StartCoroutine(ShowSkipButton());
                skipButton.onClick.AddListener(() => director.Stop());
            }
            else
            {
                skipButton.GetComponent<CanvasGroup>().Visible(false);
            }
            isClicked = false;
        }

        public void Skip()
        {
            if (isClicked) return;
            isClicked = true;
            director.time = director.duration;
            director.Evaluate();
            director.Stop();
        }

        private IEnumerator ShowSkipButton()
        {
            yield return new WaitForSeconds(skipButtonPresentDelay);
            skipButton.GetComponent<CanvasGroup>().DOVisible(1f, 0.5f);
        }
    }
}