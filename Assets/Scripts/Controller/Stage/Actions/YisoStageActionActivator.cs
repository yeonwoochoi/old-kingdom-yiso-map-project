using UnityEngine;

namespace Controller.Stage.Actions {
    [AddComponentMenu("Yiso/Controller/Quest/StageActionActivator")]
    public class YisoStageActionActivator : YisoStageAction {
        public GameObject[] targets;
        public bool active = true;

        public override void PerformAction() {
            if (targets == null || targets.Length == 0) return;
            foreach (var target in targets) {
                target.SetActive(active);
            }
        }
    }
}