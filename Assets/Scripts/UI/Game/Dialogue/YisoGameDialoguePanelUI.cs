using System;
using System.Collections.Generic;
using Core.Behaviour;
using DG.Tweening;
using MEC;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Game.Dialogue {
    public class YisoGameDialoguePanelUI : RunIBehaviour {
        [SerializeField, Title("Content")] private TextMeshProUGUI contentText;
        [SerializeField, Title("Speaker Tag")] private CanvasGroup speakerTagCanvas;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField, Title("Indicator")] private YisoGameDialogueNextIndicatorUI indicatorUI;
        [SerializeField, Title("Skip")] private Image touchImage;
        
        private CanvasGroup canvasGroup;

        private bool speeching = false;
        private bool skipped = false;
        private bool nextDialogue = false;

        protected override void Start() {
            canvasGroup = GetComponent<CanvasGroup>();
            touchImage.OnPointerClickAsObservable()
                .Subscribe(OnTouchPanel)
                .AddTo(this);
        }

        public void StartDialogue(string nickname, string[] contents, Action onComplete = null) {
            canvasGroup.DOVisible(1f, .25f).OnComplete(() => {
                speakerTagCanvas.Visible(nickname != string.Empty);
                speakerNameText.SetText(nickname);
                StartCoroutine(DOSpeech(contents, () => {
                    canvasGroup.DOVisible(0f, .25f);
                    onComplete?.Invoke();
                }));
            });
        }

        private IEnumerator<float> DOSpeech(IReadOnlyList<string> contents, Action onComplete) {
            touchImage.raycastTarget = true;
            foreach (var content in contents) {
                speeching = true;
                skipped = false;
                nextDialogue = false;
                
                indicatorUI.ActiveIndicator(false);
                var contentLength = content.Length;
                contentText.SetText("");
                for (var i = 0; i < contentLength; i++) {
                    if (skipped) {
                        contentText.SetText(content);
                        break;
                    }
                    var text = content[i];
                    contentText.text += text;
                    yield return Timing.WaitForSeconds(0.05f);
                }
                
                speeching = false;
                indicatorUI.ActiveIndicator(true);
                while (!nextDialogue) {
                    yield return Timing.WaitForOneFrame;
                }
            }

            speeching = false;
            skipped = false;
            nextDialogue = false;
            indicatorUI.ActiveIndicator(false);
            contentText.SetText("");
            touchImage.raycastTarget = false;
            onComplete();
        }

        private void OnTouchPanel(PointerEventData data) {
            if (!skipped && speeching) {
                skipped = true;
                return;
            }

            nextDialogue = true;
        }
    }
}