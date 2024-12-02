using System;
using System.Collections.Generic;
using Core.Domain.Actor.Enemy.Drop;
using Core.Domain.Actor.Npc;
using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Actor.Enemy {
    [CreateAssetMenu(fileName = "Enemy", menuName = "Yiso/Actors/Enemy")]
    public class YisoEnemySO : YisoNpcSO {
        public YisoEnemyTypes type = YisoEnemyTypes.NORMAL;
        [Title("Drops")] public DropItem[] drops;

        public List<YisoEnemyBaseDrop> CreateDrops() {
            var result = new List<YisoEnemyBaseDrop>();

            foreach (var drop in drops) {
                YisoEnemyBaseDrop item = null;
                if (drop.money) item = new YisoEnemyMoneyDrop(drop.moneyValue);
                else item = new YisoEnemyItemDrop(drop.itemSO.id, drop.probability);
                result.Add(item);
            }

            return result;
        }


        [Serializable]
        public class DropItem {
            public bool money;
            [ShowIf("money")] [MinValue(1)] public int moneyValue = 1;
            [HideIf("money")] [Range(1, 100)] public int probability;
            [HideIf("money")] public YisoItemSO itemSO;
        }
    }
}