using System;
using Core.Domain.Types;
using Newtonsoft.Json;

namespace Core.Server.Domain {
    [Serializable]
    public class YisoServerCombatFactors {
        [JsonProperty("take_more_rate")]
        public double takeMoreRate;

        [JsonProperty("enemy_combat_factors")]
        public YisoServerEnemyCombatFactors enemyCombatFactors = new();
    }
    
    [Serializable]
    public class YisoServerEnemyCombatFactors {
        [JsonProperty("player_to_enemy_damage_rate")]
        public YisoServerEnemyFactors playerToEnemyDamageRate = new();
        [JsonProperty("ally_to_enemy_damage_rate")]
        public YisoServerEnemyFactors allyToEnemyDamageRate = new();
        [JsonProperty("enemy_to_player_damage_rate")]
        public YisoServerEnemyFactors enemyToPlayerDamageRate = new();
        [JsonProperty("enemy_to_ally_damage_rate")]
        public YisoServerEnemyFactors enemyToAllyDamageRate = new();
    }
}