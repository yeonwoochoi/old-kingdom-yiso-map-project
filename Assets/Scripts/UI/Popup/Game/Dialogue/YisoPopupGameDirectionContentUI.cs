using System.Collections.Generic;
using Core.Domain.Types;
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

namespace UI.Popup.Game.Dialogue {
    public class YisoPopupGameDirectionContentUI : YisoPopupBaseContentUI {
        [SerializeField, Title("Content")] private TextMeshProUGUI dialogueText;

        [Title("Indicator")] [SerializeField] private YisoGameDialogueNextIndicatorUI indicatorUI;

        [Title("Touch Area")] [SerializeField] private Image touchImage;

        private bool texting = false;
        private bool skipped = false;
        private bool nextDialogue = false;

        protected override void Start() {
            base.Start();
            touchImage.OnPointerClickAsObservable().Subscribe(OnTouchPanel).AddTo(this);
        }

        protected override void ClearPanel() {
            dialogueText.SetText("");
            indicatorUI.ActiveIndicator(false);
            cachedArgs = null;
            touchImage.raycastTarget = false;
        }

        private YisoPopupGameDialogueArgs cachedArgs;

        protected override void HandleData(object data = null) {
            cachedArgs = (YisoPopupGameDialogueArgs) data!;
            StartCoroutine(DODialogue());
        }

        private IEnumerator<float> DODialogue() {
            yield return Timing.WaitForSeconds(0.5f);

            foreach (var direction in cachedArgs.Direction.Directions) {
                var content = direction[CurrentLocale];
                touchImage.raycastTarget = true;
                
                dialogueText.SetText("");
                texting = true;
                skipped = false;
                nextDialogue = false;
                
                indicatorUI.ActiveIndicator(false);
                var dialogueLength = content.Length;

                for (var i = 0; i < dialogueLength; i++) {
                    if (skipped) {
                        dialogueText.SetText(content);
                        break;
                    }

                    var text = content[i];
                    dialogueText.text += text;
                    yield return Timing.WaitForSeconds(0.03f);
                }

                texting = false;
                indicatorUI.ActiveIndicator(true);
                while (!nextDialogue) {
                    yield return Timing.WaitForOneFrame;
                }
            }

            touchImage.raycastTarget = false;
            indicatorUI.ActiveIndicator(false);
            cachedArgs.Invoke();
        }

        private void OnTouchPanel(PointerEventData data) {
            if (!skipped && texting) {
                skipped = true;
                return;
            }

            nextDialogue = true;
        }
        
        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.GAME_DIRECTION;
    }
}