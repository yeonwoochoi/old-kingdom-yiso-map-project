using System;
using System.Collections.Generic;
using BaseCamp.Manager;
using Core.Behaviour;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Character;
using Core.Service.Game;
using Core.Service.Scene;
using Core.Service.UI;
using Core.Service.UI.Popup;
using DG.Tweening;
using Sirenix.OdinInspector;
using UI.HUD;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils.Extensions;

namespace BaseCamp.UI.HUD {
    public class YisoPlayerBaseCampHUDCanvasUI : RunIBehaviour {
        [SerializeField, Title("Panels")] private RectTransform leftPanel;
        [SerializeField] private RectTransform rightPanel;
        [SerializeField] private CanvasGroup[] centerPanels;

        [SerializeField, Title("Anchor Position")]
        private float leftAnchorPos = -630;
        [SerializeField] private float rightAnchorPos = 650;

        [SerializeField, Title("Inputs")] private YisoPlayerJoystickUI joystickUI;
        [SerializeField] private YisoPlayerBaseCampHUDInputsUI inputsUI;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button leaveButton;
        
        [SerializeField, Title("Platform")] private List<GameObject> platformAffectObjects;

        private readonly List<CanvasGroup> platformAffectCanvases = new();


        private CanvasGroup canvasGroup;
        
        protected override void Awake() {
            base.Awake();
            var uiService = YisoServiceProvider.Instance.Get<IYisoUIService>();
            uiService.SetBaseCampHUDUI(this);
            canvasGroup = GetComponent<CanvasGroup>();
            
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
            leaveButton.onClick.AddListener(OnClickLeave);

            var sceneService = YisoServiceProvider.Instance.Get<IYisoSceneService>();
            BaseCampMode(false);
            
            menuButton.onClick.AddListener(() => {
                YisoServiceProvider.Instance.Get<IYisoUIService>().ShowMenuUI(YisoMenuTypes.INVENTORY);
            });
        }

        public void BaseCampMode(bool flag) {
            Visible(flag);
            canvasGroup.Visible(flag);
        }

        public void Visible(bool flag) {
            leftPanel.DOAnchorPosX(flag ? 0f : leftAnchorPos, .25f);
            rightPanel.DOAnchorPosX(flag ? 0f : rightAnchorPos, .25f);
            var visible = flag ? 1f : 0f;
            foreach (var center in centerPanels) {
                center.DOVisible(visible, .25f);
            }
        }

        public void ActiveInteract(bool flag, Action onClick) {
            inputsUI.ActiveRegularButton(flag, onClick);
        }

        public YisoPlayerJoystickUI GetJoystick() => joystickUI;

        private void OnClickLeave() {
            YisoServiceProvider.Instance.Get<IYisoPopupUIService>()
                .Alert("알림", "정말 초기화면으로 이동하시겠습니까?", () => {
                    YisoServiceProvider.Instance.Get<IYisoSceneService>()
                        .LoadScene(YisoSceneTypes.INIT);
                }, () => { });
        }
    }
}