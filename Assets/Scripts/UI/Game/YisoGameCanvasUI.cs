using System;
using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Types;
using Core.Service;
using Core.Service.UI;
using Core.Service.UI.Event;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Components;
using UI.Game.Base;
using UI.Game.Dialogue;
using UI.Game.Loading;
using UnityEngine;
using Utils.Extensions;

namespace UI.Game {
    public class YisoGameCanvasUI : YisoUIController {
        [SerializeField, Title("Backgrounds")] private CanvasGroup backgroundCanvas;
        [SerializeField] private YisoHoldButtonUI closeButton;
        [SerializeField] private TextMeshProUGUI titleText;

        [SerializeField, Title("Panels")] private CanvasGroup basicKeyPanelCanvas;
        [SerializeField] private YisoGameLoadingPanelUI loadingPanelUI;
        [SerializeField] private YisoGameDialoguePanelUI dialoguePanelUI;

        [SerializeField] private List<YisoGameBasePanelUI> panelUis;
        [SerializeField] private CanvasGroup canvasGroup;

        private readonly Dictionary<YisoGameUITypes, int> cachedIndexDict = new();

        private YisoGameUITypes currentType;

        public bool IsUIActive { get; private set; }

        protected override void Awake() {
            base.Awake();
            YisoServiceProvider.Instance.Get<IYisoUIService>().SetGameUI(this);
        }

        protected override void Start() {
            base.Start();
            for (var i = 0; i < panelUis.Count; i++) {
                cachedIndexDict[panelUis[i].GetUIType()] = i;
            }
        }

        public void HideCanvas() {
            canvasGroup.Visible(false);
            var index = cachedIndexDict[currentType];
            panelUis[index].Visible(false);
            if (currentType.HasBlur())
                backgroundCanvas.Visible(false);

            switch (currentType) {
                case YisoGameUITypes.STORY_CLEAR:
                    IsUIActive = false;
                    break;
            }
        }

        public void ShowCanvas(YisoGameUITypes type, object data = null) {
            switch (type) {
                case YisoGameUITypes.STORY_CLEAR:
                    IsUIActive = true;
                    break;
            }

            currentType = type;
            var index = cachedIndexDict[type];
            panelUis[index].Visible(true, data);
            titleText.gameObject.SetActive(type.HasTitle());
            closeButton.gameObject.SetActive(type.HasCloseButton());
            titleText.SetText(type.ToString(CurrentLocale));
            backgroundCanvas.Visible(type.HasBlur());
            canvasGroup.Visible(true);
        }

        public void Visible(GameUIEventArgs args) {
            OnShowGameCanvasUI(args);
        }

        public void VisibleLoading(bool flag, Action callback = null) {
            loadingPanelUI.Visible(flag, callback);
        }

        public void ShowDialogue(GameUIDialogueEventArgs args) {
            dialoguePanelUI.StartDialogue(args.Speaker, args.Contents, args.OnComplete);
        }

        public void Visible(bool flag) {
            var value = flag ? 1f : 0f;
            backgroundCanvas.DOVisible(value, .25f);
            if (!flag) {
                titleText.SetText("");
                basicKeyPanelCanvas.Visible(false);
            }
        }

        private void OnShowGameCanvasUI(GameUIEventArgs args) {
            titleText.SetText(args.Title);

            switch (args) {
                case GameUIBasicKeyEventArgs keyArgs:
                    basicKeyPanelCanvas.Visible(true);
                    break;
            }

            closeButton.SetButton(args.NeedHold, args.OnClick);
            backgroundCanvas.DOVisible(1f, .25f);
        }

        public void OnClickClose() { }

        public enum Types {
            BASIC_KEY,
            INFO,
            INFO_PRESS
        }
    }
}