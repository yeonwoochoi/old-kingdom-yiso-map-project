using System.Collections.Generic;
using Core.Domain.Actor.Ally;
using Core.Domain.Actor.Enemy;
using Core.Domain.Actor.Npc;
using Core.Domain.Actor.Player;
using Core.Domain.Drop;
using Core.Domain.Entity;

namespace Core.Service.Character {
    public interface IYisoCharacterService : IYisoService {
        YisoPlayer GetPlayer();
        YisoEnemy CreateEnemy(YisoEnemySO enemySO, bool developMode = true);
        public YisoAlly CreateAlly(YisoAllySO allySO, bool developMode = true);
        public YisoNpc CreateNpc(YisoNpcSO npcSO);
        public List<YisoDropItem> GetDropItems(IYisoEntity entity = null);
        public void ResetPlayerData();
        public void LoadPlayerData();
        public bool IsDataVersionMatched();
    }
}