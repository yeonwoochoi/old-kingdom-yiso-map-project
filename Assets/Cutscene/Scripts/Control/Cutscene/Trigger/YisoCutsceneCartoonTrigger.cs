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
    public class YisoCutsceneCartoonTrigger: YisoBaseCutsceneTrigger
    {
        [SerializeField] private CanvasGroup cGroup;
        [SerializeField] private DialogCanvas dialogCanvas;
        [SerializeField] private Image[] cartoonImages;
        [SerializeField] private Button skipButton;
        [SerializeField] private EventTrigger trigger;
        [SerializeField] private bool showSkipButton = true;
        
        private YisoCutsceneController.ControlType controlType = YisoCutsceneController.controlType;
        private YisoLocale.Locale locale = YisoCutsceneController.locale;
        
        private YisoCutsceneCartoon cartoons;
        
        private Coroutine dialogTriggerCoroutine;
        private Coroutine showSkipButtonCoroutine;
        private Coroutine typingCoroutine;
        private Coroutine enterClickListenerCoroutine;
        
        private string currentSentence;
        private int currentDialogIndex = 0;
        private bool isStart = false;
        private bool isDialogPanelSet = false;
        
        private readonly float touchSkipDelayPercentage = 0.2f;
        private readonly float skipButtonPresentDelay = 2f;
        private readonly float panelFadeInOutDelay = 0.5f;
        
        private float TypingSpeed => locale == YisoLocale.Locale.KR ? 0.015f : 0.01f;
        
        [Serializable]
        private class DialogCanvas
        {
            public CanvasGroup cGroup;
            public Text text;

            public void Init()
            {
                cGroup.Visible(false);
                text.text = "";
            }
        }
    
        public void InvokeDialogTrigger(YisoCutsceneCartoonSO so) {
            Init();
            
            cartoons = so.CreateCartoon();

            cGroup.DOVisible(1f, panelFadeInOutDelay);
            
            switch (controlType)
            {
                case YisoCutsceneController.ControlType.Space:
                    enterClickListenerCoroutine = StartCoroutine(CheckEnterClickEvent());
                    break;
                case YisoCutsceneController.ControlType.Touch:
                {
                    var entry = new EventTrigger.Entry
                    {
                        eventID = EventTriggerType.PointerClick
                    };
                    entry.callback.AddListener((eventData) => {OnClickDialogPanel();});
                    trigger.triggers.Add(entry);
                    break;
                }
                case YisoCutsceneController.ControlType.Auto:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (showSkipButton)
            {
                skipButton.onClick.AddListener(() =>
                {
                    if (dialogTriggerCoroutine != null)
                    {
                        StopCoroutine(dialogTriggerCoroutine);
                    }
                    Resume();
                });
                showSkipButtonCoroutine = StartCoroutine(ShowSkipButton());
            }
            else
            {
                skipButton.GetComponent<CanvasGroup>().Visible(false);
            }
            
            dialogTriggerCoroutine = StartCoroutine(ShowDialogs());
        }

        protected override void Init() {
            base.Init();
            
            Pause();
            
            isStart = false;
            isTyping = false;
            
            currentDialogIndex = 0;
            
            trigger.triggers.Clear();
            
            dialogCanvas.Init();
            foreach (var image in cartoonImages)
            {
                image.transform.parent.GetComponent<CanvasGroup>().Visible(0f);
                image.sprite = null;
            }
        }
        
        protected override void Resume()
        {
            trigger.triggers.Clear();
            var skipButtonCanvasGroup = skipButton.GetComponent<CanvasGroup>();
            StopCoroutine(dialogTriggerCoroutine);
            if (showSkipButton)
            {
                StopCoroutine(showSkipButtonCoroutine);
                skipButtonCanvasGroup.DOVisible(0f, panelFadeInOutDelay);
            }
            if (controlType == YisoCutsceneController.ControlType.Space) StopCoroutine(enterClickListenerCoroutine);
            cGroup.DOVisible(0f, panelFadeInOutDelay);
            base.Resume();
        }
        
        private IEnumerator ShowDialogs()
        {
            for (var i = 0; i < cartoons.cartoons.Count; i++)
            {
                isDialogPanelSet = false;
                var imageIndex = i % 4;
                var cartoon = cartoons.cartoons[i];
                
                currentSentence = cartoon.direction[locale];

                dialogCanvas.text.text = "";
                cartoonImages[imageIndex].sprite = cartoon.image;

                yield return dialogCanvas.cGroup.DOVisible(1f, panelFadeInOutDelay).OnComplete(() => isDialogPanelSet = true).WaitForCompletion();

                typingCoroutine = StartCoroutine(TypeSentence(currentSentence, dialogCanvas.text, TypingSpeed));

                cartoonImages[imageIndex].transform.parent.GetComponent<CanvasGroup>().DOVisible(1f, panelFadeInOutDelay);
                
                if (i == 0) isStart = true;

                // 동영상 제작시에만 해당 (일정 시간이 지나면 자동스킵)
                if (controlType == YisoCutsceneController.ControlType.Auto)
                {
                    var readingTime = locale == YisoLocale.Locale.KR ? cartoon.readingTime.kr : cartoon.readingTime.en;
                    yield return new WaitForSeconds(float.Parse(readingTime));
                    OnClickDialogPanel();
                }

                while (i == currentDialogIndex)
                {
                    yield return null;
                }

                if (i < cartoons.cartoons.Count - 1)
                {
                    cartoonImages[imageIndex].transform.parent.GetComponent<CanvasGroup>().DOVisible(0.4f, panelFadeInOutDelay);
                }
            }
        }

        private void OnClickDialogPanel()
        {
            if (!isStart) return;
            if (isTyping)
            {
                var progress = (float) dialogCanvas.text.text.Length / (float) currentSentence.Length;
                if (progress >= touchSkipDelayPercentage)
                {
                    if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                    isTyping = false;
                    dialogCanvas.text.text = currentSentence;   
                }
            }
            else if (isDialogPanelSet)
            {
                if (currentSentence == dialogCanvas.text.text)
                {
                    if (cartoons.cartoons.Count <= currentDialogIndex + 1)
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

        private IEnumerator ShowSkipButton()
        {
            yield return new WaitForSeconds(skipButtonPresentDelay);
            skipButton.GetComponent<CanvasGroup>().DOVisible(1f, panelFadeInOutDelay);
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