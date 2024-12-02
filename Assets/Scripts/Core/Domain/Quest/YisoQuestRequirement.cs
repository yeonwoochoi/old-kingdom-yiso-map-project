using System;
using Core.Domain.Actor.Npc;
using Core.Domain.Actor.Player;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Service;
using Core.Service.Data;
using Core.Service.Data.Item;
using UnityEngine.Events;
using Utils.Extensions;

namespace Core.Domain.Quest {
    public abstract class YisoQuestRequirement {
        public Types Type { get; }
        public int Index { get; }

        protected YisoQuestRequirement(Types type, int index) {
            Type = type;
            Index = index;
        }

        public abstract bool Check();

        public abstract (int index, string target, string value, bool complete) GetObjectiveUI(YisoLocale.Locale locale);

        public abstract bool TryCheckAndUpdate(object value, YisoLocale.Locale locale, out QuestUpdateEventArgs.UpdateInfo updateInfo);
        
        public virtual void Setup(YisoPlayer player) { }
        
        public static Types ToType<T>() where T : YisoQuestRequirement {
            var type = typeof(T);
            if (type == typeof(YisoQuestEnemyRequirement)) return Types.ENEMY;
            if (type == typeof(YisoQuestItemRequirement)) return Types.ITEM;
            if (type == typeof(YisoQuestFieldEnterRequirement)) return Types.FIELD_ENTER;
            if (type == typeof(YisoQuestCompleteQuestRequirement)) return Types.COMPLETE_QUEST;
            if (type == typeof(YisoQuestPreCompleteQuestRequirement)) return Types.PRE_COMPLETE_QUEST;
            if (type == typeof(YisoQuestNpcRequirement)) return Types.NPC;
            throw new NotImplementedException(nameof(type));
        }

        public abstract void Clear();

        public enum Types {
            ITEM, ENEMY, 
            NPC,
            FIELD_ENTER,
            COMPLETE_QUEST,
            PRE_COMPLETE_QUEST,
            COMPLETE_INSTANT_CUTSCENE,
            ENTER_SUB_STAGE,
        }

        public enum UpdateStates {
            NOT_EXIST,
            COMPLETED,
            UPDATED,
            NOT_ENOUGH
        }
    }
    
