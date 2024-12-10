using Manager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Cutscene.Actions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Action/CutsceneActionAnimatorSetter")]
    public class YisoCutsceneActionAnimatorSetter : YisoCutsceneAction {
        public bool player;
        [ShowIf("@!player")] public Animator animator;
        public RuntimeAnimatorController animatorController;

        public override void PerformAction() {
            if (animatorController == null) return;
            if (!player && animator == null) return;
            if (player) {
                if (GameManager.HasInstance) {
                    GameManager.Instance.Player.ChangeAnimatorController(animatorController);
                }
            }
            else {
                animator.runtimeAnimatorController = animatorController;
            }
        }
    }
}