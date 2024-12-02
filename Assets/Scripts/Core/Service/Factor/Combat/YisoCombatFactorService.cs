using Core.Domain.Types;
using Core.Server.Domain;
using Core.Service.Server;

namespace Core.Service.Factor.Combat {
    public class YisoCombatFactorService : IYIsoCombatFactorService {
        private readonly IYisoServerService serverService;
        private bool ready = false;
        public bool IsReady() => ready;

        private readonly YisoServerCombatFactors combatFactors = new();

        #region IMPLEMENTS

        public double CalculateTakeMore(double fromHR, double toHR) => 1 + ((fromHR - toHR) / combatFactors.takeMoreRate);

        public double GetPlayerToEnemyDamageRate(YisoEnemyTypes type) =>
            combatFactors.enemyCombatFactors.playerToEnemyDamageRate[type];

        public double GetAllyToEnemyDamageRate(YisoEnemyTypes type) =>
            combatFactors.enemyCombatFactors.allyToEnemyDamageRate[type];

        public double GetEnemyToPlayerDamageRate(YisoEnemyTypes type) =>
            combatFactors.enemyCombatFactors.enemyToPlayerDamageRate[type];

        public double GetEnemyToAllyDamageRate(YisoEnemyTypes type) =>
            combatFactors.enemyCombatFactors.enemyToAllyDamageRate[type];

        #endregion
        
        private void LoadFactors() {
            if (serverService.IsReady()) 
                LoadFactorsOnline();
            else LoadFactorsOffline();

            ready = true;
        }

        private void LoadFactorsOnline() { }

        private void LoadFactorsOffline() {
            combatFactors.takeMoreRate = 117;

            combatFactors.enemyCombatFactors.playerToEnemyDamageRate[YisoEnemyTypes.NORMAL] = 25d;
            combatFactors.enemyCombatFactors.playerToEnemyDamageRate[YisoEnemyTypes.ELITE] = 17d;
            combatFactors.enemyCombatFactors.playerToEnemyDamageRate[YisoEnemyTypes.FIELD_BOSS] = 10d;
            combatFactors.enemyCombatFactors.playerToEnemyDamageRate[YisoEnemyTypes.BOSS] = 5d;

            combatFactors.enemyCombatFactors.allyToEnemyDamageRate[YisoEnemyTypes.NORMAL] = 15d;
            combatFactors.enemyCombatFactors.allyToEnemyDamageRate[YisoEnemyTypes.ELITE] = 12d;
            combatFactors.enemyCombatFactors.allyToEnemyDamageRate[YisoEnemyTypes.FIELD_BOSS] = 8d;
            combatFactors.enemyCombatFactors.allyToEnemyDamageRate[YisoEnemyTypes.BOSS] = 3d;

            combatFactors.enemyCombatFactors.enemyToPlayerDamageRate[YisoEnemyTypes.NORMAL] = 13d;
            combatFactors.enemyCombatFactors.enemyToPlayerDamageRate[YisoEnemyTypes.ELITE] = 17d;
            combatFactors.enemyCombatFactors.enemyToPlayerDamageRate[YisoEnemyTypes.FIELD_BOSS] = 20d;
            combatFactors.enemyCombatFactors.enemyToPlayerDamageRate[YisoEnemyTypes.BOSS] = 25d;

            combatFactors.enemyCombatFactors.enemyToAllyDamageRate[YisoEnemyTypes.NORMAL] = 15d;
            combatFactors.enemyCombatFactors.enemyToAllyDamageRate[YisoEnemyTypes.ELITE] = 20d;
            combatFactors.enemyCombatFactors.enemyToAllyDamageRate[YisoEnemyTypes.FIELD_BOSS] = 25d;
            combatFactors.enemyCombatFactors.enemyToAllyDamageRate[YisoEnemyTypes.BOSS] = 35d;
        }
        
        private YisoCombatFactorService() {
            serverService = YisoServiceProvider.Instance.Get<IYisoServerService>();
            LoadFactors();
        }
        
        internal static YisoCombatFactorService CreateService() => new();

        public void OnDestroy() { }
    }
}