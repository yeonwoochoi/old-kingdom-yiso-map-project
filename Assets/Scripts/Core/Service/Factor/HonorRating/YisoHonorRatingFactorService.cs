using System.Collections.Generic;
using System.Linq;
using Core.Domain.Types;
using Core.Server.Domain;
using Core.Service.Server;
using Utils.Extensions;

namespace Core.Service.Factor.HonorRating {
    public class YisoHonorRatingFactorService : IYisoHonorRatingFactorService {
        private readonly IYisoServerService serverService;
        private readonly Dictionary<HonorRatingFactors, bool> initializes = new();
        
        private readonly Dictionary<int, (double, double)> stageHonorRatings = new();
        private readonly Dictionary<YisoEnemyTypes, double> enemyHonorRatingFactors = new();
        private readonly Dictionary<YisoEnemyTypes, double> enemyMaxHpFactors = new();

        private readonly YisoServerAllyHonorRatingFactors playerFactors = new();
        private readonly YisoServerAllyHonorRatingFactors allyFactors = new();
        private readonly YisoServerAllyHonorRatingFactors erryFactors = new();

        private readonly YisoServerItemHonorRatingFactors itemFactors = new();

        public bool IsReady() => initializes.Values.All(flag => flag);
        
        #region IMPLEMENTS
        
        public (double, double) GetStageHonorRating(int stageId) => stageHonorRatings[stageId];
        public double GetEnemyHonorRatingFactor(YisoEnemyTypes type) => enemyHonorRatingFactors[type];
        public double GetEnemyMaxHpFactor(YisoEnemyTypes type) => enemyMaxHpFactors[type];
        public YisoServerAllyHonorRatingFactors GetAllyFactors() => allyFactors;
        public YisoServerAllyHonorRatingFactors GetErryFactors() => erryFactors;
        public YisoServerAllyHonorRatingFactors GetPlayerFactors() => playerFactors;

        public YisoServerItemHonorRatingFactors GetItemFactors() => itemFactors;
        
        #endregion

        #region STAGE HR
        
        private void CalculateStageHonorRating() {
            if (serverService.IsReady()) {
                CalculateStageHonorRatingOnline();
            } else 
                CalculateStageHonorRatingOffline();

            initializes[HonorRatingFactors.STAGE] = true;
        }

        private void CalculateStageHonorRatingOnline() { }

        private void CalculateStageHonorRatingOffline() {
            for (var i = 1; i <= 100; i++) {
                var hrDiff = CalculateDiff(i);
                if (i == 1) {
                    stageHonorRatings[i] = (100, hrDiff);
                    continue;
                }

                var (beforeHR, beforeHRDiff) = stageHonorRatings[i - 1];
                stageHonorRatings[i] = (beforeHR + beforeHRDiff, hrDiff);
            }
            
            return;
            double CalculateDiff(int stage) => 100 + 1 * stage;
        }
        
        #endregion
        

        #region FACTORS
        
        private void LoadFactors() {
            if (serverService.IsReady()) LoadFactorsOnline();
            else LoadFactorsOffline();
        }
        
        private void LoadFactorsOnline() {
            initializes[HonorRatingFactors.ENEMY] = true;
        }

        private void LoadFactorsOffline() {
            SetEnemyHonorRatingFactorsOffline();
            SetPlayerFactorsOffline();
            SetErryFactorsOffline();
            SetAllyFactorOffline();
            SetItemFactorOffline();
        }

        private void SetEnemyHonorRatingFactorsOffline() {
            enemyHonorRatingFactors[YisoEnemyTypes.NORMAL] = 1d;
            enemyHonorRatingFactors[YisoEnemyTypes.ELITE] = 1.1d;
            enemyHonorRatingFactors[YisoEnemyTypes.FIELD_BOSS] = 1.5d;
            enemyHonorRatingFactors[YisoEnemyTypes.BOSS] = 2d;

            enemyMaxHpFactors[YisoEnemyTypes.NORMAL] = 1d;
            enemyMaxHpFactors[YisoEnemyTypes.ELITE] = 1d;
            enemyMaxHpFactors[YisoEnemyTypes.FIELD_BOSS] = 1d;
            enemyMaxHpFactors[YisoEnemyTypes.BOSS] = 1d;

            initializes[HonorRatingFactors.ENEMY] = true;
        }

        private void SetPlayerFactorsOffline() {
            playerFactors.honorRating = 0.1d;
            playerFactors.maxHp = 10d;
            initializes[HonorRatingFactors.PLAYER] = true;
        }

        private void SetErryFactorsOffline() {
            erryFactors.honorRating = 1.15d;
            erryFactors.maxHp = 1.5d;
            initializes[HonorRatingFactors.ERRY] = true;
        }

        private void SetAllyFactorOffline() {
            allyFactors.honorRating = 1d;
            allyFactors.maxHp = 1.5d;
            initializes[HonorRatingFactors.ALLY] = true;
        }

        private void SetItemFactorOffline() {
            itemFactors.honorRating = 0.9d;
            
            itemFactors.rankHonorRatingFactors.n = 0.8d;
            itemFactors.rankHonorRatingFactors.m = 1d;
            itemFactors.rankHonorRatingFactors.c = 1.5d;
            itemFactors.rankHonorRatingFactors.b = 2d;
            itemFactors.rankHonorRatingFactors.a = 3d;
            itemFactors.rankHonorRatingFactors.s = 6d;

            itemFactors.honorRatingErrorFactors.minError = 0.05d;
            itemFactors.honorRatingErrorFactors.maxError = 0.1d;
            
            initializes[HonorRatingFactors.ITEM] = true;
        }
        
        #endregion
        
        public void OnDestroy() { }

        private YisoHonorRatingFactorService(YisoServiceProvider provider) {
            serverService = provider.Get<IYisoServerService>();
            foreach (var factor in EnumExtensions.Values<HonorRatingFactors>()) {
                initializes[factor] = false;
            }
            
            CalculateStageHonorRating();
            LoadFactors();
        }

        internal static YisoHonorRatingFactorService CreateService(YisoServiceProvider provider) => new(provider);

        private enum HonorRatingFactors {
            STAGE,
            ENEMY,
            PLAYER,
            ALLY,
            ERRY,
            ITEM
        }
    }
}