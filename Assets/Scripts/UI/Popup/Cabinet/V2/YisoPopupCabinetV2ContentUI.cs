using System;
using System.Collections;
using Core.Domain.Locale;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Game;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UI.Common.Inventory;
using UI.Game.Dialogue;
using UI.Popup.Base;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Utils.Extensions;

namespace UI.Popup.Cabinet.V2 {
    public class YisoPopupCabinetV2ContentUI : YisoPopupBaseContentUI {
        [SerializeField, Title("Content")] private TextMeshProUGUI contentText;
        [SerializeField] private Image characterImage;
        [SerializeField] private Image touchImage;
        [SerializeField] private YisoGameDialogueNextIndicatorUI indicatorUI;
        [Title("Buttons")] [SerializeField] private YisoButtonWithCanvas okButton;
        [SerializeField] private YisoButtonWithCanvas cancelButton;

        private CanvasGroup contentTextCanvas;
        private CanvasGroup characterImageCanvas;

        private bool speeching = false;
        private bool skipped = false;
        private bool nextContent = false;

        private YisoPopupCabinetEventArgs args = null;

        private IEnumerator doContentCoroutine = null;

        protected override void Start() {
            base.Start();
            touchImage.OnPointerClickAsObservable()
                .Subscribe(OnTouchPanel)
                .AddTo(this);
            
            okButton.onClick.AddListener(() => args?.InvokeOkCbs());
            cancelButton.onClick.AddListener(() => args?.InvokeCancelCbs());
        }

        protected override void ClearPanel() {
            speeching = false;
            skipped = false;
            nextContent = false;
            indicatorUI.ActiveIndicator(false);
            contentText.SetText("");
            touchImage.raycastTarget = false;
            
            contentTextCanvas.Visible(false);
            characterImageCanvas.Visible(false);

            okButton.Visible = false;
            cancelButton.Visible = false;

            if (doContentCoroutine != null) {
                StopCoroutine(doContentCoroutine);
                doContentCoroutine = null;    
            }
        }

        protected override void HandleData(object data = null) {
            args = (YisoPopupCabinetEventArgs) data!;

            okButton.SetText(args.OkButtonText == string.Empty ? "확인" : args.OkButtonText);
            cancelButton.SetText(args.CancelButtonText == string.Empty ? "취소" : args.CancelButtonText);

            var currentLocale = YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
            doContentCoroutine = DOContent(currentLocale);
            StartCoroutine(doContentCoroutine);
        }

        private IEnumerator DOContent(YisoLocale.Locale currentLocale) {
            yield return YieldInstructionCache.WaitForSeconds(0.5f);
            touchImage.raycastTarget = true;

            for (var i = 0; i < args.Cabinet.CabinetContents.Count; i++) {
                var content = args.Cabinet.CabinetContents[i];
                var isLast = i == args.Cabinet.CabinetContents.Count - 1;
                
                speeching = true;
                skipped = false;
                nextContent = false;
                
                indicatorUI.ActiveIndicator(false);
                if (content.IsText) {
                    contentText.SetText("");
                    contentTextCanvas.Visible(true);
                    var text = content.Text[currentLocale];
                    var length = text.Length;
                    for (var j = 0; j < length; j++) {
                        if (skipped) {
                            contentText.SetText(text);
                            break;
                        }

                        contentText.text += text[j];
                        yield return YieldInstructionCache.WaitForSeconds(0.03f);
                    }
                } else {
                    characterImage.sprite = content.Image;
                    
                    var duration = 0.5f;
                    var elapsedTime = 0f;
                    while (elapsedTime < duration) {
                        if (skipped) {
                            characterImageCanvas.Visible(true);
                            break;
                        }
                        elapsedTime += Time.deltaTime;
                        characterImageCanvas.alpha = Mathf.Clamp01(elapsedTime / duration);
                        yield return null;
                    }
                }

                speeching = false;
                indicatorUI.ActiveIndicator(true);
                while (!nextContent) {
                    yield return null;
                }
                
                if (isLast) break;
                
                if (content.IsText) {
                    contentText.SetText("");
                    contentTextCanvas.Visible(false);
                } else
                    characterImageCanvas.Visible(false);
            }
            
            touchImage.raycastTarget = false;
            indicatorUI.ActiveIndicator(false);
            okButton.Visible = true;
            cancelButton.Visible = args.CancelCbCount > 1;
        }

        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.CABINET;

        public override void GetComponentOnAwake() {
            contentTextCanvas = contentText.GetComponent<CanvasGroup>();
            if (contentTextCanvas == null)
                contentTextCanvas = contentText.AddComponent<CanvasGroup>();

            characterImageCanvas = characterImage.GetComponent<CanvasGroup>();
            if (characterImageCanvas == null)
                characterImageCanvas = characterImage.AddComponent<CanvasGroup>();
        }

        private void OnTouchPanel(PointerEventData data) {
            if (!skipped && speeching) {
                skipped = true;
                return;
            }

            nextContent = true;
        }
    }
}