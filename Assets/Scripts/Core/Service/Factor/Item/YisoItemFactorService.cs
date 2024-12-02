using System.Collections.Generic;
using Core.Domain.Item;
using Core.Domain.Types;
using Core.Server.Domain;
using Core.Service.Server;
using UnityEngine;
using Utils;
using Utils.Extensions;

namespace Core.Service.Factor.Item {
    public class YisoItemFactorService : IYisoItemFactorService {
        private readonly IYisoServerService serverService;
        private bool ready = false;

        private readonly YisoServerItemFactors itemFactors = new();

        private Randomizer.WeightedRandomSelector<YisoEquipFactions> factionRandomSelector;
        private Randomizer.WeightedRandomSelector<YisoEquipRanks> rankRandomizer;
        private Randomizer.WeightedRandomSelector<YisoEquipSlots> slotRandomizer;

        private YisoItemFactorService() {
            serverService = YisoServiceProvider.Instance.Get<IYisoServerService>();
            LoadFactors();
        }
        
        #region IMPLEMENTS

        public YisoEquipFactions GetRandomFaction() => factionRandomSelector.GetRandomItem();
        public YisoEquipRanks GetRandomRank() => rankRandomizer.GetRandomItem();
        public YisoEquipSlots GetRandomSlot() => slotRandomizer.GetRandomItem();

        public (double baseBuyPrice, double totalBuyPrice, double sellPrice) GetPrices(int stageId, YisoItem item) {
            var basePrice = 1000 * Mathf.RoundToInt(Mathf.Pow((float) itemFactors.priceFactors.priceFactor, stageId - 1));
            if (item is not YisoEquipItem equipItem) return (basePrice, basePrice, basePrice);
            var totalPrice = basePrice
                             * itemFactors.priceFactors.ranks[equipItem.Rank]
                             * itemFactors.priceFactors.slots[equipItem.Slot]
                             * itemFactors.priceFactors.factions[equipItem.Faction];
            return (basePrice, totalPrice, totalPrice * itemFactors.priceFactors.sellFactor);
        }

        public double CalculateMoney(int stageId) {
            return itemFactors.dropFactors.moneyDropSumRate +
                   (double)Mathf.Pow((float) itemFactors.dropFactors.moneyDropBaseRate, stageId - 1);
        }

        public double GetEnemyDropItemRate(YisoEnemyTypes type) => itemFactors.dropFactors.dropFactors[type];

        public double GetEnemyMoneyDropRate(YisoEnemyTypes type) => itemFactors.dropFactors.moneyDropFactors[type];
        
        #endregion

        private void LoadFactors() {
            if (serverService.IsReady()) LoadFactorsOnline();
            else LoadFactorsOffline();
            
            ready = true;
        }

        private void LoadFactorsOnline() { }

        private void LoadFactorsOffline() {
            LoadRandomizerOffline();
            LoadPriceFactors();
            LoadDropFactors();
        }

        private void LoadRandomizerOffline() {
            factionRandomSelector = new Randomizer.WeightedRandomSelector<YisoEquipFactions>(
                new Dictionary<YisoEquipFactions, double> {
                    { YisoEquipFactions.COMMONER, 30 },
                    { YisoEquipFactions.THIEF, 1 },
                    { YisoEquipFactions.PIRATE, 3 },
                    { YisoEquipFactions.NAVY, 0.3 },
                    { YisoEquipFactions.KOREA_CORPORAL, 0.1 },
                    { YisoEquipFactions.BAEKJAE_LIEUTENANT, 0.01 },
                    { YisoEquipFactions.ANPAGYEON, 0.0001 }
                });

            rankRandomizer = new Randomizer.WeightedRandomSelector<YisoEquipRanks>(
                new Dictionary<YisoEquipRanks, double> {
                    { YisoEquipRanks.N, 40 },
                    { YisoEquipRanks.M, 10 },
                    { YisoEquipRanks.C, 1 },
                    { YisoEquipRanks.B, 2 },
                    { YisoEquipRanks.A, 0.2 },
                    { YisoEquipRanks.S, 0.01 },
                });

            slotRandomizer = new Randomizer.WeightedRandomSelector<YisoEquipSlots>(
                new Dictionary<YisoEquipSlots, double> {
                    { YisoEquipSlots.WEAPON, 20 },
                    { YisoEquipSlots.HAT, 14 },
                    { YisoEquipSlots.TOP, 15 },
                    { YisoEquipSlots.BOTTOM, 16 },
                    { YisoEquipSlots.SHOES, 17 },
                    { YisoEquipSlots.GLOVE, 18 },
                });
        }

        private void LoadPriceFactors() {
            itemFactors.priceFactors.priceFactor = 1.0725d;
            itemFactors.priceFactors.sellFactor = 0.65d;
            
            itemFactors.priceFactors.ranks[YisoEquipRanks.N] = 1d;
            itemFactors.priceFactors.ranks[YisoEquipRanks.M] = 1.2d;
            itemFactors.priceFactors.ranks[YisoEquipRanks.C] = 1.5d;
            itemFactors.priceFactors.ranks[YisoEquipRanks.B] = 2d;
            itemFactors.priceFactors.ranks[YisoEquipRanks.A] = 4d;
            itemFactors.priceFactors.ranks[YisoEquipRanks.S] = 6d;

            itemFactors.priceFactors.factions[YisoEquipFactions.COMMONER] = 1d;
            itemFactors.priceFactors.factions[YisoEquipFactions.THIEF] = 1d;
            itemFactors.priceFactors.factions[YisoEquipFactions.PIRATE] = 1d;
            itemFactors.priceFactors.factions[YisoEquipFactions.NAVY] = 1.2d;
            itemFactors.priceFactors.factions[YisoEquipFactions.KOREA_CORPORAL] = 1.2d;
            itemFactors.priceFactors.factions[YisoEquipFactions.BAEKJAE_LIEUTENANT] = 1.2d;
            itemFactors.priceFactors.factions[YisoEquipFactions.ANPAGYEON] = 6d;
            
            foreach (var slot in EnumExtensions.Values<YisoEquipSlots>()) {
                itemFactors.priceFactors.slots[slot] = slot == YisoEquipSlots.WEAPON ? 1.2d : 1d;
            }
        }

        private void LoadDropFactors() {
            itemFactors.dropFactors.moneyDropBaseRate = 1.0725d;
            itemFactors.dropFactors.moneyDropSumRate = 50;

            itemFactors.dropFactors.moneyDropFactors[YisoEnemyTypes.NORMAL] = 1d;
            itemFactors.dropFactors.moneyDropFactors[YisoEnemyTypes.ELITE] = 1.2d;
            itemFactors.dropFactors.moneyDropFactors[YisoEnemyTypes.FIELD_BOSS] = 2d;
            itemFactors.dropFactors.moneyDropFactors[YisoEnemyTypes.BOSS] = 3d;

            itemFactors.dropFactors.dropFactors[YisoEnemyTypes.NORMAL] = 0.5d;
            itemFactors.dropFactors.dropFactors[YisoEnemyTypes.ELITE] = 0.3d;
            itemFactors.dropFactors.dropFactors[YisoEnemyTypes.FIELD_BOSS] = 0.05d;
            itemFactors.dropFactors.dropFactors[YisoEnemyTypes.BOSS] = 0.01d;
        }

        public bool IsReady() => ready;

        public void OnDestroy() { }

        internal static YisoItemFactorService CreateService() => new();
    }
}