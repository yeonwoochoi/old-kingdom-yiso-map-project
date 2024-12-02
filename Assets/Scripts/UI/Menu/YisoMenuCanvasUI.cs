using System;
using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Types;
using Core.Service;
using Core.Service.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu {
    public class YisoMenuCanvasUI : RunIBehaviour {
        [SerializeField] private Button backgroundCloseButton;
        [SerializeField, Title("Main Panel")] private YisoMenuMainPanelUI mainPanelUI;

        private IYisoUIService uiService;
        
        public bool IsUIActive { get; private set; }
        
        protected override void Awake() {
            base.Awake();
            uiService = YisoServiceProvider.Instance.Get<IYisoUIService>();
            uiService.SetMenuUI(this);
        }

        protected override void Start() {
            backgroundCloseButton.onClick.AddListener(OnClickCloseButton);
        }

        public void ShowCanvas(YisoMenuTypes type, object data) {
            IsUIActive = true;
            mainPanelUI.Show(type, data);
        }

        public void HideCanvas() {
            IsUIActive = false;
            mainPanelUI.Hide();
        }

        private void OnClickCloseButton() {
            uiService.ShowHUDUI();
        }
    }
}