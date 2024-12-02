using Character.Ability.Skill;
using UnityEngine;

namespace Tools.AI.Actions.Attack {
    [AddComponentMenu("Yiso/Character/AI/Actions/AIActionTeleportAttack")]
    public class YisoAIActionTeleportAttack : YisoAIActionSkill {
        protected YisoCharacterSkillTeleportAttack teleportAttack;

        public override void Initialization() {
            if (!ShouldInitialize) return;
            base.Initialization();
            teleportAttack = character?.FindAbility<YisoCharacterSkillTeleportAttack>();
        }

        public override void OnEnterState() {
            base.OnEnterState();
            teleportAttack.StartSkillCast(Target);
        }

        public override void OnExitState() {
            base.OnExitState();
            teleportAttack.StopSkillCast();
        }

        public override void PerformAction() {
        }
    }
}