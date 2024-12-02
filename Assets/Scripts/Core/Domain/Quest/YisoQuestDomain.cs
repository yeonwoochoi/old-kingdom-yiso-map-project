using System;
using System.Collections.Generic;
using Core.Domain.Actor.Enemy;
using Core.Domain.Actor.Player;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Domain.Quest.SO;
using Core.Service;
using Core.Service.Character;
using Core.Service.Data;
using UnityEngine;
using Utils.Extensions;

namespace Core.Domain.Quest {
    public class QuestItem {
        private readonly YisoLocale name;
        private readonly RequirementType type;

        private int currentValue = 0;
        private int startQuantity = 0;
        
        public Sprite ItemIcon { get; }

        public int Value { get; }

        public int ItemId { get; }

        private readonly Dictionary<RequirementType, bool> achieves = new();

        private YisoPlayer player;
        
        public bool Complete => achieves[type];

        public void Clear() {
            currentValue = 0;
            foreach (var req in EnumExtensions.Values<RequirementType>())
                achieves[req] = false;
        }

        public QuestItem(YisoItemSO so, RequirementType type, int value) {
            ItemIcon = so.icon;
            ItemId = so.id;
            name = so.name;
            this.type = type;
            Value = value;

            foreach (var req in EnumExtensions.Values<RequirementType>()) {
                achieves[req] = false;
            }
        }

        public void Setup(YisoPlayer player) {
            this.player = player;
            var inventory = player.InventoryModule;
            var itemExist = inventory.TryFindItemById(ItemId, out var item);
            startQuantity = itemExist ? item.Quantity : 0;
            if (type == RequirementType.DROP)
                startQuantity = -Value;
        }

        public bool TryCheckUpdate(int itemId, YisoLocale.Locale locale, out QuestUpdateEventArgs.UpdateInfo info) {
            info = null;

            if (itemId != ItemId || achieves[type]) return false;
            
            var complete = false;
            var inventory = player.InventoryModule;
            var itemExist = inventory.TryFindItemById(itemId, out var item);
            var quantity = itemExist ? item.Quantity : 0;
            switch (type) {
                case RequirementType.ACQUIRE:
                    if (!itemExist) throw new Exception($"Item(id={itemId}) not found");
                    currentValue = quantity - startQuantity;
                    break;
                case RequirementType.DROP:
                    currentValue = Mathf.Abs(startQuantity + quantity);
                    break;
                default:
                    info = null;
                    break;
            }

            complete = currentValue == Value;
            var updateState = complete
                ? YisoQuestRequirement.UpdateStates.COMPLETED
                : YisoQuestRequirement.UpdateStates.UPDATED;
            
            var updateValue = GetObjectiveValue();
            var objectiveTarget = CombineObjectiveTarget(locale);
            info = new QuestUpdateEventArgs.UpdateInfo(updateState, objectiveTarget, updateValue);
            achieves[type] = complete;
            return info != null;
        }

        public (string objectiveTarget, string objectiveValue, bool complete) GetObjectiveUI(YisoLocale.Locale locale) {
            var target = CombineObjectiveTarget(locale);
            var value = GetObjectiveValue();
            return (target, value, Complete);
        }

