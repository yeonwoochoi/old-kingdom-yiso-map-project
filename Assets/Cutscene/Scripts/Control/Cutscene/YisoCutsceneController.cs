using Core.Domain.Locale;
using Manager;
using Tools.Cutscene;
using Tools.Event;
using UnityEngine;
using UnityEngine.Playables;

namespace Cutscene.Scripts.Control.Cutscene
{ 
    public struct YisoCutsceneStateChangeEvent
    {
        public string cutsceneName;
        public YisoCutsceneTrigger.CutsceneState cutsceneState;
        public YisoCutsceneStateChangeEvent(string newName, YisoCutsceneTrigger.CutsceneState state)
        {
            cutsceneName = newName;
            cutsceneState = state;
        }
        static YisoCutsceneStateChangeEvent e;
        public static void Trigger(string newName, YisoCutsceneTrigger.CutsceneState state)
        {
            e.cutsceneName = newName;
            e.cutsceneState = state;
            YisoEventManager.TriggerEvent(e);
        }
    }
    
    public class YisoCutsceneController: MonoBehaviour
    {
        public enum ControlType
        {
            Space, Touch, Auto
        }

        public PlayableDirector director;

        public static YisoLocale.Locale locale = YisoLocale.Locale.KR;
        public static ControlType controlType = ControlType.Space;

        protected bool wasPaused = false;

        protected virtual void Awake() {
            wasPaused = false;
            YisoCutsceneStateChangeEvent.Trigger(gameObject.scene.name, YisoCutsceneTrigger.CutsceneState.Play);
            if (director.state != PlayState.Playing) {
                director.Play();   
            }
        }

        protected virtual void OnEnable()
        {
            director.played += OnResumed;
            director.paused += OnPaused;
            director.stopped += OnStopped;
        }
        protected virtual void OnDisable()
        {
            director.played -= OnResumed;
            director.paused -= OnPaused;
            director.stopped -= OnStopped;
        }

        private void OnResumed(PlayableDirector playableDirector) {
            if (!wasPaused) return;
            YisoCutsceneStateChangeEvent.Trigger(gameObject.scene.name, YisoCutsceneTrigger.CutsceneState.Play);
            wasPaused = false;
        }

        private void OnPaused(PlayableDirector playableDirector) {
            wasPaused = true;
            YisoCutsceneStateChangeEvent.Trigger(gameObject.scene.name, YisoCutsceneTrigger.CutsceneState.Pause);
        }

        private void OnStopped(PlayableDirector playableDirector)
        {
            YisoCutsceneStateChangeEvent.Trigger(gameObject.scene.name, YisoCutsceneTrigger.CutsceneState.Stop);
            StartCoroutine(GameManager.Instance.UnloadCutsceneAdditive());
        }
    }
}