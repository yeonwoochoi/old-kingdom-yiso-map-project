using Core.Domain.Types;
using Core.Server.Domain;

namespace Core.Service.Factor.HonorRating {
    public interface IYisoHonorRatingFactorService : IYisoService {
        public (double, double) GetStageHonorRating(int stageId);
        public double GetEnemyHonorRatingFactor(YisoEnemyTypes type);
        public double GetEnemyMaxHpFactor(YisoEnemyTypes type);
        public YisoServerAllyHonorRatingFactors GetAllyFactors();
        public YisoServerAllyHonorRatingFactors GetErryFactors();
        public YisoServerAllyHonorRatingFactors GetPlayerFactors();
        public YisoServerItemHonorRatingFactors GetItemFactors();
    }
}