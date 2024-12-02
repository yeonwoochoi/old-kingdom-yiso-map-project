using Core.Domain.Types;

namespace Core.Service.Factor.Combat {
    public interface IYIsoCombatFactorService : IYisoService {
        public double CalculateTakeMore(double fromHR, double toHR);
        public double GetPlayerToEnemyDamageRate(YisoEnemyTypes type);
        public double GetAllyToEnemyDamageRate(YisoEnemyTypes type);
        public double GetEnemyToPlayerDamageRate(YisoEnemyTypes type);

        public double GetEnemyToAllyDamageRate(YisoEnemyTypes type);
    }
}