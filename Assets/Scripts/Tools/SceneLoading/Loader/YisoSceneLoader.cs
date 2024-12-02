using Core.Behaviour;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tools.SceneLoading.Loader {
    [AddComponentMenu("Yiso/Tools/Scene/SceneLoader")]
    public class YisoSceneLoader : RunIBehaviour {
        public enum LoadingSceneModes {
            UnityNative,
            Single,
            Additive
        }

        public string sceneName;
        public LoadingSceneModes loadingSceneMode = LoadingSceneModes.Single;

        public virtual void LoadScene() {
            switch (loadingSceneMode) {
                case LoadingSceneModes.UnityNative:
                    SceneManager.LoadScene(sceneName);
                    break;
                case LoadingSceneModes.Single:
                    YisoSingleSceneLoader.LoadScene(sceneName);
                    break;
                case LoadingSceneModes.Additive:
                    YisoAdditiveSceneLoader.LoadScene(sceneName);
                    break;
            }
        }
    }
}