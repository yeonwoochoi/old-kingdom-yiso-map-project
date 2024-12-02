using System;
using System.Collections;
using Core.Domain.Locale;
using Cutscene.Scripts.Domain.Cutscene.Scenario;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Extensions;

namespace Cutscene.Scripts.Control.Cutscene.Trigger
{
    public class YisoCutsceneCartoonDialogTrigger : YisoBaseCutsceneTrigger
    {
        [SerializeField] private CanvasGroup cGroup;
        [SerializeField] private CartoonDialogCanvas leftCanvasGroup;
        [SerializeField] private CartoonDialogCanvas rightCanvasGroup;
        [SerializeField] private Button skipButton;
        [SerializeField] private EventTrigger trigger;
        [SerializeField] private bool showSkipButton = false;

        private YisoCutsceneController.ControlType controlType = YisoCutsceneController.controlType;
        private YisoLocale.Locale locale = YisoCutsceneController.locale;
        
        private YisoCutsceneDialog dialogs;
        private CartoonDialogCanvas currentCanvas;
        private string currentSentence;
        private int currentDialogIndex = 0;
        private bool isStart = false;
        private bool isDialogPanelSet = false;
        
        private Coroutine dialogTriggerCoroutine;
        private Coroutine showSkipButtonCoroutine;
        private Coroutine typingCoroutine;
        private Coroutine enterClickListenerCoroutine;

        private readonly float touchSkipDelayPercentage = 0.2f;
        private readonly float skipButtonPresentDelay = 2f;
        private readonly float panelFadeInOutDelay = 0.5f;
        
        private float TypingSpeed => locale == YisoLocale.Locale.KR ? 0.015f : 0.01f;

        [Serializable]
        public class CartoonDialogCanvas
        {
            public CanvasGroup cGroup;
            public Image iconImage;
            public Text characterNameText;
            public Text dialogText;

            public void Init()
            {
                cGroup.Visible(false);
                iconImage.sprite = null;
                characterNameText.text = "";
                dialogText.text = "";
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
            
            dialogTriggerCoroutine = StartCoroutine(InvokeDialogCoroutine());
        }

        private IEnumerator InvokeDialogCoroutine()
        {
            Init();
            cGroup.DOVisible(1f, panelFadeInOutDelay);
            for (var i = 0; i < dialogs.messages.Count; i++)
            {
                Debug.Log($"{i} : Start");
                isDialogPanelSet = false;
                
                if (!dialogs.messages[i].pause) base.Resume();
                else Pause();
                
                var message = dialogs.messages[i];

                currentSentence = message.message[locale];
                currentCanvas = GetDialogCanvas(message.uiPosition);

                currentCanvas.iconImage.sprite = message.icon;
                currentCanvas.characterNameText.text = message.name[locale];
                currentCanvas.dialogText.text = "";

                var oppositeCanvasGroup = GetDialogCanvas(message.uiPosition, true).cGroup;
                if (oppositeCanvasGroup.IsVisible())
                {
                    oppositeCanvasGroup.DOVisible(0f, 0.1f);
                    Debug.Log($"{i} : Opposite Done");
                }

                if (!currentCanvas.cGroup.IsVisible())
                {
                    yield return currentCanvas.cGroup.DOVisible(1f, panelFadeInOutDelay).OnComplete(() => isDialogPanelSet = true).WaitForCompletion();
                    Debug.Log($"{i} : Current Done");
                }
                else
                {
                    isDialogPanelSet = true;
                }

                Debug.Log($"{i} : Type Start");
                typingCoroutine = StartCoroutine(TypeSentence(currentSentence, currentCanvas.dialogText, TypingSpeed));
                if (i == 0) isStart = true;
                
                if (controlType == YisoCutsceneController.ControlType.Auto)
                {
                    var readingTime = locale == YisoLocale.Locale.KR ? message.readingTime.kr : message.readingTime.en;
                    Debug.Log($"{i} : Wait Start");
                    yield return new WaitForSeconds(float.Parse(readingTime));
                    Debug.Log($"{i} : Wait End");
                    OnClickDialogPanel();
                }
                
                while (i == currentDialogIndex)
                {
                    yield return null;
                }
                Debug.Log($"{i} : End");
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
            
            leftCanvasGroup.cGroup.DOVisible(0f, panelFadeInOutDelay);
            rightCanvasGroup.cGroup.DOVisible(0f, panelFadeInOutDelay);
            cGroup.DOVisible(0f, panelFadeInOutDelay);
            
            base.Resume();
        }

        protected override void Init()
        {
            base.Init();
            cGroup.Visible(false);
            leftCanvasGroup.Init();
            rightCanvasGroup.Init();
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

        private CartoonDialogCanvas GetDialogCanvas(UIPosition uiPosition, bool opposite = false)
        {
            if (opposite)
            {
                return uiPosition switch
                {
                    UIPosition.TopLeft => rightCanvasGroup,
                    UIPosition.BottomRight => leftCanvasGroup,
                    UIPosition.TopRight => leftCanvasGroup,
                    UIPosition.BottomLeft => rightCanvasGroup,
                    _ => throw new ArgumentOutOfRangeException(nameof(uiPosition), uiPosition, null)
                };
            }
            return uiPosition switch
            {
                UIPosition.TopLeft => leftCanvasGroup,
                UIPosition.BottomRight => rightCanvasGroup,
                UIPosition.TopRight => rightCanvasGroup,
                UIPosition.BottomLeft => leftCanvasGroup,
                _ => throw new ArgumentOutOfRangeException(nameof(uiPosition), uiPosition, null)
            };
        }
    }
}