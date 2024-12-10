using Controller.Map;
using Core.Behaviour;
using Manager;
using Tools.Event;
using UnityEngine;

namespace Controller.Bounty {
    [AddComponentMenu("Yiso/Controller/Bounty/Bounty Boss Register Listener")]
    public class YisoBountyBossRegisterListener: RunIBehaviour, IYisoEventListener<SpawnObjectEvent> {
        public void OnEvent(SpawnObjectEvent e) {
            RegisterBoss(e.spawnObj);
        }

        private void RegisterBoss(GameObject bossObj) {
            if (!BountyManager.HasInstance) return;
            BountyManager.Instance.RegisterBoss(bossObj);
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