using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Actor.Player;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Data;
using Core.Domain.Locale;
using Core.Domain.Quest.SO;
using Sirenix.Utilities;

namespace Core.Domain.Quest {
    public sealed class YisoQuest {
        public int Id { get; }
        public Types Type { get; }
        
        public bool AutoComplete { get; }

        private readonly YisoLocale name;

        public List<YisoQuestRequirement> StartRequirements { get; }
        public List<YisoQuestRequirement> CompleteRequirements { get; }
        public List<YisoQuestAction> StartActions { get; }
        public List<YisoQuestAction> CompleteActions { get; }

        private readonly Dictionary<YisoQuestRequirement.Types, List<int>> completeRequirementIndexDict = new();
        private readonly Dictionary<YisoQuestRequirement.Types, List<int>> startRequirementIndexDict = new();
        
        public bool IsComplete => CompleteRequirements.All(r => r.Check());

        public bool IsStartRequirementAllDone => StartRequirements.All(r => r.Check());

        public bool IsCompleteRequirementAllDone => CompleteRequirements.All(r => r.Check());

        public bool CanStart => StartRequirements.Count == 1 || StartRequirements.All(r => r.Check());

        public float GetQuestProgress(bool normalize = false) {
            var totalRequirement = CompleteRequirements.Count;
            if (totalRequirement == 0) return normalize ? 1 : 100;
            var completeRequirement = CompleteRequirements.Count(req => req.Check());
            var progress = completeRequirement / (float) totalRequirement;
            if (normalize) return progress;
            return progress * 100f;
        }

        public List<YisoQuestAction> GetRewardActions(bool complete = true) {
            var actions = complete ? CompleteActions : StartActions;
            return actions.Where(action =>
                    action.Type != YisoQuestAction.Types.ITEM_ADD && action.Type != YisoQuestAction.Types.ITEM_REMOVE)
                .ToList();
        }
        
        public YisoQuest(YisoQuestSO so) {
            Id = so.id;
            Type = so.type;
            name = so.name;
            AutoComplete = so.autoComplete;
            
            StartRequirements = so.startRequirements.Select((r, i) => r.CreateRequirement(i)).ToList();
            CompleteRequirements = so.completeRequirements.Select((r, i) => r.CreateRequirement(i)).ToList();
            StartActions = so.startActions.Select(s => s.CreateAction()).ToList();
            CompleteActions = so.completeActions.Select(s => s.CreateAction()).ToList();

            StartRequirements.Select(r => (r.Type, r.Index))
                .ForEach(item => {
                    var (type, index) = item;
                    if (!startRequirementIndexDict.ContainsKey(type))
                        startRequirementIndexDict[type] = new List<int>();
                    startRequirementIndexDict[type].Add(index);
                });
            
            CompleteRequirements.Select(r => (r.Type, r.Index))
                .ForEach(item => {
                    var (type, index) = item;
                    if (!completeRequirementIndexDict.ContainsKey(type))
                        completeRequirementIndexDict[type] = new List<int>();
                    completeRequirementIndexDict[type].Add(index);
                });
        }

        public string GetName(YisoLocale.Locale locale) => name[locale];

        public void CompleteQuest(YisoPlayer player) {
            foreach (var action in CompleteActions) {
                // TODO(TRY GIVE)
                action.TryGive(player, out var reason);
            }
        }

        public bool CanGetReward(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            foreach (var action in CompleteActions) {
                if (!action.CanGive(player, out reason)) return false;
            }

            return true;
        }
        
        public List<(T, int)> FindCompleteRequirements<T>() where T : YisoQuestRequirement {
            var type = YisoQuestRequirement.ToType<T>();
            if (!completeRequirementIndexDict.TryGetValue(type, out var indexes)) return null;
            return indexes.Select(index => (CompleteRequirements[index] as T, index)).ToList();
        }

        private List<(T, int)> FindStartRequirements<T>() where T : YisoQuestRequirement {
            var type = YisoQuestRequirement.ToType<T>();
            if (!startRequirementIndexDict.TryGetValue(type, out var indexes)) return null;
            return indexes.Select(index => (StartRequirements[index] as T, index)).ToList();
        }

        public (T, int) FindCompleteRequirement<T>() where T : YisoQuestRequirement {
            var type = YisoQuestRequirement.ToType<T>();
            if (!completeRequirementIndexDict.ContainsKey(type)) return (null, -1);
            var index = completeRequirementIndexDict[type].First();
            return ((T)CompleteRequirements[index], index);
        }

