using Core.Domain.Quest;
using Core.Domain.Types;
using Core.Service.UI.Event;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Service.UI.Game {
    public class YisoGameUIService : IYisoGameUIService {
        private event UnityAction<GameUIDialogueEventArgs> OnDialogueEvent;
        private event UnityAction<GameUIEventArgs> OnGameUIEvent;

        private readonly IYisoUIService uiService;
        
        public void RegisterOnDialogue(UnityAction<GameUIDialogueEventArgs> handler) {
            OnDialogueEvent += handler;
        }
        
        public void UnregisterOnDialogue(UnityAction<GameUIDialogueEventArgs> handler) {
            OnDialogueEvent -= handler;
        }

        public void RegisterOnGame(UnityAction<GameUIEventArgs> handler) {
            OnGameUIEvent += handler;
        }

        public void UnregisterOnGame(UnityAction<GameUIEventArgs> handler) {
            OnGameUIEvent -= handler;
        }

        public void RaiseDialogue(GameUIDialogueEventArgs args) {
            OnDialogueEvent?.Invoke(args);
        }

        public void RaiseGame(GameUIEventArgs args) {
            OnGameUIEvent?.Invoke(args);
        }

        public void ShowStoryClearPopup(UnityAction onClickBaseCamp, UnityAction onClickStory, UnityAction onClickClose = null, bool existNextStage = true) {
            var dataPack = ((UnityAction)NewOnClickBaseCamp, (UnityAction)NewOnClickStory, (UnityAction) NewOnClickClear, existNextStage);
            uiService.ActiveOnlyGameUI(true, YisoGameUITypes.STORY_CLEAR, dataPack);
            return;

            void NewOnClickStory() {
                onClickStory?.Invoke();
                uiService.ActiveOnlyGameUI(false, YisoGameUITypes.STORY_CLEAR);
            }

            void NewOnClickBaseCamp() {
                onClickBaseCamp?.Invoke();
                uiService.ActiveOnlyGameUI(false, YisoGameUITypes.STORY_CLEAR);
            }

            void NewOnClickClear() {
                onClickClose?.Invoke();
                uiService.ActiveOnlyGameUI(false, YisoGameUITypes.STORY_CLEAR);
            }
        }

        public void ShowStageLoadingComment(bool hideHud, int stage, UnityAction onComplete = null) {
            var dataPack = (stage, (UnityAction) NewOnComplete);
            
            if (hideHud) uiService.ShowGameUI(YisoGameUITypes.STORY_COMMENT, dataPack);
            else uiService.ActiveOnlyGameUI(true, YisoGameUITypes.STORY_COMMENT, dataPack);
            return;
            
            void NewOnComplete() {
                if (hideHud) uiService.ShowHUDUI();
                else uiService.ActiveOnlyGameUI(false, YisoGameUITypes.STORY_COMMENT);
                onComplete?.Invoke();
            }
        }

        public void FloatingText(string value, UnityAction onComplete = null) {
            var dataPack = (value, (UnityAction) NewOnCompleted);
            uiService.ActiveOnlyGameUI(true, YisoGameUITypes.FLOATING_TEXT, dataPack);
            
            return;
            void NewOnCompleted() {
                onComplete?.Invoke();
                uiService.ActiveOnlyGameUI(false, YisoGameUITypes.FLOATING_TEXT);
            }
        }
        public void OnDestroy() { }
        public bool IsReady() => true;
        public bool IsActive() => true;
        private YisoGameUIService() {
            uiService = YisoServiceProvider.Instance.Get<IYisoUIService>();
        }

        internal static YisoGameUIService CreateService() => new();
    }
}