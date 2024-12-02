using System.Collections;
using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Data;
using Core.Domain.Types;
using Core.Logger;
using Core.Service;
using Core.Service.Character;
using Core.Service.Data;
using Core.Service.Game;
using Core.Service.Scene;
using Core.Service.Stage;
using Core.Service.UI.Popup;
using DG.Tweening;
using MEC;
using UI.Init.Loading;
using UI.Init.Menu;
using UnityEngine;
using Utils;
using Utils.Extensions;

namespace UI.Init {
    public class YisoInitCanvasUI : RunIBehaviour {
        [SerializeField] private YisoInitLoadingTabToStartUI tabToStartUI;
        [SerializeField] private YisoInitLoadingProgressUI loadingProgressUI;
        [SerializeField] private YisoInitMenuItemsUI menuItemsUI;
        [SerializeField] private Texture2D normalCursor;

        protected override void Awake() {
            /*var isMobile = YisoServiceProvider.Instance.Get<IYisoGameService>().IsMobile();
            if (!isMobile) {
                var spot = Vector2.zero;
                Cursor.SetCursor(normalCursor, spot, CursorMode.Auto);
            }*/
        }

        protected override void Start() {
            StartCoroutine(DOLoading());
        }

        private IEnumerator DOLoading() {
            yield return YieldInstructionCache.WaitForSeconds(1f);
            LoadData();
            yield return loadingProgressUI.PanelCanvas.DOVisible(1f, 0.25f).WaitForCompletion();
            yield return loadingProgressUI.DOProgress(2f, predicate: () => loadedAllData);
            
            var characterService = YisoServiceProvider.Instance.Get<IYisoCharacterService>();
            if (!characterService.IsDataVersionMatched()) {
                yield return YieldInstructionCache.WaitForSeconds(0.2f);
                var sceneService = YisoServiceProvider.Instance.Get<IYisoSceneService>();
                var popupService = YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
                popupService.AlertS("데이터 업데이트", "앱 내 데이터가 업데이트 되었습니다\n초기화 후 다시 진행됩니다.", () => {
                    characterService.ResetPlayerData();
                    sceneService.LoadScene(YisoSceneTypes.STORY);
                }, hideCancel: true);
                yield break;
            }
            
            yield return loadingProgressUI.PanelCanvas.DOVisible(0f, 0.25f).WaitForCompletion();
            tabToStartUI.PanelCanvas.Visible(true);
            yield return tabToStartUI.DOBlink();
            tabToStartUI.PanelCanvas.Visible(false);
            var currentStage = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();
            if (currentStage > 1) {
                yield return menuItemsUI.PanelCanvas.DOVisible(1f, 0.25f).WaitForCompletion();
            } else {
                YisoServiceProvider.Instance.Get<IYisoSceneService>().LoadScene(YisoSceneTypes.STORY);
            }
        }

        private bool loadedAllData = false;

        private void LoadData() {
            loadedAllData = true;
        }
    }
}