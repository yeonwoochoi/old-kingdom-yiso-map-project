using System;
using UnityEngine.SceneManagement;

namespace Core.Domain.Types {
    public enum YisoSceneTypes {
        NONE,
        INIT,
        UI,
        GAME,
        STORY,
        BASE_CAMP,
        COOP,
        BOUNTY
    }

    public static class YisoSceneTypesUtils {
        public static string ToBuildName(this YisoSceneTypes type) => type switch {
            YisoSceneTypes.INIT => "Init Scene",
            YisoSceneTypes.UI => "UI Scene",
            YisoSceneTypes.GAME => "Game Scene",
            YisoSceneTypes.STORY => "Story Scene",
            YisoSceneTypes.BASE_CAMP => "Base Camp Scene",
            YisoSceneTypes.BOUNTY => "Bounty Scene"
        };

        public static Scene ToScene(this YisoSceneTypes type) {
            var sceneName = type.ToBuildName();
            return SceneManager.GetSceneByName(sceneName);
        }

        public static bool IsCombatScene(this YisoSceneTypes type) =>
            type is YisoSceneTypes.COOP or YisoSceneTypes.GAME or YisoSceneTypes.STORY;

        public static bool ShowHUD(this YisoSceneTypes type) => type is not YisoSceneTypes.INIT;
    }
}