        private string CombineObjectiveTarget(YisoLocale.Locale locale) {
            var itemName = name[locale];
            return type switch {
                RequirementType.DROP => locale == YisoLocale.Locale.KR ? $"{itemName} 버리기" : $"Drop {itemName}",
                RequirementType.REINFORCE => locale == YisoLocale.Locale.KR ? $"{itemName} 강화" : $"Reinforce {itemName}",
                RequirementType.ACQUIRE => locale == YisoLocale.Locale.KR ? $"{itemName} 획득" : $"Get {itemName}",
                RequirementType.USE => locale == YisoLocale.Locale.KR ? $"{itemName} 사용" : $"Use {itemName}",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private string GetObjectiveValue() {
            switch (type) {
                case RequirementType.DROP:
                case RequirementType.REINFORCE:
                case RequirementType.USE:
                    return string.Empty;
            }

            return $"({currentValue}/{Value})";
        }

        public enum RequirementType {
            DROP,
            REINFORCE,
            ACQUIRE,
            USE
        }

        public void Reset() {
            currentValue = 0;
            foreach (var req in EnumExtensions.Values<RequirementType>())
                achieves[req] = false;
        }
    }

    public class QuestEnemy {
        private readonly int count;
        private int currentCount = 0;

        public int Id => EnemySO.id;
        public bool Completed { get; private set; }
        
        public YisoEnemySO EnemySO { get; }

        public QuestEnemy(YisoEnemySO so, int count) {
            EnemySO = so;
            this.count = count;
        }

        public void Clear() {
            currentCount = 0;
            Completed = false;
        }

        public bool TryCheckUpdate(int enemyId, YisoLocale.Locale locale, out QuestUpdateEventArgs.UpdateInfo info) {
            info = null;

            if (enemyId == Id && !Completed) {
                currentCount++;
                Completed = currentCount == count;
                var state = Completed
                    ? YisoQuestRequirement.UpdateStates.COMPLETED
                    : YisoQuestRequirement.UpdateStates.UPDATED;
                var objectiveValue = GetObjectiveValue();
                var objectiveTarget = GetObjectiveTarget(locale);
                info = new QuestUpdateEventArgs.UpdateInfo(state, objectiveTarget, objectiveValue);
            }
            
            return info != null;
        }
        
        public (string objectiveTarget, string objectiveValue, bool complete) GetObjectiveUI(YisoLocale.Locale locale) {
            var uiCount = GetObjectiveValue();
            var uiTarget = GetObjectiveTarget(locale);
            return (uiTarget, uiCount, Completed);
        }
        
        private string GetObjectiveValue() => $"({currentCount}/{count})";
        
        private string GetObjectiveTarget(YisoLocale.Locale locale) {
            var target = EnemySO.name[locale];
            return locale == YisoLocale.Locale.KR ? $"{target} 처치" : $"Kill {target}";
        }
        
        public void Reset() {
            currentCount = 0;
        }
    }
    
    public class QuestField {
        private readonly int fieldId;
        private readonly YisoLocale fieldName;

        public bool Entered { get; private set; } = false;

        public QuestField(YisoQuestRequirementSO.Field field) {
            fieldId = field.id;
            fieldName = field.fieldName;
        }

        public string GetFieldName(YisoLocale.Locale locale) => fieldName[locale];

        public bool TryCheckUpdate(int fieldId, YisoLocale.Locale locale, out QuestUpdateEventArgs.UpdateInfo updateInfo) {
            updateInfo = null;

            if (this.fieldId != fieldId || Entered) return false;
            
            Entered = true;
            updateInfo = new QuestUpdateEventArgs.UpdateInfo(YisoQuestRequirement.UpdateStates.COMPLETED, GetObjectiveTarget(locale), "");

            return true;
        }

        public void Clear() {
            Entered = false;
        }
        
        private string GetObjectiveTarget(YisoLocale.Locale locale) {
            var target = GetFieldName(locale);
            return locale == YisoLocale.Locale.KR ? $"{target}로 이동" : $"Move to {target}";
        }
    }

    public class QuestCompleteQuest {
        public int Id { get; }

        private readonly YisoLocale questName;

        public bool Completed { get; private set; } = false;

        private readonly bool preComplete;

        public QuestCompleteQuest(YisoQuestSO questSO, bool preComplete = false) {
            this.preComplete = preComplete;
            Id = questSO.id;
            questName = questSO.name;
        }

        public void Clear() {
            Completed = false;
        }

        public bool TryCheckUpdate(int questId, YisoLocale.Locale locale, out QuestUpdateEventArgs.UpdateInfo info) {
            info = null;

            if (questId == Id && !Completed) {
                var player = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer();
                Completed = preComplete ? player.QuestModule.IsQuestPreComplete(questId) : player.QuestModule.IsQuestComplete(questId);
                var state = Completed
                    ? YisoQuestRequirement.UpdateStates.COMPLETED
                    : YisoQuestRequirement.UpdateStates.UPDATED;
                var objectiveValue = GetObjectiveValue(locale);
                var objectiveTarget = GetObjectiveTarget(locale);
                info = new QuestUpdateEventArgs.UpdateInfo(state, objectiveTarget, objectiveValue);
            }
            
            return info != null;
        }

        public (string objectiveTarget, string objectiveValue, bool complete) GetObjectiveUI(YisoLocale.Locale locale) {
            var target = GetObjectiveTarget(locale);
            var value = GetObjectiveValue(locale);
            return (target, value, Completed);
        }

        private string GetObjectiveTarget(YisoLocale.Locale locale) => questName[locale];

        private string GetObjectiveValue(YisoLocale.Locale locale) {
            if (Completed) {
                return locale == YisoLocale.Locale.KR ? "완료" : "Complete";
            }

            return locale == YisoLocale.Locale.KR ? "미완료" : "Not Complete";
        }
    }
}