using System.Collections.Generic;
using Core.Behaviour;
using Manager_Temp_;
using Tools.Event;
using UnityEngine;

namespace Controller.Stage {
    [AddComponentMenu("Yiso/Controller/Quest/StageActionTrigger")]
    public class YisoStageActionTrigger : RunIBehaviour, IYisoEventListener<YisoStageChangeEvent> {
        public List<YisoStageAction> stageActions;
        [Range(1, 100)] public int stage = 1;

        public void OnEvent(YisoStageChangeEvent e) {
            if (e.currentStage.Id != stage) return;
            Initialization();
        }

        protected virtual void Initialization() {
            if (stageActions == null) return;
            stageActions.Sort((x, y) => x.priority.CompareTo(y.priority));
            foreach (var stageAction in stageActions) {
                stageAction.PerformAction();
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            this.YisoEventStartListening();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.YisoEventStopListening();
        }
    }
}