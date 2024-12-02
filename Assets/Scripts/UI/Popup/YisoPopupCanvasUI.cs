using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Character;
using Core.Service.Data;
using Core.Service.Scene;
using Core.Service.UI;
using Core.Service.UI.Popup;
using Sirenix.OdinInspector;
using UI.Popup.Alert;
using UI.Popup.Base;
using UI.Popup.Input;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Popup {
    public class YisoPopupCanvasUI : RunIBehaviour {
        [SerializeField] private Image background;
        [SerializeField, Title("Touch Area")] private Image touchImage;
        [SerializeField, Title("Popups")] private List<YisoPopupBaseContentUI> contentUIs;

        private readonly Dictionary<YisoPopupTypes, YisoPopupBaseContentUI> contents = new();

        private CanvasGroup canvasGroup;

        public bool Visible {
            get => canvasGroup.IsVisible();
            set => canvasGroup.Visible(value);
        }
        
        public bool IsUIActive { get; private set; }
        
        private bool activeBackgroundTouch = false;
        private YisoPopupTypes currentType;

        protected override void Awake() {
            base.Awake();
            YisoServiceProvider.Instance.Get<IYisoPopupUIService>().SetPopupCanvas(this);
            YisoServiceProvider.Instance.Get<IYisoUIService>().SetPopupCanvasUI(this);
        }

        protected override void Start() {
            base.Start();
            touchImage.OnPointerClickAsObservable().Subscribe(OnTouch).AddTo(this);
            canvasGroup = GetComponent<CanvasGroup>();
            foreach (var content in contentUIs) {
                contents[content.GetPopupType()] = content;
            }

            var characterService = YisoServiceProvider.Instance.Get<IYisoCharacterService>();
            var popupService = YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
            if (!characterService.IsDataVersionMatched()) {
                var sceneService = YisoServiceProvider.Instance.Get<IYisoSceneService>();
                if (sceneService.GetCurrentScene() == YisoSceneTypes.INIT) return;
                popupService.AlertS("데이터 업데이트", "앱 내 데이터가 업데이트 되었습니다\n초기화 후 다시 진행됩니다.", () => {
                    YisoServiceProvider.Instance.Get<IYisoCharacterService>().ResetPlayerData();
                    if (sceneService.GetCurrentScene() == YisoSceneTypes.INIT) {
                        sceneService.LoadScene(YisoSceneTypes.STORY);
                        return;
                    }
                    sceneService.LoadScene(YisoSceneTypes.INIT);
                }, hideCancel: true);
            }
        }

        public void ShowCanvas(YisoPopupTypes type, object data = null) {
            IsUIActive = true;
            background.enabled = type.ShowBackground();
            currentType = type;
            contents[currentType].Visible(true, data);
            canvasGroup.Visible(true);
        }

        public void HideCanvas() {
            IsUIActive = false;
            canvasGroup.Visible(false);
            contents[currentType].Visible(false);
        }

        private void OnTouch(PointerEventData data) { }
    }
}