using System.Collections.Generic;
using UnityEngine;

namespace Tools.Cutscene.Actions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Action/CutsceneActionActivator")]
    public class YisoCutsceneActionActivator : YisoCutsceneAction {
        public List<GameObject> targetObjects;

        public override void PerformAction() {
            if (targetObjects == null || targetObjects.Count == 0) return;
            foreach (var targetObject in targetObjects) {
                targetObject.SetActive(true);
            }
        }
    }
}