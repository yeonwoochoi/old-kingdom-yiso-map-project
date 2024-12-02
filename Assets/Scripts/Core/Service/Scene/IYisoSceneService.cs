using System;
using Core.Domain.Types;
using UnityEngine.Events;

namespace Core.Service.Scene {
    public interface IYisoSceneService : IYisoService {
        public void LoadScene(YisoSceneTypes type, Action<bool> onFade = null);
        public YisoSceneTypes GetCurrentScene();
        public void SetCurrentScene(YisoSceneTypes type);
        public void LoadUIScene();
        public void RegisterOnSceneChanged(UnityAction<YisoSceneTypes, YisoSceneTypes> handler);
        public void UnregisterOnSceneChanged(UnityAction<YisoSceneTypes, YisoSceneTypes> handler);
    }
}