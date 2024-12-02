using Cutscene.Scripts.Control.Cutscene.Trigger;
using UnityEngine;
using UnityEngine.Playables;

namespace Cutscene.Scripts.Control.Cutscene.Utils {
    public class YisoCutsceneDirectorSetter : MonoBehaviour {
        public PlayableDirector cutscenePlayableDirector;
        private bool isSet = false;

        private void Awake() {
            SetDirector();
        }

        private void OnEnable() {
            SetDirector();
        }

        private void SetDirector() {
            if (isSet) return;
            isSet = true;
            var skipButtons = GetComponentsInChildren<YisoCutsceneSkipButtonController>();
            var playableDirectors = GetComponentsInChildren<YisoBaseCutsceneTrigger>();

            foreach (var director in playableDirectors) {
                if (director != null && cutscenePlayableDirector != null) {
                    director.director = cutscenePlayableDirector;
                }
            }

            foreach (var skipButton in skipButtons) {
                if (skipButton != null && cutscenePlayableDirector != null) {
                    skipButton.director = cutscenePlayableDirector;
                }
            }
        }
    }
}