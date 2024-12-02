using Core.Domain.Item;
using Core.Domain.Types;

namespace Core.Service.Factor.Item {
    public interface IYisoItemFactorService : IYisoService {
        public YisoEquipFactions GetRandomFaction();
        public YisoEquipRanks GetRandomRank();
        public YisoEquipSlots GetRandomSlot();
        public (double baseBuyPrice, double totalBuyPrice, double sellPrice) GetPrices(int stageId, YisoItem item);
        public double CalculateMoney(int stageId);
        public double GetEnemyDropItemRate(YisoEnemyTypes type);
        public double GetEnemyMoneyDropRate(YisoEnemyTypes type);
    }
}