using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.Cutscene.Actions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Action/CutsceneActionInActivator")]
    public class YisoCutsceneActionInActivator : YisoCutsceneAction {
        public List<GameObject> targetObjects;
        public float delay = 0f;

        public override void PerformAction() {
            if (targetObjects == null || targetObjects.Count == 0) return;
            if (delay > 0f) {
                StartCoroutine(Inactivate());
            }
            else {
                foreach (var targetObject in targetObjects) {
                    targetObject.SetActive(false);
                }
            }
        }

        protected virtual IEnumerator Inactivate() {
            yield return new WaitForSeconds(delay);
            foreach (var targetObject in targetObjects) {
                targetObject.SetActive(false);
            }
        }
    }
}