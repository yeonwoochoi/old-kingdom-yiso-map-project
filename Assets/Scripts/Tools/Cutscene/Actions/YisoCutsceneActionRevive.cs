using Character.Core;
using Core.Service;
using Core.Service.UI.HUD;
using UnityEngine;

namespace Tools.Cutscene.Actions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Action/CutsceneActionRevive")]
    public class YisoCutsceneActionRevive : YisoCutsceneAction {
        public YisoCharacter[] characters;

        public override void PerformAction() {
            if (characters != null && characters.Length > 0) {
                foreach (var character in characters) {
                    if (character.gameObject.activeInHierarchy && character.conditionState.CurrentState ==
                        YisoCharacterStates.CharacterConditions.Dead) {
                        character.RespawnAt(YisoCharacter.FacingDirections.South, true);
                    }
                }

                YisoServiceProvider.Instance.Get<IYisoHUDUIService>().SwitchToAttack();
            }
        }
    }
}