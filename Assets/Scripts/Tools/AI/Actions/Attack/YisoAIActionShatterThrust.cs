using Character.Ability.Skill;
using UnityEngine;

namespace Tools.AI.Actions.Attack {
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionShatterThrust")]
    public class YisoAIActionShatterThrust : YisoAIActionSkill {
        protected YisoCharacterSkillShatterThrust shatterThrust;

        public override void Initialization() {
            base.Initialization();
            shatterThrust = character?.FindAbility<YisoCharacterSkillShatterThrust>();
        }

        public override void OnEnterState() {
            base.OnEnterState();
            shatterThrust.StartSkillCast(Target);
        }

        public override void OnExitState() {
            base.OnExitState();
            shatterThrust.StopSkillCast();
        }

        public override void PerformAction() {
        }
    }
}