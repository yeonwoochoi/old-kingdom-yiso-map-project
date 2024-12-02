using System;
using Newtonsoft.Json;

namespace Core.Server.Domain {
    [Serializable]
    public class YisoServerItemFactors {
        [JsonProperty("random_factors")]
        public YisoServerItemRandomFactors randomFactors = new();

        [JsonProperty("price_factors")]
        public YisoServerItemPriceFactors priceFactors = new();

        [JsonProperty("drop_factors")]
        public YisoServerItemDropFactors dropFactors = new();
    }

    [Serializable]
    public class YisoServerItemRandomFactors {
        public YisoServerItemSlotFactors slots = new();
        public YisoServerItemFactionFactors factions = new();
        public YisoServerItemRankFactors ranks = new();
    }

    [Serializable]
    public class YisoServerItemPriceFactors {
        [JsonProperty("price_factor")]
        public double priceFactor;

        [JsonProperty("sell_factor")]
        public double sellFactor;
        
        
        public YisoServerItemSlotFactors slots = new();
        public YisoServerItemFactionFactors factions = new();
        public YisoServerItemRankFactors ranks = new();
    }

    [Serializable]
    public class YisoServerItemDropFactors {
        [JsonProperty("money_drop_base_rate")]
        public double moneyDropBaseRate;
        [JsonProperty("money_drop_sum_rate")]
        public double moneyDropSumRate;

        [JsonProperty("money_drop_factors")]
        public YisoServerEnemyFactors moneyDropFactors = new(); // apply money factor
        [JsonProperty("drop_factors")]
        public YisoServerEnemyFactors dropFactors = new(); // drop probs
    }
}