using System.Collections;
using System.Collections.Generic;
using Character.Core;
using UnityEngine;

namespace Tools.Cutscene.Actions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Action/CutsceneActionCharacterFreezer")]
    public class YisoCutsceneActionCharacterFreezer : YisoCutsceneAction {
        public List<YisoCharacter> targetCharacters;
        public float delay = 0f;
        public bool freeze = false;

        public override void PerformAction() {
            if (targetCharacters == null || targetCharacters.Count == 0) return;
            if (delay > 0f) {
                StartCoroutine(FreezeCo());
            }
            else {
                Freeze();
            }
        }

        protected virtual IEnumerator FreezeCo() {
            yield return new WaitForSeconds(delay);
            Freeze();
        }

        protected virtual void Freeze() {
            foreach (var character in targetCharacters) {
                if (freeze) {
                    character.Freeze(YisoCharacterStates.FreezePriority.CutsceneAction);
                }
                else {
                    character.UnFreeze(YisoCharacterStates.FreezePriority.CutsceneAction | YisoCharacterStates.FreezePriority.Respawn);
                }
            }
        }
    }
}