using System.Collections;
using System.Collections.Generic;
using Core.Domain.Locale;
using Cutscene.Scripts.Domain;
using Cutscene.Scripts.Domain.Cutscene;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Extensions;

namespace Cutscene.Scripts.Control.Cutscene.Trigger
{
    public class YisoCutsceneStageDirectionPopupTrigger: YisoBaseCutsceneTrigger
    {
        [SerializeField] private CanvasGroup cGroup;
        [SerializeField] private Text stageDirectionText;
        [SerializeField] private EventTrigger trigger;

        private List<Direction> directions;
        
        private YisoCutsceneController.ControlType controlType = YisoCutsceneController.controlType;
        private YisoLocale.Locale locale = YisoCutsceneController.locale;
        private int currentDirectionIndex = 0;
        private string currentSentence;
        private bool isStart = false;
        private Coroutine directionTriggerCoroutine;
        private Coroutine typingCoroutine;
        private Coroutine enterClickListenerCoroutine;
        
        private readonly float touchSkipDelayPercentage = 0.2f;
        private float TypingSpeed => locale == YisoLocale.Locale.KR ? 0.05f : 0.01f;

        public void InvokeDirectionTrigger(YisoCutsceneStageDirectionSO so)
        {
            Pause();
            currentDirectionIndex = 0;
            directions = so.CreateCutSceneDirections().directions;
            isStart = false;
            isTyping = false;
            
            trigger.triggers.Clear();
            if (controlType == YisoCutsceneController.ControlType.Space)
            {
                enterClickListenerCoroutine = StartCoroutine(CheckEnterClickEvent());
            }
            else if (controlType == YisoCutsceneController.ControlType.Touch)
            {
                // TODO (Stage Direction Trigger ): Touch Control 시 해당 event가 안 들어감
                var entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerClick
                };
                entry.callback.AddListener((eventData) => {OnClickDialogPanel();});
                trigger.triggers.Add(entry);
            }
            
            directionTriggerCoroutine = StartCoroutine(HandleDirectionEvent());
        }

        protected override void Init()
        {
            base.Init();
            stageDirectionText.text = "";
            cGroup.DOVisible(1f, 1f);
        }

        protected override void Resume()
        {
            trigger.triggers.Clear();
            if (controlType == YisoCutsceneController.ControlType.Space) StopCoroutine(enterClickListenerCoroutine);
            StopCoroutine(typingCoroutine);
            StopCoroutine(directionTriggerCoroutine);
            cGroup.DOVisible(0f, 1f);
            base.Resume();
        }

        private IEnumerator HandleDirectionEvent()
        {
            Init();
            for (var i = 0; i < directions.Count; i++)
            {
                if (!directions[i].pause)
                {
                    base.Resume();
                }
                else
                {
                    Pause();
                }
                
                currentSentence = directions[i].direction[locale];

                typingCoroutine = StartCoroutine(TypeSentence(currentSentence, stageDirectionText, TypingSpeed));
                if (i == 0)
                {
                    isStart = true;
                }

                // 일정 시간 지나면 자동 스킵
                if (controlType == YisoCutsceneController.ControlType.Auto)
                {
                    var readingTime = locale == YisoLocale.Locale.KR
                        ? directions[i].readingTime.kr
                        : directions[i].readingTime.en;
                    yield return new WaitForSeconds(float.Parse(readingTime));
                    OnClickDialogPanel();
                }

                yield return new WaitForSeconds(1f);

                while (i == currentDirectionIndex)
                {
                    yield return null;
                }
            }
        }

        private void OnClickDialogPanel()
        {
            if (isTyping)
            {
                var progress = (float) stageDirectionText.text.Length / (float) currentSentence.Length;
                if (progress >= touchSkipDelayPercentage)
                {
                    if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                    isTyping = false;
                    stageDirectionText.text = currentSentence;   
                }
            }
            else
            {
                if (currentSentence == stageDirectionText.text)
                {
                    if (directions.Count <= currentDirectionIndex + 1)
                    {
                        // Done
                        Resume();
                    }
                    else if (isStart)
                    {
                        currentDirectionIndex++;
                    }
                }
            }
        }

        private IEnumerator CheckEnterClickEvent()
        {
            while (!IsDone)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    OnClickDialogPanel();
                }
                yield return null;
            }
        }
    }
}