using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Cutscene.Scripts.Control.Cutscene.Trigger
{
    public abstract class YisoBaseCutsceneTrigger: MonoBehaviour
    {
        public PlayableDirector director;
        protected bool IsDone => director.state == PlayState.Paused && director.time == 0f;
        protected bool isTyping = false;
        
        protected virtual void Start() {}
        protected virtual void OnDisable() { }

        protected void Pause()
        {
            if (director == null) return;
            if (!director.playableGraph.IsValid()) return;
            director.playableGraph.GetRootPlayable(0).SetSpeed(0);
        }

        protected virtual void Resume()
        {
            if (director == null) return;
            if (!director.playableGraph.IsValid()) return;
            if (!IsDone)
            {
                director.playableGraph.GetRootPlayable(0).SetSpeed(1);
            }
        }
        
        protected virtual void Init() {}
        
        protected virtual IEnumerator TypeSentence(string message, Text textUI, float typingSpeed = 1f)
        {
            if (isTyping) yield break;
            textUI.text = "";
            isTyping = true;
            foreach (var letter in message.ToCharArray())
            {
                textUI.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
            isTyping = false;
        }
    }
}