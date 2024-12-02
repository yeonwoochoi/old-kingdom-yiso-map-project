using System.Collections.Generic;
using System.Linq;
using Core.Domain.Locale;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Game;
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

namespace UI.Popup.Cabinet {
    public class YisoPopupCabinetContentUI : YisoPopupBaseContentUI {
        [Title("Content")]
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Image touchImage;
        [SerializeField] private YisoGameDialogueNextIndicatorUI indicatorUI;
        [Title("Buttons")]
        [SerializeField] private Button okButton; 
        [SerializeField] private TextMeshProUGUI okButtonText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI cancelButtonText;

        private CanvasGroup okButtonCanvas;
        private CanvasGroup cancelButtonCanvas;
        
        private bool speeching = false;
        private bool skipped = false;
        private bool nextContent = false;

        private YisoPopupCabinetEventArgs cachedArgs = null;

        protected override void Start() {
            base.Start();
            touchImage.OnPointerClickAsObservable()
                .Subscribe(OnTouchPanel)
                .AddTo(this);
            
            okButton.onClick.AddListener(() => cachedArgs?.InvokeOkCbs());
            cancelButton.onClick.AddListener(() => cachedArgs?.InvokeCancelCbs());
        }

        protected override void ClearPanel() {
            speeching = false;
            skipped = false;
            nextContent = false;
            indicatorUI.ActiveIndicator(false);
            contentText.SetText("");
            touchImage.raycastTarget = false;
            
            okButtonCanvas.Visible(false);
            cancelButtonCanvas.Visible(false);
        }

        protected override void HandleData(object data = null) {
            cachedArgs = (YisoPopupCabinetEventArgs) data!;

            okButtonText.SetText(cachedArgs.OkButtonText == string.Empty ? "확인": cachedArgs.OkButtonText);
            cancelButtonText.SetText(cachedArgs.CancelButtonText == string.Empty ? "취소": cachedArgs.CancelButtonText);
            
            Timing.RunCoroutine(DOContent(cachedArgs.Cabinet.Contents));
        }

        private IEnumerator<float> DOContent(IReadOnlyList<YisoLocale> contentLocales) {
            yield return Timing.WaitForSeconds(0.5f);
            touchImage.raycastTarget = true;
            var currentLocal = YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
            var contents = contentLocales.Select(c => c[currentLocal]);

            foreach (var content in contents) {
                contentText.SetText("");
                speeching = true;
                skipped = false;
                nextContent = false;
                
                indicatorUI.ActiveIndicator(false);
                var contentLength = content.Length;

                for (var i = 0; i < contentLength; i++) {
                    if (skipped) {
                        contentText.SetText(content);
                        break;
                    }

                    var text = content[i];
                    contentText.text += text;
                    yield return Timing.WaitForSeconds(0.03f);
                }

                speeching = false;
                indicatorUI.ActiveIndicator(true);
                while (!nextContent) {
                    yield return Timing.WaitForOneFrame;
                }
            }

            touchImage.raycastTarget = false;
            indicatorUI.ActiveIndicator(false);
            okButtonCanvas.Visible(true);
            cancelButtonCanvas.Visible(cachedArgs.CancelCbCount > 1);
        }

        public override void GetComponentOnAwake() {
            okButtonCanvas = okButton.GetComponent<CanvasGroup>();
            cancelButtonCanvas = cancelButton.GetComponent<CanvasGroup>();
        }

        public override YisoPopupTypes GetPopupType() => YisoPopupTypes.CABINET;

        private void OnTouchPanel(PointerEventData data) {
            if (!skipped && speeching) {
                skipped = true;
                return;
            }

            nextContent = true;
        }
    }
}