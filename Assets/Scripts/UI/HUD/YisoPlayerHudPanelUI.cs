using System;
using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Character;
using Core.Service.Game;
using Core.Service.Scene;
using Core.Service.UI;
using Core.Service.UI.HUD;
using DG.Tweening;
using Sirenix.OdinInspector;
using UI.HUD.Interact;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace UI.HUD {
    public class YisoPlayerHudPanelUI : RunIBehaviour {
        [SerializeField, Title("Panels")] private RectTransform leftPanel;
        [SerializeField] private RectTransform rightPanel;
        [SerializeField] private CanvasGroup[] centerPanels;

        [SerializeField, Title("Anchor Position")]
        private float leftAnchorPos = -630;
        [SerializeField] private float rightAnchorPos = 650;

        [SerializeField, Title("Inputs")] private YisoPlayerInputsUI inputsUI;

        [SerializeField, Title("Platform")] private List<GameObject> platformAffectObjects;
        
        private CanvasGroup canvasGroup;

        private IYisoHUDUIService huduiService;

        private readonly List<CanvasGroup> platformAffectCanvases = new();

        public void Visible(bool flag) {
            OnVisibleHUDPanel(flag);
        }

        public void GameMode(bool flag) {
            OnVisibleHUDPanel(flag);
            canvasGroup.Visible(flag);
        }

        protected override void Awake() {
            base.Awake();
            canvasGroup = GetComponent<CanvasGroup>();
            
            huduiService = YisoServiceProvider.Instance.Get<IYisoHUDUIService>();
            var uiService = YisoServiceProvider.Instance.Get<IYisoUIService>();
            uiService.SetHUDUI(this);
            huduiService.SetPanelUI(this);

            foreach (var affectObject in platformAffectObjects) {
                var cg = affectObject.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = affectObject.AddComponent<CanvasGroup>();
                platformAffectCanvases.Add(cg);
            }

            if (!YisoServiceProvider.Instance.Get<IYisoGameService>().IsMobile()) {
                foreach (var cg in platformAffectCanvases) cg.Visible(false);
            }
        }

        protected override void Start() {
            base.Start();
            var uiService = YisoServiceProvider.Instance.Get<IYisoUIService>();
            uiService.SetHUDUI(this);
            var sceneService = YisoServiceProvider.Instance.Get<IYisoSceneService>();
            GameMode(sceneService.GetCurrentScene().ShowHUD());
        }

        protected override void OnEnable() {
            base.OnEnable();
            huduiService.RegisterOnVisibleHUD(OnVisibleHUDPanel);
        }

        protected override void OnDisable() {
            base.OnDisable();
            huduiService.UnregisterOnVisibleHUD(OnVisibleHUDPanel);
        }
        
        public void ShowInteractButton(YisoHudUIInteractTypes type, UnityAction onClick) {
            inputsUI.ShowInteractButton(type, onClick);
        }

        public void HideInteractButton(YisoHudUIInteractTypes type) {
            inputsUI.HideInteractButton(type);
        }
        
        private void OnVisibleHUDPanel(bool flag) {
            leftPanel.DOAnchorPosX(flag ? 0f : leftAnchorPos, .25f);
            rightPanel.DOAnchorPosX(flag ? 0f : rightAnchorPos, .25f);
            var visible = flag ? 1f : 0f;
            foreach (var center in centerPanels) {
                center.DOVisible(visible, .25f);
            }
        }
    }
}