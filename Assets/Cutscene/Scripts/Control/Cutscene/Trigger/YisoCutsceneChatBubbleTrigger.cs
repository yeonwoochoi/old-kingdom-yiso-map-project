using System.Collections;
using Core.Domain.Locale;
using Cutscene.Scripts.Domain;
using Cutscene.Scripts.Domain.Cutscene.Scenario;
using TMPro;
using UnityEngine;

namespace Cutscene.Scripts.Control.Cutscene.Trigger
{
    public class YisoCutsceneChatBubbleTrigger: YisoBaseCutsceneTrigger
    {
        [SerializeField] private Transform parentTransform;
        [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
        [SerializeField] private TextMeshPro textMeshPro;
        [SerializeField] private bool stopTimeLine = true;

        private float TypingSpeed => locale == YisoLocale.Locale.KR ? 0.05f : 0.025f;
        private YisoLocale.Locale locale = YisoCutsceneController.locale;
        
        private readonly float lerpTime = 0.5f;
        
        private Coroutine typeCoroutine;

        public void InvokeChatBubble(YisoCutsceneChatBubbleSO so)
        {
            Init();
            if (stopTimeLine) Pause();
            var msg = YisoCutsceneController.locale == YisoLocale.Locale.KR
                ? so.comment.kr
                : so.comment.en;
            
            // Set Background Size
            textMeshPro.SetText(msg);
            textMeshPro.ForceMeshUpdate();
            var textSize = textMeshPro.GetRenderedValues(false);
            var padding = new Vector2(1f, 0.75f);
            backgroundSpriteRenderer.size = textSize + padding;
            textMeshPro.SetText("");
            textMeshPro.ForceMeshUpdate();

            // Set offset
            var offset = new Vector3(-0.83f, -0.1f);
            var bgSize = backgroundSpriteRenderer.size;
            parentTransform.localPosition = new Vector3(-bgSize.x / 2f + 0.83f, bgSize.y / 2f + 1.3f);
            backgroundSpriteRenderer.transform.localPosition =
                new Vector3(backgroundSpriteRenderer.size.x / 2f, 0f) + offset;
            
            // Show Bubble
            typeCoroutine = StartCoroutine(TypeSentence(msg));
            if (stopTimeLine) Resume();
        }

        protected override void Init()
        {
            base.Init();
            if (typeCoroutine != null) StopCoroutine(typeCoroutine);

            backgroundSpriteRenderer.color = new Color(1, 1, 1, 0);
            textMeshPro.color = new Color(0, 0, 0, 0);
            textMeshPro.SetText("");
            textMeshPro.ForceMeshUpdate();
        }

        private IEnumerator TypeSentence(string msg)
        {
            yield return StartCoroutine(ShowSpeechBalloon());
            foreach (var letter in msg.ToCharArray())
            {
                textMeshPro.text += letter;
                yield return new WaitForSeconds(TypingSpeed);
            }

            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(HideSpeechBalloon());
        }

        private IEnumerator ShowSpeechBalloon()
        {
            var smoothness = 0.01f;
            var progress = 0f;
            var increment = smoothness/lerpTime;
            while (progress < 1)
            {
                backgroundSpriteRenderer.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), progress);
                textMeshPro.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), progress);
                progress += increment;
                yield return new WaitForSeconds(smoothness);
            }
        }

        private IEnumerator HideSpeechBalloon()
        {
            var smoothness = 0.01f;
            var progress = 0f;
            var increment = smoothness/lerpTime;
            while (progress < 1)
            {
                backgroundSpriteRenderer.color = Color.Lerp(new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), progress);
                textMeshPro.color = Color.Lerp(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), progress);
                progress += increment;
                yield return new WaitForSeconds(smoothness);
            }
        }
    }
}