using Core.Domain.Actor.Attack;
using Core.Service;

namespace Core.Domain.Entity {
    public interface IYisoEntity {
        public int GetId();
    }

    public interface IYisoHealthEntity : IYisoEntity {
        public int GetMaxHp();
        public int GetHp();
    }

    public interface IYisoCombatableEntity : IYisoHealthEntity {
        public double GetCombatRating();
        public YisoAttack CreateAttack(IYisoEntity entity);
        public YisoAttack CreateSkillAttack(IYisoEntity entity, int skillId);
        public YisoAttackResult GetAttack(YisoAttack attack);

        public double CalculateTakeMore(double otherCR) => 1 + ((GetCombatRating() - otherCR) / 117);
    }
}