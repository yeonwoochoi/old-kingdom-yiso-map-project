using Core.Domain.Types;
using Core.Service;
using Core.Service.UI;
using Sirenix.OdinInspector;
using UI.Game.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Game.Story.Clear {
    public class YisoGameStoryClearContentUI : YisoGameBasePanelUI {
        [SerializeField, Title("Buttons")] private Button baseCampButton;
        [SerializeField] private Button storyButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private CanvasGroup developingCanvas;

        private CanvasGroup storyButtonCanvas;
        private UnityAction onClickBaseCamp = null;
        private UnityAction onClickStory = null;
        private UnityAction onClickClose = null;

        protected override void Start() {
            base.Start();
            storyButtonCanvas = storyButton.GetComponent<CanvasGroup>();
            baseCampButton.onClick.AddListener(() => onClickBaseCamp?.Invoke());
            storyButton.onClick.AddListener(() => onClickStory?.Invoke());
            closeButton.onClick.AddListener(() => onClickClose?.Invoke());
        }

        protected override void HandleData(object data) {
            var (onClickBaseCamp, onClickStory, onClickClose, existNextStage) = ((UnityAction, UnityAction, UnityAction, bool)) data;

            this.onClickBaseCamp = onClickBaseCamp;
            this.onClickStory = onClickStory;
            this.onClickClose = onClickClose;
            
            developingCanvas.Visible(!existNextStage);
            storyButtonCanvas.Visible(existNextStage);
        }

        protected override void Init() {
            ClearPanel();
        }

        protected override void ClearPanel() {
            onClickBaseCamp = null;
            onClickStory = null;
            onClickClose = null;
        }

        public override YisoGameUITypes GetUIType() => YisoGameUITypes.STORY_CLEAR;
    }
}