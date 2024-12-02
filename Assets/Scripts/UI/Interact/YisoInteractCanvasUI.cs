using System.Collections.Generic;
using Core.Domain.Types;
using Core.Service;
using Core.Service.UI;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Interact.Base;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact {
    public class YisoInteractCanvasUI : YisoUIController {
        [SerializeField, Title("Panels")] private List<YisoInteractBasePanelUI> panelUis;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField, Title("Close Button")] private Image closeButtonImage;
        [SerializeField] private Color whiteColor = Color.white;
        [SerializeField] private Color blackColor = Color.black;
        
        public bool IsUIActive { get; private set; }

        private readonly Dictionary<YisoInteractTypes, int> cachedIndexDict = new();

        private YisoInteractTypes currentType;

        protected override void Awake() {
            base.Awake();
            YisoServiceProvider.Instance.Get<IYisoUIService>().SetInteractUI(this);
        }

        protected override void Start() {
            base.Start();
            for (var i = 0; i < panelUis.Count; i++) {
                cachedIndexDict[panelUis[i].GetType()] = i;
            }
        }
        
        public void HideCanvas() {
            IsUIActive = false;
            canvasGroup.Visible(false);
            var index = cachedIndexDict[currentType];
            panelUis[index].Visible(false);
            closeButtonImage.color = Color.white;
        }

        public void ShowCanvas(YisoInteractTypes type, object data = null) {
            IsUIActive = true;
            currentType = type;
            var index = cachedIndexDict[type];
            panelUis[index].Visible(true, data);
            if (type == YisoInteractTypes.WORLD_MAP) {
                titleText.SetText("");
                closeButtonImage.color = blackColor;
            }
            else {
                var title = type.ToString(CurrentLocale);
                titleText.SetText(title);
                closeButtonImage.color = whiteColor;
            }
            canvasGroup.Visible(true);
        }

        public void OnClickClose() {
            YisoServiceProvider.Instance.Get<IYisoUIService>().ShowHUDUI();
        }
    }
}