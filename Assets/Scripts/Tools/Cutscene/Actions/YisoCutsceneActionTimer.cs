using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Cutscene.Actions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Action/CutsceneActionTimer")]
    public class YisoCutsceneActionTimer: YisoCutsceneAction {
        public enum TimerActionType {
            Start, Pause, Resume, Stop
        }
        
        public TimerActionType actionType = TimerActionType.Start;
        [ShowIf("actionType", TimerActionType.Start)] public float time = 60f;
        [ShowIf("actionType", TimerActionType.Start)] public bool forceStart = false;
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoCutsceneActionTimer>();
        
        public override void PerformAction() {
            if (!TimeManager.HasInstance) {
                LogService.Warn("[YisoCutsceneActionTimer] There is no Time Manager.");
                return;
            }

            switch (actionType) {
                case TimerActionType.Start:
                    TimeManager.Instance.StartTimer(time, forceStart);
                    break;
                case TimerActionType.Pause:
                    TimeManager.Instance.PauseTimer();
                    break;
                case TimerActionType.Resume:
                    TimeManager.Instance.ResumeTimer();
                    break;
                case TimerActionType.Stop:
                    TimeManager.Instance.StopTimer();
                    break;
            }
        }
    }
}