using System.Collections.Generic;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Data;
using MEC;
using Sirenix.OdinInspector;
using TMPro;
using UI.Game.Dialogue;
using UI.Popup.Base;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Popup.Npc.Dialogue {
    public class YisoPopupNpcDialogueContentUI : YisoPopupBaseContentUI {
        [Title("Content")] [SerializeField] private TextMeshProUGUI dialogueText;

        [Title("Speaker")] [SerializeField]
        private CanvasGroup speakerTagCanvas;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private Image speakerImage;
        
        [Title("Indicator")]
        [SerializeField] private YisoGameDialogueNextIndicatorUI indicatorUI;

        [Title("Touch Area")] [SerializeField] private Image touchImage;

        private CanvasGroup speakerImageCanvas;
        
        private bool speeching = false;
        private bool skipped = false;
        private bool nextDialogue = false;

        private YisoPopupNpcDialogueArgs cachedArgs = null;

        protected override void HandleData(object data = null) {
            cachedArgs = (YisoPopupNpcDialogueArgs) data!;

            Timing.RunCoroutine(DODialogue());
        }

        protected override void Start() {
            base.Start();
            touchImage.OnPointerClickAsObservable().Subscribe(OnTouchPanel).AddTo(this);
        }

        protected override void ClearPanel() {
            speeching = false;
            skipped = false;
            nextDialogue = false;
            
            indicatorUI.ActiveIndicator(false);
            dialogueText.SetText("");
            touchImage.raycastTarget = false;
            speakerNameText.SetText("");
            speakerTagCanvas.Visible(false);
            speakerImageCanvas.Visible(false);
        }

        private IEnumerator<float> DODialogue() {
            yield return Timing.WaitForSeconds(0.5f);
            touchImage.raycastTarget = true;

            speakerTagCanvas.Visible(true);
            
            foreach (var dialogue in cachedArgs.Dialogue.Dialogues) {
                dialogueText.SetText("");
                speeching = true;
                skipped = false;
                nextDialogue = false;

                var dialogueContent = dialogue.GetContent(CurrentLocale);
                speakerNameText.SetText(dialogue.GetSpeaker(CurrentLocale));
                speakerImage.sprite = dialogue.Icon;
                speakerImageCanvas.Visible(true);
                
                indicatorUI.ActiveIndicator(false);
                var dialogueLength = dialogueContent.Length;

                for (var i = 0; i < dialogueLength; i++) {
                    if (skipped) {
                        dialogueText.SetText(dialogueContent);
                        break;
                    }

                    var text = dialogueContent[i];
                    dialogueText.text += text;
                    yield return Timing.WaitForSeconds(0.03f);
                }

                speeching = false;
                indicatorUI.ActiveIndicator(true);
                while (!nextDialogue) {
                    yield return Timing.WaitForOneFrame;
                }
            }

            touchImage.raycastTarget = false;
            indicatorUI.ActiveIndicator(false);
            cachedArgs.InvokeOnClose();
        }

        public override void GetComponentOnAwake() {
            base.GetComponentOnAwake();
            speakerImageCanvas = speakerImage.GetComponent<CanvasGroup>();
        }

        private void OnTouchPanel(PointerEventData data) {
            if (!skipped && speeching) {
                skipped = true;
                return;
            }

            nextDialogue = true;
        }
        
        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.NPC_DIALOGUE;
    }
}