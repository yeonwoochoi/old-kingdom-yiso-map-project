using System.Collections.Generic;
using Character.Weapon;
using Core.Domain.Actor.Player.SO;
using Core.Domain.Item;
using Core.Domain.Store;
using Core.Domain.Types;

namespace Core.Service.Data.Item {
    public interface IYisoItemService : IYisoService {
        public YisoItem GetItemOrElseThrow(int id);
        public YisoItemSO GetItemSOOrElseThrow(int id);
        public bool TryGetSetItem(int id, out YisoSetItem item);
        public bool TryGetStore(int id, out YisoStore store);
        public YisoItem CreateRandomItem(int stageId = -1);
        public YisoItem CreateRandomWeapon(YisoWeapon.AttackType type, YisoEquipRanks rank);
        public List<YisoItem> CreateItemFromSO(YisoPlayerInventoryItemsSO so);
        public YisoStore CreateDevStore(int stageId, int count = 15);
        public YisoItem CreateRandomItem(YisoEquipItem item);
        public YisoItem CreateRandomEquip(YisoEquipRanks rank);
    }
}