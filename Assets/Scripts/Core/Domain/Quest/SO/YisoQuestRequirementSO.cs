using System;
using Core.Domain.Actor.Enemy;
using Core.Domain.Actor.Npc;
using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Quest.SO {
    [Serializable]
    public class YisoQuestRequirementSO {
        [Title("Basic")] public YisoQuestRequirement.Types type;
        [ShowIf("type", YisoQuestRequirement.Types.ITEM), Title("Item")]
        public Item item;

        [ShowIf("type", YisoQuestRequirement.Types.ENEMY), Title("Enemy")]
        public Enemy enemy;
        
        [ShowIf("type", YisoQuestRequirement.Types.FIELD_ENTER)]
        public Field field;

        [ShowIf("type", YisoQuestRequirement.Types.COMPLETE_QUEST)]
        public Quest quest;

        [ShowIf("type", YisoQuestRequirement.Types.PRE_COMPLETE_QUEST)]
        public Quest preCompleteQuest;

        [ShowIf("type", YisoQuestRequirement.Types.NPC)]
        public YisoNpcSO npcSO;

        

        public YisoQuestRequirement CreateRequirement(int index) {
            switch (type) {
                case YisoQuestRequirement.Types.ITEM:
                    return new YisoQuestItemRequirement(item.To(), index);
                case YisoQuestRequirement.Types.ENEMY:
                    return new YisoQuestEnemyRequirement(enemy.To(), index);
                case YisoQuestRequirement.Types.FIELD_ENTER:
                    return new YisoQuestFieldEnterRequirement(field.To(), index);
                case YisoQuestRequirement.Types.COMPLETE_QUEST:
                    return new YisoQuestCompleteQuestRequirement(quest.To(), index);
                case YisoQuestRequirement.Types.PRE_COMPLETE_QUEST:
                    return new YisoQuestPreCompleteQuestRequirement(preCompleteQuest.To(true), index);
                case YisoQuestRequirement.Types.NPC:
                    return new YisoQuestNpcRequirement(npcSO, index);
            }
            return null;
        }
        
        [Serializable]
        public class Item {
            [Required] public YisoItemSO itemSO;
            public QuestItem.RequirementType type;
            [MinValue(1)] public int value = 1;
            public QuestItem To() => new(itemSO, type, value);
        }

        [Serializable]
        public class Enemy {
            [Required] public YisoEnemySO enemySO;
            [MinValue(1)] public int count = 1;
            [Title("Cutscene")] public bool instantCutscene = false;
            [ShowIf("instantCutscene")] public GameObject linkedPrefab;
            

            public QuestEnemy To() {
                if (enemySO.type is YisoEnemyTypes.BOSS or YisoEnemyTypes.ELITE or YisoEnemyTypes.FIELD_BOSS) {
                    count = 1;
                }

                return new QuestEnemy(enemySO, count);
            }
        }

        [Serializable]
        public class Field {
            public int id;
            public YisoLocale fieldName;

            public QuestField To() {
                return new QuestField(this);
            }
        }

        [Serializable]
        public class Quest {
            public YisoQuestSO questSO;

            public QuestCompleteQuest To(bool preComplete = false) {
                return new QuestCompleteQuest(questSO, preComplete);
            }
        }
    }
}