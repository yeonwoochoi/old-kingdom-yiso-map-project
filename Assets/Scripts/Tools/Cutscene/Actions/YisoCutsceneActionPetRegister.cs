using Character.Ability;
using Character.Core;
using Core.Domain.Actor.Player.Modules.Pet;
using Core.Service;
using Core.Service.Character;
using Manager_Temp_.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Cutscene.Actions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Action/CutsceneActionPetRegister")]
    public class YisoCutsceneActionPetRegister : YisoCutsceneAction {
        public YisoCharacter[] characters;
        public bool register = true;

        public YisoPlayerPetModule PetModule =>
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().PetModule;

        public override void PerformAction() {
            if (characters == null || characters.Length == 0) return;
            foreach (var character in characters) {
                if (character == null || !character.gameObject.activeInHierarchy) continue;
                Register(character);
            }
        }

        protected virtual void Register(YisoCharacter targetCharacter) {
            var combatStat = targetCharacter.FindAbility<YisoCharacterStat>().CombatStat;
            if (register) {
                PetModule.Register(combatStat, true);
            }
            else {
                PetModule.Unregister(combatStat, true);
            }

            YisoPetEvent.Trigger(targetCharacter, combatStat.GetId(), register);
        }

        // [Button]
        // public virtual void Register() {
        //     if (character == null) return;
        //     var combatStat = character.FindAbility<YisoCharacterStat>().CombatStat;
        //     if (register) {
        //         PetModule.Register(combatStat, true);
        //     }
        //     else {
        //         PetModule.Unregister(combatStat, true);
        //     }
        //     YisoPetEvent.Trigger(character, combatStat.GetId(), register);
        // }
    }
}