using Character.Ability;
using Core.Service;
using Core.Service.Character;
using UnityEngine;

namespace Tools.AI.Decisions {
    [AddComponentMenu("Yiso/Character/AI/Decisions/AIDecisionCheckPetRegistry")]
    public class YisoAIDecisionCheckPetRegistry : YisoAIDecision {
        public override bool Decide() {
            return CheckIfPetIsRegistered();
        }

        protected virtual bool CheckIfPetIsRegistered() {
            if (brain.owner == null) return false;
            var stat = brain.owner.FindAbility<YisoCharacterStat>();
            if (stat == null) return false;
            return YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().PetModule
                .TryGet(stat.CombatStat.GetId(), out var pet);
        }
    }
}