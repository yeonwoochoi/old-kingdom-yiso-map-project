using System;

namespace Core.Service.UI.Event {
    public class GameUIDialogueEventArgs : EventArgs {
        public string Speaker { get; }
        public string[] Contents { get; }
        public Action OnComplete { get; set; }

        public GameUIDialogueEventArgs(string speaker, string[] contents, Action onComplete) {
            Speaker = speaker;
            Contents = contents;
            OnComplete = onComplete;
        }
    }

    public abstract class GameUIEventArgs : EventArgs {
        public string Title { get; }
        public bool NeedHold { get; }
        public Action OnClick { get; set; }

        protected GameUIEventArgs(string title, bool needHold, Action onClick) {
            Title = title;
            NeedHold = needHold;
            OnClick = onClick;
        }
    }

    public class GameUIBasicKeyEventArgs : GameUIEventArgs {
        public GameUIBasicKeyEventArgs(Action onClick) : base("이동 및 공격", true, onClick) { }
    }
}