        public bool TryCheckCompleteRequirement(YisoQuestRequirement.Types type, object value, YisoLocale.Locale locale,
            out QuestUpdateEventArgs args) => type switch {
            YisoQuestRequirement.Types.ENEMY => TryCheckCompleteRequirements<YisoQuestEnemyRequirement>(value, locale,
                out args),
            YisoQuestRequirement.Types.ITEM => TryCheckCompleteRequirements<YisoQuestItemRequirement>(value, locale, out args),
            YisoQuestRequirement.Types.COMPLETE_QUEST => TryCheckCompleteRequirements<YisoQuestCompleteQuestRequirement>(value, locale, out args),
            YisoQuestRequirement.Types.PRE_COMPLETE_QUEST => TryCheckCompleteRequirements<YisoQuestPreCompleteQuestRequirement>(value, locale, out args),
            YisoQuestRequirement.Types.FIELD_ENTER => TryCheckCompleteRequirements<YisoQuestFieldEnterRequirement>(value, locale, out args),
            YisoQuestRequirement.Types.NPC => TryCheckCompleteRequirements<YisoQuestNpcRequirement>(value, locale, out args),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        
        public bool TryCheckStartRequirement(YisoQuestRequirement.Types type, object value, YisoLocale.Locale locale,
            out QuestUpdateEventArgs args) => type switch {
            YisoQuestRequirement.Types.ENEMY => TryCheckStartRequirements<YisoQuestEnemyRequirement>(value, locale,
                out args),
            YisoQuestRequirement.Types.ITEM => TryCheckStartRequirements<YisoQuestItemRequirement>(value, locale, out args),
            YisoQuestRequirement.Types.COMPLETE_QUEST => TryCheckStartRequirements<YisoQuestCompleteQuestRequirement>(value, locale, out args),
            YisoQuestRequirement.Types.PRE_COMPLETE_QUEST => TryCheckStartRequirements<YisoQuestPreCompleteQuestRequirement>(value, locale, out args),
            YisoQuestRequirement.Types.FIELD_ENTER => TryCheckStartRequirements<YisoQuestFieldEnterRequirement>(value, locale, out args),
            YisoQuestRequirement.Types.NPC => TryCheckStartRequirements<YisoQuestNpcRequirement>(value, locale, out args),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public bool IsQuestEnemy(int enemyId) {
            var reqs = GetCompleteRequirements<YisoQuestEnemyRequirement>();
            return reqs.Any(req => req.EnemyId == enemyId);
        }

        private List<T> GetCompleteRequirements<T>() where T : YisoQuestRequirement {
            var type = YisoQuestRequirement.ToType<T>();
            return CompleteRequirements.Where(req => req.Type == type).Cast<T>().ToList();
        }

        private bool TryCheckStartRequirements<T>(object value, YisoLocale.Locale locale,
            out QuestUpdateEventArgs args) where T : YisoQuestRequirement{
            args = null;

            var pairs = FindStartRequirements<T>();
            if (pairs == null) return false;

            foreach (var pair in pairs) {
                var (req, index) = pair;
                if (req.Check()) continue;
                if (!req.TryCheckAndUpdate(value, locale, out var info)) continue;
                args = new QuestUpdateEventArgs(this, index, info);
            }

            return args != null;
        }

        private bool TryCheckCompleteRequirements<T>(object value, YisoLocale.Locale locale, out QuestUpdateEventArgs args)
            where T : YisoQuestRequirement {
            args = null;

            var pairs = FindCompleteRequirements<T>();
            if (pairs == null) return false;

            foreach (var pair in pairs) {
                var (req, index) = pair;
                if (req.Check()) continue;
                if (!req.TryCheckAndUpdate(value, locale, out var info)) continue;
                args = new QuestUpdateEventArgs(this, index, info);
            }

            return args != null;
        }

        public void Clear() {
            CompleteRequirements.ForEach(req => req.Clear());
            StartRequirements.ForEach(req => req.Clear());
            
        }

        public void ClearRequirements() {
            Action<List<YisoQuestRequirement>> resetReqAction = requirements => {
                foreach (var req in requirements) req.Clear();
            };

            resetReqAction(StartRequirements);
            resetReqAction(CompleteRequirements);
        }
        
        public enum Types {
            MAIN, SUB
        }
    }
    
    public enum YisoQuestStatus {
        IDLE,
        READY,
        PROGRESS,
        PRE_COMPLETE,
        COMPLETE
    }

    public enum YisoQuestGiveReasons {
        NONE,
        INVENTORY_FULL,
        NO_ITEM_IN_INVENTORY
    }
}