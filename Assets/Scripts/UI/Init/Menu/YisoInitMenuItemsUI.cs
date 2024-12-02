using System;
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
using UI.Init.Setting;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Utils.Extensions;

namespace UI.Init.Menu {
    public class YisoInitMenuItemsUI : RunIBehaviour {
        [SerializeField] private List<YisoInitMenuItemUI> menuItems;
        [SerializeField] private YisoInitSettingCanvasUI initSettingCanvasUI;

        public CanvasGroup PanelCanvas { get; private set; }

        protected override void Start() {
            base.Start();
            PanelCanvas = GetComponent<CanvasGroup>();
            for (var i = 0; i < menuItems.Count; i++) {
                menuItems[i].MenuToggle.onValueChanged.AddListener(OnMenuSelected(i));
                var index = i;
                menuItems[i].SelectedImage.OnPointerClickAsObservable()
                    .Subscribe(_ => OnClickMenu(index)).AddTo(this);
            }
        }

        private void OnClickMenu(int index) {
            var type = index.ToEnum<Types>();
            switch (type) {
                case Types.START_NEW:
                    YisoServiceProvider.Instance.Get<IYisoPopupUIService>()
                        .AlertS("경고", "데이터가 전부 지워집니다\n계속하시겠습니까?",
                            () => {
                                YisoServiceProvider.Instance.Get<IYisoCharacterService>().ResetPlayerData();
                                YisoServiceProvider.Instance.Get<IYisoSceneService>()
                                    .LoadScene(YisoSceneTypes.STORY);
                            },
                            () => { });
                    break;
                case Types.START_CONTINUE:
                    YisoServiceProvider.Instance.Get<IYisoSceneService>()
                        .LoadScene(YisoSceneTypes.BASE_CAMP);
                    break;
                case Types.OPTIONS:
                    initSettingCanvasUI.OnVisibleCanvas(true);
                    break;
                case Types.EXIT:
                    Application.Quit(0);
                    break;
            }
        }

        private UnityAction<bool> OnMenuSelected(int index) => flag => {
            menuItems[index].OnToggle(flag);
        };

        public enum Types {
            START_NEW, START_CONTINUE,
            OPTIONS, EXIT
        }
    }
}