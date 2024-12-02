using System;
using Core.Domain.Types;
using Newtonsoft.Json;

namespace Core.Server.Domain {
    [Serializable]
    public class YisoServerHonorRatingFactors {
        [JsonProperty("player_factors")] public YisoServerAllyHonorRatingFactors playerFactors = new();
        [JsonProperty("ally_factors")] public YisoServerAllyHonorRatingFactors allyFactors = new();
        [JsonProperty("erry_factors")] public YisoServerAllyHonorRatingFactors erryFactors = new();
        
        
        [JsonProperty("enemy_honor_rating_factors")]
        public YisoServerEnemyHonorRatingFactors enemyHonorRatingFactors = new();
        [JsonProperty("enemy_max_hp_factors")]
        public YisoServerEnemyHonorRatingFactors enemyMaxHpFactors = new();
        
        [JsonProperty("item_honor_rating_factors")]
        public YisoServerItemHonorRatingFactors itemHonorRatingFactors = new();
    }
    
    [Serializable]
    public class YisoServerEnemyHonorRatingFactors {
        public double normal;
        public double elite;
        [JsonProperty("field_boss")]
        public double fieldBoss;
        public double boss;
    }
    
    [Serializable]
    public class YisoServerAllyHonorRatingFactors {
        [JsonProperty("honor_rating")]
        public double honorRating;
        [JsonProperty("max_hp")]
        public double maxHp;
    }

    [Serializable]
    public class YisoServerItemHonorRatingFactors {
        [JsonProperty("honor_rating")]
        public double honorRating;
        [JsonProperty("rank_honor_rating_factors")]
        public YisoServerItemRankHonorRatingFactors rankHonorRatingFactors = new();
        [JsonProperty("honor_rating_error_factors")]
        public YisoServerItemHonorRatingErrorFactors honorRatingErrorFactors = new();
    }

    [Serializable]
    public class YisoServerItemRankHonorRatingFactors {
        public double n;
        public double m;
        public double c;
        public double b;
        public double a;
        public double s;

        public double this[YisoEquipRanks rank] =>
            rank switch {
                YisoEquipRanks.N => n,
                YisoEquipRanks.M => m,
                YisoEquipRanks.C => c,
                YisoEquipRanks.B => s,
                YisoEquipRanks.A => a,
                YisoEquipRanks.S => s,
                _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
            };
    }

    [Serializable]
    public class YisoServerItemHonorRatingErrorFactors {
        [JsonProperty("min_error")]
        public double minError;
        [JsonProperty("max_error")]
        public double maxError;
    }
}