using System;
using Core.Behaviour;
using Core.Domain.Types;
using Core.Service;
using Core.Service.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace BaseCamp.UI.HUD {
    public class YisoPlayerBaseCampHUDInputsUI : RunIBehaviour {
        [SerializeField] private Button regularButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button settingButton;
        
        private CanvasGroup regularButtonCanvas;

        private Action onClickInteract = null;

        private Image buttonImage;

        protected override void Start() {
            base.Start();
            buttonImage = regularButton.GetComponent<Image>();
            regularButtonCanvas = regularButton.GetComponent<CanvasGroup>();
            
            regularButton.onClick.AddListener(() => onClickInteract?.Invoke());
            
            inventoryButton.onClick.AddListener(() => {
                YisoServiceProvider.Instance.Get<IYisoUIService>().ShowMenuUI(YisoMenuTypes.INVENTORY);
            });
            
            settingButton.onClick.AddListener(() => {
                YisoServiceProvider.Instance.Get<IYisoUIService>().ShowMenuUI(YisoMenuTypes.SETTINGS);
            });
        }

        public void ActiveRegularButton(bool flag, Action onClick) {
            if (flag) onClickInteract = onClick;
            var value = flag ? 1f : 0.5f;
            if (regularButtonCanvas == null) return;
            regularButtonCanvas.alpha = value;
            regularButtonCanvas.interactable = flag;
            regularButtonCanvas.blocksRaycasts = flag;
        }
    }
}