using System;
using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Types;
using Core.Service;
using Core.Service.UI;
using Core.Service.UI.Popup2;
using Sirenix.OdinInspector;
using UI.Popup2.Base;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Popup2 {
    public class YisoPopup2CanvasUI : RunIBehaviour {
        [SerializeField, Title("Touch Area")] private Image touchImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField, Title("Popups")] private List<YisoPopup2BaseContentUI> contentUIs;

        private readonly Dictionary<YisoPopup2Types, YisoPopup2BaseContentUI> contents = new();

        private YisoPopup2Types currentType;
        
        public bool IsUIActive { get; private set; }

        protected override void Awake() {
            base.Awake();
            YisoServiceProvider.Instance.Get<IYisoPopup2UIService>().SetCanvasUI(this);
            YisoServiceProvider.Instance.Get<IYisoUIService>().SetPopup2CanvasUI(this);
        }

        protected override void Start() {
            base.Start();
            foreach (var content in contentUIs) {
                contents[content.GetPopupTypes()] = content;
            }

            touchImage.OnPointerClickAsObservable().Subscribe(OnTouch).AddTo(this);
        }

        public void ShowCanvas(YisoPopup2Types type, object data = null) {
            IsUIActive = true;
            currentType = type;
            contents[currentType].Visible(true, data);
            canvasGroup.Visible(true);
        }

        public void HideCanvas() {
            IsUIActive = false;
            canvasGroup.Visible(false);
            contents[currentType].Visible(false);
        }

        private void OnTouch(PointerEventData eventData) {
            HideCanvas();
        }
    }
}