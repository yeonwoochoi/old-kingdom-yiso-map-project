using Character.Ability.Skill;
using UnityEngine;

namespace Tools.AI.Actions.Attack {
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionSpinAttack")]
    public class YisoAIActionSpinAttack : YisoAIActionSkill {
        protected YisoCharacterSkillSpinAttack spinAttack;

        public override void Initialization() {
            base.Initialization();
            spinAttack = character?.FindAbility<YisoCharacterSkillSpinAttack>();
        }

        public override void OnEnterState() {
            base.OnEnterState();
            spinAttack.StartSkillCast(Target);
        }

        public override void OnExitState() {
            base.OnExitState();
            spinAttack.StopSkillCast();
        }

        public override void PerformAction() {
        }
    }
}