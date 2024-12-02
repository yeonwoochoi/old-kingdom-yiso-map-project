using Character.Health;
using UnityEngine;

namespace Tools.Cutscene.Actions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Action/CutsceneActionImmortal")]
    public class YisoCutsceneActionImmortal: YisoCutsceneAction {
        public YisoHealth[] targetHealth;
        public bool immortal = true;
        
        public override void PerformAction() {
            if (targetHealth == null || targetHealth.Length == 0) return;
            foreach (var health in targetHealth) {
                health.isPermanentlyInvulnerable = immortal;
            }
        }
    }
}