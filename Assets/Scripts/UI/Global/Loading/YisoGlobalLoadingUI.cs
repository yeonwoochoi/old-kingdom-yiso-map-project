using System.Collections;
using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Locale;
using Core.Service;
using Core.Service.Data;
using Core.Service.Domain;
using Core.Service.Game;
using Core.Service.Stage;
using Core.UI.Loading;
using DG.Tweening;
using MEC;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;
using Utils.ObjectId;

namespace UI.Global.Loading {
    public class YisoGlobalLoadingUI : RunIBehaviour {
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private TextMeshProUGUI commentText;
        [SerializeField] private YisoLoadingCommentsSO commentsSO;
        [SerializeField] private Image logoImage;

        private static readonly int ShineLocation = Shader.PropertyToID("_ShineLocation");
        private const float SHINE_INCREASE_TIME = 1f;
        private const string LOADING_DEFAULT = "Loading";

        private CanvasGroup commentCanvas;
        private CanvasGroup canvasGroup;
        private Material logoMaterial;

        public bool Visible {
            get => canvasGroup.IsVisible();
            set => canvasGroup.Visible(value);
        }

        private string testTag;
        
        private string loadingTag;
        private string shineTag;

        protected override void Start() {
            base.Start();
            commentCanvas = commentText.GetComponent<CanvasGroup>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            commentCanvas.Visible(false);
            logoMaterial = logoImage.material;

            shineTag = YisoObjectID.GenerateString();
            loadingTag = YisoObjectID.GenerateString();
        }

        public void StartLoading() {
            var comment = commentsSO.GetRandomComment(YisoLocale.Locale.KR);
            commentText.SetText(comment);
            Visible = true;
            StartCoroutine(DOShineLogo(), shineTag);
            StartCoroutine(DOLoadingText(), loadingTag);
        }

        public void StartStageLoading() {
            var currentStage = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();
            var locale = YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
            var comment = YisoServiceProvider.Instance.Get<IYisoDomainService>().GetStoryLoadingComment(currentStage, locale);
            commentText.SetText(comment);
            Visible = true;
            StartCoroutine(DOShineLogo(), shineTag);
            StartCoroutine(DOLoadingText(), loadingTag);
        }

        public void StopLoading() {
            Visible = false;
            KillCoroutine(loadingTag);
            KillCoroutine(shineTag);
            commentCanvas.Visible(false);
            loadingText.SetText(LOADING_DEFAULT);
            logoMaterial.SetFloat(ShineLocation, 0f);
        }

        private IEnumerator<float> DOShineLogo() {
            while (true) {
                var timeElapsed = 0f;
                logoMaterial.SetFloat(ShineLocation, 0f);
                while (timeElapsed < SHINE_INCREASE_TIME) {
                    var currentValue = Mathf.Lerp(0, 1, timeElapsed / SHINE_INCREASE_TIME);
                    timeElapsed += Time.deltaTime;
                    logoMaterial.SetFloat(ShineLocation, currentValue);
                    yield return Timing.WaitForOneFrame;
                }
                yield return Timing.WaitForSeconds(0.3f);
            }
        }

        private IEnumerator<float> DOLoadingText() {
            commentCanvas.DOVisible(1f, 1f);
            var dotCount = 0;
            loadingText.SetText(LOADING_DEFAULT);
            while (true) {
                if (dotCount == 3) {
                    loadingText.SetText(LOADING_DEFAULT);
                    dotCount = 0;
                } else {
                    loadingText.text += ".";
                    dotCount++;
                }
                
                yield return Timing.WaitForSeconds(0.5f);
            }
        }
    }
}