    public static class YisoQuestRequirementTypeUtils {
        public static bool CanMultipleCheck(this YisoQuestRequirement.Types type) => type switch {
            YisoQuestRequirement.Types.ITEM => false,
            YisoQuestRequirement.Types.ENEMY => false,
            YisoQuestRequirement.Types.NPC => false,
            YisoQuestRequirement.Types.FIELD_ENTER => true,
            YisoQuestRequirement.Types.COMPLETE_QUEST => true,
            YisoQuestRequirement.Types.PRE_COMPLETE_QUEST => true,
            YisoQuestRequirement.Types.COMPLETE_INSTANT_CUTSCENE => true,
            YisoQuestRequirement.Types.ENTER_SUB_STAGE => true,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public class YisoQuestEnemyRequirement : YisoQuestRequirement {
        private readonly QuestEnemy enemy;

        public int EnemyId => enemy.EnemySO.id;
        
        public YisoQuestEnemyRequirement(QuestEnemy enemy, int index) : base(Types.ENEMY, index) {
            this.enemy = enemy;
        }

        public override bool Check() => enemy.Completed;

        public override (int index, string target, string value, bool complete) GetObjectiveUI(YisoLocale.Locale locale) {
            var (title, value, completed) = enemy.GetObjectiveUI(locale);
            return (Index, title, value, completed);
        }

        public override bool TryCheckAndUpdate(object value, YisoLocale.Locale locale, out QuestUpdateEventArgs.UpdateInfo updateInfo) {
            var enemyId = (int) value;
            var result = enemy.TryCheckUpdate(enemyId, locale, out updateInfo);
            return result;
        }

        public override void Clear() {
            enemy.Clear();
        }
    }
    
    public class YisoQuestItemRequirement : YisoQuestRequirement {
        public QuestItem QuestItem { get; }
        private YisoItem item = null;

        public YisoQuestItemRequirement(QuestItem questItem, int index) : base(Types.ITEM, index) {
            QuestItem = questItem;
        }

        public override void Setup(YisoPlayer player) {
            QuestItem.Setup(player);
        }

        public override bool Check() => QuestItem.Complete;
        public override (int index, string target, string value, bool complete) GetObjectiveUI(YisoLocale.Locale locale) {
            var (target, value, complete) = QuestItem.GetObjectiveUI(locale);
            return (Index, target, value, complete);
        }

        public override bool TryCheckAndUpdate(object value,YisoLocale.Locale locale, out QuestUpdateEventArgs.UpdateInfo updateInfo) {
            item ??= YisoServiceProvider.Instance.Get<IYisoItemService>().GetItemOrElseThrow(QuestItem.ItemId);

            var itemId = (int) value;
            var result = QuestItem.TryCheckUpdate(itemId, locale, out updateInfo);
            return result;
        }

        public override void Clear() {
            QuestItem.Clear();
        }
    }
    
    public class YisoQuestFieldEnterRequirement : YisoQuestRequirement {
        public QuestField Field { get; }
        public YisoQuestFieldEnterRequirement(QuestField field, int index) : base(Types.FIELD_ENTER, index) {
            Field = field;
        }

        public override bool Check() => Field.Entered;

        public override (int index, string target, string value, bool complete) GetObjectiveUI(YisoLocale.Locale locale) {
            var name = Field.GetFieldName(locale);
            var title = locale == YisoLocale.Locale.KR ? $"<b>{name}</b>으로 이동하기" : $"Move to <b>{name}</b>";
            var value = string.Empty;
            return (Index, title, value, Field.Entered);
        }

        public override bool TryCheckAndUpdate(object value, YisoLocale.Locale locale, out QuestUpdateEventArgs.UpdateInfo updateInfo) {
            var mapId = (int) value;
            var result = Field.TryCheckUpdate(mapId, locale, out updateInfo);
            return result;
        }

        public override void Clear() {
            Field.Clear();
        }
    }

    public class YisoQuestCompleteQuestRequirement : YisoQuestRequirement {
        private QuestCompleteQuest Quest { get; }

        public YisoQuestCompleteQuestRequirement(QuestCompleteQuest quest, int index) : base(Types.COMPLETE_QUEST,
            index) {
            Quest = quest;
        }

        public override bool Check() => Quest.Completed;

        public override (int index, string target, string value, bool complete) GetObjectiveUI(YisoLocale.Locale locale) {
            var (target, value, complete) = Quest.GetObjectiveUI(locale);
            return (Index, target, value, complete);
        }

        public override bool TryCheckAndUpdate(object value, YisoLocale.Locale locale, out QuestUpdateEventArgs.UpdateInfo updateInfo) {
            var questId = (int) value;
            return Quest.TryCheckUpdate(questId, locale, out updateInfo);
        }

        public override void Clear() {
            Quest.Clear();
        }
    }

    public class YisoQuestPreCompleteQuestRequirement : YisoQuestRequirement {
        private QuestCompleteQuest Quest { get; }

        public YisoQuestPreCompleteQuestRequirement(QuestCompleteQuest quest, int index) : base(Types.PRE_COMPLETE_QUEST,
            index) {
            Quest = quest;
        }

        public override bool Check() => Quest.Completed;

        public override (int index, string target, string value, bool complete) GetObjectiveUI(YisoLocale.Locale locale) {
            var (target, value, complete) = Quest.GetObjectiveUI(locale);
            return (Index, target, value, complete);
        }

        public override bool TryCheckAndUpdate(object value, YisoLocale.Locale locale, out QuestUpdateEventArgs.UpdateInfo updateInfo) {
            var questId = (int) value;
            return Quest.TryCheckUpdate(questId, locale, out updateInfo);
        }

        public override void Clear() {
            Quest.Clear();
        }
    }

    public class YisoQuestNpcRequirement : YisoQuestRequirement {
        private readonly int id;
        public YisoQuestNpcRequirement(YisoNpcSO so, int index) : base(Types.NPC, index) {
            id = so.id;
        }

        private bool completed = false;

        public override bool Check() => completed;

        public override (int index, string target, string value, bool complete) GetObjectiveUI(YisoLocale.Locale locale) {
            return (Index, string.Empty, string.Empty, false);
        }

        public override bool TryCheckAndUpdate(object value, YisoLocale.Locale locale, out QuestUpdateEventArgs.UpdateInfo updateInfo) {
            updateInfo = null;
            var npcId = (int) value;

            if (id != npcId || completed) return false;

            completed = true;

            updateInfo = new QuestUpdateEventArgs.UpdateInfo(UpdateStates.COMPLETED, "", "");
            return true;
        }

        public override void Clear() {
            completed = false;
        }
    }
}