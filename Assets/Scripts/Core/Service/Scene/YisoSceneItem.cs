using System;
using Core.Domain.Types;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Core.Service.Scene {
    public class YisoSceneItem {
        public YisoSceneTypes Type { get; }
        public LoadSceneMode Mode { get; }
        
        public bool AttachUI { get; }

        public YisoSceneTypes UnloadType { get; }

        public UnityAction OnStart { get; }

        public UnityAction<float> OnProgress { get; }

        public UnityAction OnComplete { get; set; }

        public Func<bool> OnAllowScenePredicate { get; }

        public bool AllowSceneActivation => OnAllowScenePredicate == null;

        public bool ExistUnloadScene => UnloadType != YisoSceneTypes.NONE;

        private YisoSceneItem(YisoSceneTypes type, LoadSceneMode mode, UnityAction onStart, UnityAction<float> onProgress, UnityAction onComplete, Func<bool> onAllowScenePredicate, YisoSceneTypes unloadType, bool attachUI) {
            Type = type;
            Mode = mode;
            OnStart = onStart;
            OnProgress = onProgress;
            OnComplete = onComplete;
            OnAllowScenePredicate = onAllowScenePredicate;
            UnloadType = unloadType;
            AttachUI = attachUI;
        }

        public static YisoSceneItemBuilder Builder(YisoSceneTypes type, LoadSceneMode mode = LoadSceneMode.Additive) => new(type, mode);
        
        public class YisoSceneItemBuilder {
            private readonly YisoSceneTypes type;
            private readonly LoadSceneMode mode;
            private YisoSceneTypes unloadType = YisoSceneTypes.NONE;
            private bool attachUI = true;
            private UnityAction onStart;
            private UnityAction<float> onProgress;
            private UnityAction onComplete;
            private Func<bool> onAllowScenePredicate;
            
            public YisoSceneItemBuilder(YisoSceneTypes type, LoadSceneMode mode = LoadSceneMode.Single) {
                this.type = type;
                this.mode = mode;
            }

            public YisoSceneItemBuilder UnloadType(YisoSceneTypes unloadType) {
                this.unloadType = unloadType;
                return this;
            }

            public YisoSceneItemBuilder OnStart(UnityAction onStart) {
                this.onStart = onStart;
                return this;
            }

            public YisoSceneItemBuilder OnProgress(UnityAction<float> onProgress) {
                this.onProgress = onProgress;
                return this;
            }

            public YisoSceneItemBuilder OnComplete(UnityAction onComplete) {
                this.onComplete = onComplete;
                return this;
            }

            public YisoSceneItemBuilder OnAllowScenePredicate(Func<bool> onAllowScenePredicate) {
                this.onAllowScenePredicate = onAllowScenePredicate;
                return this;
            }

            public YisoSceneItemBuilder AttachUI(bool attachUI) {
                this.attachUI = attachUI;
                return this;
            }

            public YisoSceneItem Build() => new(
                type,
                mode,
                onStart,
                onProgress,
                onComplete,
                onAllowScenePredicate,
                unloadType,
                attachUI
            );
        }
    }
}