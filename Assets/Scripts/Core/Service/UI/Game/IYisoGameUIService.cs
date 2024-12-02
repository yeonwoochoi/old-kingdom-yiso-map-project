using Core.Service.UI.Event;
using UnityEngine.Events;

namespace Core.Service.UI.Game {
    public interface IYisoGameUIService : IYisoSubUIService {
        void RegisterOnDialogue(UnityAction<GameUIDialogueEventArgs> handler);
        void UnregisterOnDialogue(UnityAction<GameUIDialogueEventArgs> handler);
        void RegisterOnGame(UnityAction<GameUIEventArgs> handler);
        void UnregisterOnGame(UnityAction<GameUIEventArgs> handler);
        void RaiseDialogue(GameUIDialogueEventArgs args);
        void RaiseGame(GameUIEventArgs args);
        public void FloatingText(string value, UnityAction onComplete = null);
        public void ShowStageLoadingComment(bool hideHud, int stage, UnityAction onComplete = null);
        public void ShowStoryClearPopup(UnityAction onClickBaseCamp, UnityAction onClickStory, UnityAction onClickClose = null, bool existNextStage = true);
    }
}