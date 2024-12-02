using System;
using System.Collections;
using Core.Domain.Locale;
using Cutscene.Scripts.Domain;
using Cutscene.Scripts.Domain.Cutscene.Scenario;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Extensions;

namespace Cutscene.Scripts.Control.Cutscene.Trigger
{
    public class YisoCutsceneDialogTrigger: YisoBaseCutsceneTrigger
    {
        [SerializeField] private DialogCanvas[] cGroups;
        [SerializeField] private EventTrigger trigger;

        private YisoCutsceneController.ControlType controlType = YisoCutsceneController.controlType;
        private YisoLocale.Locale locale = YisoCutsceneController.locale;
        
        private YisoCutsceneDialog dialogs;

        private Coroutine dialogTriggerCoroutine;
        private Coroutine typingCoroutine;
        private Coroutine enterClickListenerCoroutine;
        
        private DialogCanvas currentCanvas;
        private string currentSentence;
        private int currentDialogIndex = 0;
        private bool isStart = false;
        private bool isDialogPanelSet = false;

        private readonly float touchSkipDelayPercentage = 0.2f;
        private readonly float panelFadeInOutDelay = 0.5f;
        private float TypingSpeed => locale == YisoLocale.Locale.KR ? 0.015f : 0.01f;

        [Serializable]
        private class DialogCanvas
        {
            public CanvasGroup cGroup;
            public CanvasGroup speechBalloonCGroup;
            public Image characterImage;
            public Text nameText;
            public Text dialogText;

            public void Init()
            {
                cGroup.Visible(false);
                speechBalloonCGroup.Visible(false);
                characterImage.sprite = null;
                dialogText.text = "";
                nameText.text = "";
            }
        }
        
        public void InvokeDialogTrigger(YisoCutsceneDialogSO so)
        {
            Pause();
            isStart = false;
            dialogs = so.CreateCutSceneDialog();
            currentDialogIndex = 0;
            isTyping = false;
            
            trigger.triggers.Clear();
            if (controlType == YisoCutsceneController.ControlType.Space)
            {
                enterClickListenerCoroutine = StartCoroutine(CheckEnterClickEvent());
            }
            else if (controlType == YisoCutsceneController.ControlType.Touch)
            {
                var entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerClick
                };
                entry.callback.AddListener((eventData) => {OnClickDialogPanel();});
                trigger.triggers.Add(entry);
            }

            dialogTriggerCoroutine = StartCoroutine(HandleDialogEvent());
        }

        protected override void Resume()
        {
            trigger.triggers.Clear();
            StopCoroutine(dialogTriggerCoroutine);
            if (controlType == YisoCutsceneController.ControlType.Space) StopCoroutine(enterClickListenerCoroutine);
            foreach (var cGroup in cGroups)
            {
                cGroup.cGroup.DOVisible(0f, panelFadeInOutDelay);
                cGroup.speechBalloonCGroup.DOVisible(0f, panelFadeInOutDelay);
            }
            base.Resume();
        }

        protected override void Init()
        {
            base.Init();
            foreach (var cGroup in cGroups)
            {
                cGroup.Init();
            }
        }
        
        private IEnumerator HandleDialogEvent()
        {
            Init();
            for (var i = 0; i < dialogs.messages.Count; i++)
            {
                isDialogPanelSet = false;
                if (!dialogs.messages[i].pause)
                {
                    base.Resume();
                }
                else
                {
                    Pause();
                }
                var message = dialogs.messages[i];
                
                currentSentence = message.message[locale];
                currentCanvas = cGroups[(int) message.uiPosition];

                currentCanvas.characterImage.sprite = message.icon;
                currentCanvas.nameText.text = message.name[locale];
                currentCanvas.dialogText.text = "";
                currentCanvas.cGroup.DOVisible(1f, panelFadeInOutDelay);

                CanvasGroup oppositeSpeechBalloon;
                switch (message.uiPosition)
                {
                    case UIPosition.TopLeft:
                        oppositeSpeechBalloon = cGroups[(int)UIPosition.TopRight].speechBalloonCGroup;
                        break;
                    case UIPosition.BottomRight:
                        oppositeSpeechBalloon = cGroups[(int)UIPosition.BottomLeft].speechBalloonCGroup;
                        break;
                    case UIPosition.TopRight:
                        oppositeSpeechBalloon = cGroups[(int)UIPosition.TopLeft].speechBalloonCGroup;
                        break;
                    case UIPosition.BottomLeft:
                        oppositeSpeechBalloon = cGroups[(int)UIPosition.BottomRight].speechBalloonCGroup;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                if (oppositeSpeechBalloon != null && oppositeSpeechBalloon.IsVisible())
                {
                    oppositeSpeechBalloon.DOVisible(0f, 0.1f);
                }
                
                yield return currentCanvas.speechBalloonCGroup.DOVisible(1f, panelFadeInOutDelay).OnComplete(() => isDialogPanelSet = true).WaitForCompletion();

                typingCoroutine = StartCoroutine(TypeSentence(currentSentence, currentCanvas.dialogText, TypingSpeed));
                if (i == 0) isStart = true;

                // 동영상 제작시에만 해당 (일정 시간이 지나면 자동스킵)
                if (controlType == YisoCutsceneController.ControlType.Auto)
                {
                    var readingTime = locale == YisoLocale.Locale.KR ? message.readingTime.kr : message.readingTime.en;
                    yield return new WaitForSeconds(float.Parse(readingTime));
                    OnClickDialogPanel();
                }

                while (i == currentDialogIndex)
                {
                    yield return null;
                }
            }
        }

        private void OnClickDialogPanel()
        {
            if (!isStart) return;
            if (isTyping)
            {
                var progress = (float) currentCanvas.dialogText.text.Length / (float) currentSentence.Length;
                if (progress >= touchSkipDelayPercentage)
                {
                    if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                    isTyping = false;
                    currentCanvas.dialogText.text = currentSentence;   
                }
            }
            else if (isDialogPanelSet)
            {
                if (currentSentence == currentCanvas.dialogText.text)
                {
                    if (dialogs.messages.Count <= currentDialogIndex + 1)
                    {
                        // Done
                        Resume();
                    }
                    else
                    {
                        currentDialogIndex++;
                    }
                }
            }
        }

        private IEnumerator CheckEnterClickEvent()
        {
            while (!IsDone)
            {
                if (Input.GetKeyDown(KeyCode.Space) && isDialogPanelSet)
                {
                    OnClickDialogPanel();
                }
                yield return null;
            }
        }
    }
}