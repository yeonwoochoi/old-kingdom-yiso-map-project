using Character.Core;
using UnityEngine;

namespace Tools.Cutscene.Actions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Action/CutsceneActionKill")]
    public class YisoCutsceneActionKill : YisoCutsceneAction {
        public YisoCharacter[] characters;

        public override void PerformAction() {
            if (characters != null && characters.Length > 0) {
                foreach (var character in characters) {
                    if (character.gameObject.activeInHierarchy && character.conditionState.CurrentState !=
                        YisoCharacterStates.CharacterConditions.Dead) {
                        character.characterHealth.Dead();
                    }
                }
            }
        }
    }
}