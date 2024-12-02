using System;
using System.Collections;
using System.Collections.Generic;
using Core.Domain.Locale;
using Cutscene.Scripts.Domain;
using Cutscene.Scripts.Domain.Cutscene;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace Cutscene.Scripts.Control.Cutscene.Trigger
{
    public class YisoCutsceneLoadingCommentTrigger: YisoBaseCutsceneTrigger
    {
        [SerializeField] private CanvasGroup cGroup;
        [SerializeField] private Text text;
        
        private Coroutine loadingCommentTriggerCoroutine;
        private Coroutine typingCoroutine;
        private List<LoadingComment> loadingComments;

        private float TypingSpeed => locale == YisoLocale.Locale.KR ? 0.05f : 0.025f;
        private YisoLocale.Locale locale = YisoCutsceneController.locale;

        public void InvokeLoadingComments(YisoCutsceneLoadingCommentsSO so)
        {
            Pause();
            loadingComments = so.CreateYisoCutsceneLoadingComments().comments;

            loadingCommentTriggerCoroutine = StartCoroutine(HandleLoadingComment());
        }

        protected override void Init()
        {
            base.Init();
            text.text = "";
        }

        protected override void Resume()
        {
            StopCoroutine(typingCoroutine);
            StopCoroutine(loadingCommentTriggerCoroutine);
            base.Resume();
        }

        private IEnumerator HandleLoadingComment()
        {
            Init();
            yield return cGroup.DOVisible(1f, 1f).WaitForCompletion();
            for (var i = 0; i < loadingComments.Count; i++)
            {
                typingCoroutine = StartCoroutine(TypeSentence(loadingComments[i].comment[locale], text, TypingSpeed));

                var readingTime = locale == YisoLocale.Locale.KR
                    ? loadingComments[i].readingTime.kr
                    : loadingComments[i].readingTime.en;
                yield return new WaitForSeconds(float.Parse(readingTime));
            }
            yield return cGroup.DOVisible(0f, 1f).WaitForCompletion();
            Resume();
        }
    }
}