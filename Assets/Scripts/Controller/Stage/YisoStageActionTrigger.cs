using System.Collections.Generic;
using System.Linq;
using Core.Behaviour;
using Tools.Event;
using UnityEngine;

namespace Controller.Stage {
    [AddComponentMenu("Yiso/Controller/Quest/StageActionTrigger")]
    public class YisoStageActionTrigger : RunIBehaviour, IYisoEventListener<YisoInGameEvent> {
        public List<YisoStageAction> stageActions;
        [Range(1, 100)] public int stage = 1;

        protected bool initialized = false;

        protected override void Awake() {
            base.Awake();
            Initialization();
        }

        public void OnEvent(YisoInGameEvent e) {
            if (e.stage == null || e.stage.Id != stage) return;
            TriggerActions(e.eventType);
        }

        protected virtual void Initialization() {
            if (initialized) return;
            if (stageActions == null) return;
            foreach (var stageAction in stageActions) {
                stageAction.Initialization();
            }
            initialized = true;
        }

        protected virtual void TriggerActions(YisoInGameEventTypes type) { 
            if (stageActions == null) return;
            if (!initialized) Initialization();

            // type과 매칭되는 action만 필터링
            var filteredActions = stageActions
                .Where(stageAction => (stageAction.eventTypes & type) != 0)
                .ToList();

            // 우선순위에 따라 정렬
            filteredActions.Sort((x, y) => x.priority.CompareTo(y.priority));

            // 필터링된 액션 수행
            foreach (var stageAction in filteredActions) {
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