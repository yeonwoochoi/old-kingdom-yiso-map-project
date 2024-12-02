using System.Collections;
using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Locale;
using DG.Tweening;
using MEC;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Utils.Extensions;
using Utils.ObjectId;

namespace UI.Global.Saving {
    public class YisoGlobalSavingContentUI : YisoUIController {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI savingText;
        [SerializeField] private float loadingDuration = 0.5f;


        private readonly YisoLocale SAVING_TEXT = new (){
            kr = "저장중",
            en = "Saving"
        };

        private IEnumerator savingCoroutine = null;
        private string textingCoroutineTag;

        protected override void Start() {
            textingCoroutineTag = YisoObjectID.GenerateString();
        }

        public void Show(UnityAction onShowed = null) {
            StartCoroutine(DOSavingText(), textingCoroutineTag);
            savingCoroutine = DOSaving(onShowed);
            StartCoroutine(savingCoroutine);
        }

        private void Clear() {
            StopCoroutine(savingCoroutine);
            savingCoroutine = null;
            KillCoroutine(textingCoroutineTag);
            savingText.SetText(SAVING_TEXT[CurrentLocale]);
        }

        private IEnumerator DOSaving(UnityAction onShowed = null) {
            yield return canvasGroup.DOVisible(1f, loadingDuration).WaitForCompletion();
            onShowed?.Invoke();
            yield return YieldInstructionCache.WaitForSeconds(loadingDuration * 2f);
            yield return canvasGroup.DOVisible(0f, loadingDuration).WaitForCompletion();
            Clear();
        }

        private IEnumerator<float> DOSavingText() {
            var text = SAVING_TEXT[CurrentLocale];
            var dotCount = 0;
            savingText.SetText(text);
            while (true) {
                if (dotCount == 3) {
                    savingText.SetText(text);
                    dotCount = 0;
                } else {
                    savingText.text += ".";
                    dotCount++;
                }
                
                yield return Timing.WaitForSeconds(0.3f);
            }
        }
    }
}