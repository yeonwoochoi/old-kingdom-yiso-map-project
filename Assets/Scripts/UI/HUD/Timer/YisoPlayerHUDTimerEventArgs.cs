using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Events;

namespace UI.HUD.Timer {
    public sealed class YisoPlayerHUDTimerEventArgs {
        public float Time { get; }
        public IReadOnlyList<UnityAction> StartActions { get; }
        public IReadOnlyList<UnityAction<float>> ProgressActions { get; }
        public IReadOnlyList<UnityAction> CompleteActions { get; }
        
        public bool HideWhenDone { get; }

        public static ArgsBuilder Builder(float time) => new(time);

        private YisoPlayerHUDTimerEventArgs(float time, bool hideWhenDone,
            IReadOnlyList<UnityAction> onStartActions, IReadOnlyList<UnityAction<float>> onProgressActions, IReadOnlyList<UnityAction> onCompleteActions) {
            Time = time;
            HideWhenDone = hideWhenDone;
            StartActions = onStartActions;
            ProgressActions = onProgressActions;
            CompleteActions = onCompleteActions;
        }

        public class ArgsBuilder {
            private readonly float time;
            private bool hideWhenDone = true;
            private readonly List<UnityAction> onStarts = new();
            private readonly List<UnityAction<float>> onProgresses = new();
            private readonly List<UnityAction> onCompletes = new();

            public ArgsBuilder(float time) {
                this.time = time;
            }

            public ArgsBuilder AddOnStart([NotNull] UnityAction handler) {
                onStarts.Add(handler);
                return this;
            }

            public ArgsBuilder AddOnProgress([NotNull] UnityAction<float> handler) {
                onProgresses.Add(handler);
                return this;
            }

            public ArgsBuilder AddOnComplete([NotNull] UnityAction handler) {
                onCompletes.Add(handler);
                return this;
            }

            public ArgsBuilder HideWhenDone(bool hideWhenDone) {
                this.hideWhenDone = hideWhenDone;
                return this;
            }

            public YisoPlayerHUDTimerEventArgs Build() => new(time, hideWhenDone, onStarts, onProgresses, onCompletes);
        }
    }
}