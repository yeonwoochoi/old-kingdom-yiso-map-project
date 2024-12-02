using System;
using System.Collections;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Data;
using Core.Service.Domain;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Game.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Game.Story.Comment {
    public class YisoGameStoryCommentContentUI : YisoGameBasePanelUI {
        [SerializeField] private Image commentBackgroundImage;
        [SerializeField] private TextMeshProUGUI commentText;
        [SerializeField, Title("Settings")] private float fillDuration = .5f;
        [SerializeField] private float showCommentDuration = .5f;
        private CanvasGroup commentTextCanvas;
        private CanvasGroup commentCanvas;
        
        protected override void GetComponentOnAwake() {
            commentCanvas = GetComponent<CanvasGroup>();
            commentTextCanvas = commentText.GetComponent<CanvasGroup>();
        }

        public override YisoGameUITypes GetUIType() => YisoGameUITypes.STORY_COMMENT;

        protected override void Init() {
            Hide();
        }

        protected override void HandleData(object data) {
            var (stage, onComplete) = ((int, UnityAction)) data;
            var comment = YisoServiceProvider.Instance.Get<IYisoDomainService>()
                .GetStageFlowComment(stage, CurrentLocale);
            StartCoroutine(DOComment(comment, onComplete));
        }

        private void Hide() {
            commentCanvas.Visible(false);
            commentTextCanvas.Visible(false);
            commentText.SetText("");
            commentBackgroundImage.fillAmount = 0;
        }

        private IEnumerator DOComment(string comment, UnityAction onComplete = null) {
            commentCanvas.Visible(true);
            commentText.SetText(comment);
            commentBackgroundImage.fillOrigin = (int) Image.OriginHorizontal.Left;
            yield return commentBackgroundImage.DOFillAmount(1f, fillDuration).WaitForCompletion();
            yield return new WaitForSeconds(.5f);
            yield return commentTextCanvas.DOVisible(1f, showCommentDuration).WaitForCompletion();
            var duration = CalculateReadDuration(comment);
            yield return new WaitForSeconds(duration);
            commentBackgroundImage.fillOrigin = (int) Image.OriginHorizontal.Right;
            yield return commentTextCanvas.DOVisible(0f, showCommentDuration).WaitForCompletion();
            yield return new WaitForSeconds(.5f);
            yield return commentBackgroundImage.DOFillAmount(0f, fillDuration).WaitForCompletion();
            onComplete?.Invoke();
        }

        private float CalculateReadDuration(string comment) {
            return 1f;
            var words = comment.Split(" ");
            var duration = words.Length * .2f;
            return duration;
        }
    }
}