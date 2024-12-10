using Manager;
using UnityEngine;

namespace Tools.Cutscene.Actions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Action/CutsceneActionMoveNextStage")]
    public class YisoCutsceneActionMoveNextStage : YisoCutsceneAction {
        public override void PerformAction() {
            if (StageManager.HasInstance) {
                StageManager.Instance.MoveToNextStage();
            }
        }
    }
}