using System.Collections.Generic;
using Core.Behaviour;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Utils.Beagle;

namespace Tools.SceneLoading {
    public class YisoSceneLoadingAntiSpill {
        protected Scene antiSpillScene;
        protected Scene destinationScene;
        protected UnityAction<Scene, Scene> onActiveSceneChangedCallback;
        protected string sceneToLoadName;
        protected string antiSpillSceneName;
        protected List<GameObject> spillSceneRoots = new(50);
        protected static List<string> scenesInBuild;

        /// <summary>
        /// Creates the temporary scene
        /// </summary>
        /// <param name="sceneToLoadName"></param>
        /// <param name="antiSpillSceneName"></param>
        public virtual void PrepareAntiFill(string sceneToLoadName, string antiSpillSceneName = "") {
            destinationScene = default;
            this.sceneToLoadName = sceneToLoadName;

            // 없으면 만들어
            if (antiSpillSceneName == "") {
                antiSpillScene = SceneManager.CreateScene($"AntiSpill_{sceneToLoadName}");
                SetAntiFillSceneActive();
            }
            else {
                scenesInBuild = YisoSceneUtils.GetScenesInBuild();
                if (!scenesInBuild.Contains(antiSpillSceneName)) {
                    Debug.LogError(
                        $"MMSceneLoadingAntiSpill : impossible to load the '{antiSpillSceneName}' scene, there is no such scene in the project's build settings.");
                    return;
                }

                SceneManager.LoadScene(antiSpillSceneName, LoadSceneMode.Additive);
                antiSpillScene = SceneManager.GetSceneByName(antiSpillSceneName);
                this.antiSpillSceneName = antiSpillScene.name;

                // SceneManager.sceneLoaded : 새로운 Scene이 Load되고 활성화 되었을때 실행됨
                SceneManager.sceneLoaded += PrepareAntiFillOnSceneLoaded;
            }
        }

        /// <summary>
        /// Anti spill Scene을 새로 만들지 않았을 때 새로운 Scene이 활성화될 준비가 된 상태에서 한번 호출됨
        /// </summary>
        /// <param name="newScene"></param>
        /// <param name="mode"></param>
        protected virtual void PrepareAntiFillOnSceneLoaded(Scene newScene, LoadSceneMode mode) {
            if (newScene.name != antiSpillSceneName) return;
            SceneManager.sceneLoaded -= PrepareAntiFillOnSceneLoaded;
            SetAntiFillSceneActive();
        }

        /// <summary>
        /// Sets the anti spill scene active
        /// </summary>
        protected virtual void SetAntiFillSceneActive() {
            if (onActiveSceneChangedCallback != null) SceneManager.activeSceneChanged -= onActiveSceneChangedCallback;
            onActiveSceneChangedCallback = OnActiveSceneChanged;
            SceneManager.activeSceneChanged += onActiveSceneChangedCallback;
            SceneManager.SetActiveScene(antiSpillScene);
        }

        /// <summary>
        /// 이후 anti spill scene => destination scene 로 갈때 호출
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        protected virtual void OnActiveSceneChanged(Scene from, Scene to) {
            if (from == antiSpillScene) {
                SceneManager.activeSceneChanged -= onActiveSceneChangedCallback;
                onActiveSceneChangedCallback = null;
                EmptyAntiSpillScene();
            }
        }

        /// <summary>
        /// anti spill scene => destination scene
        /// </summary>
        protected virtual void EmptyAntiSpillScene() {
            if (antiSpillScene.IsValid() && antiSpillScene.isLoaded) {
                spillSceneRoots.Clear();
                antiSpillScene.GetRootGameObjects(spillSceneRoots); // spillSceneRoots에 씬의 모든 루트 게임 오브젝트를 추가함 

                destinationScene = SceneManager.GetSceneByName(sceneToLoadName);

                if (spillSceneRoots.Count > 0) {
                    if (destinationScene.IsValid() && destinationScene.isLoaded) {
                        foreach (var root in spillSceneRoots) {
                            SceneManager.MoveGameObjectToScene(root, destinationScene);
                        }
                    }
                }

                SceneManager.UnloadSceneAsync(antiSpillScene);
            }
        }
    }
}