using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Domain.Actor.Player.Modules.Base;
using Core.Domain.Locale;
using Core.Domain.Quest;
using Core.Domain.Quest.SO;
using Core.Service;
using Core.Service.Data;
using Core.Service.Stage;
using Core.Service.UI.Global;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace Core.Domain.Actor.Player.Modules.Quest {
    public class YisoPlayerQuestModule : YisoPlayerBaseModule {
        public event UnityAction<QuestEventArgs> OnQuestEvent;
        private QuestUpdateEventArgs cachedUpdateArgs = null;
        
        private readonly Dictionary<int, YisoQuest> quests = new();
        private readonly Dictionary<YisoQuestStatus, List<int>> progresses = new() {
            { YisoQuestStatus.IDLE, new List<int>() },
            { YisoQuestStatus.READY, new List<int>() },
            { YisoQuestStatus.PROGRESS, new List<int>() },
            { YisoQuestStatus.PRE_COMPLETE, new List<int>() },
            { YisoQuestStatus.COMPLETE, new List<int>() }
        };
        
        
        public YisoPlayerQuestModule(YisoPlayer player, IReadOnlyList<YisoQuest> quests) : base(player) {
            foreach (var quest in quests) {
                this.quests[quest.Id] = quest;
                progresses[YisoQuestStatus.IDLE].Add(quest.Id);
            }
        }

        public void ResetData(IReadOnlyList<YisoQuest> quests) {
            foreach (var progress in progresses.Values) progress.Clear();
            this.quests.Clear();
            foreach (var quest in quests) {
                this.quests[quest.Id] = quest;
                progresses[YisoQuestStatus.IDLE].Add(quest.Id);
            }
        }

        public void ReadyQuest(int questId) {
            var quest = quests[questId];
            if (!TryFindQuestIdIndex(YisoQuestStatus.IDLE, questId, out var questIndex))
                throw new ArgumentException($"QuestID: {questId} not in idle states");
            
            quest.ClearRequirements();
            progresses[YisoQuestStatus.IDLE].RemoveAt(questIndex);
            progresses[YisoQuestStatus.READY].Add(questId);
            foreach (var requirement in quest.StartRequirements) requirement.Setup(player);
            
            // Debug.Log($"[QUEST|IDLE => READY] {quest.GetName(YisoLocale.Locale.KR)}({questId})");
            RaiseEvent(new QuestStatusChangeEventArgs(quest, YisoQuestStatus.READY));
        }
        

        public void StartQuest(IReadOnlyList<int> stageIds, int questId) {
            var quest = quests[questId];
            var args = new QuestOrderedEventArgs();
            var startActions = quest.StartActions;
            
            // Setup requirement
            if (!TryFindQuestIdIndex(YisoQuestStatus.READY, questId, out var questIndex))
                throw new ArgumentException($"QuestID: {questId} not in ready states");
            progresses[YisoQuestStatus.READY].RemoveAt(questIndex);

            if (quest.CompleteRequirements.IsEmpty()) {
                var status = quest.AutoComplete ? YisoQuestStatus.COMPLETE : YisoQuestStatus.PRE_COMPLETE;
                progresses[status].Add(questId);
                args.Add(new QuestStatusChangeEventArgs(quest, status));
            } else {
                foreach (var requirement in quest.CompleteRequirements) requirement.Setup(player);
                progresses[YisoQuestStatus.PROGRESS].Add(questId);
                args.Add(new QuestStatusChangeEventArgs(quest, YisoQuestStatus.PROGRESS));
            }
            
            // Give action when start quest
            foreach (var action in startActions) action.TryGive(player, out _);


            foreach (var action in startActions.Where(a => a is YisoQuestItemAddAction).Cast<YisoQuestItemAddAction>()) {
                UpdateRequirement(YisoQuestRequirement.Types.ITEM, action.ItemId);
            }
            
            args.ReOrder();
            RaiseEvent(args);
            
            // Debug.Log($"[QUEST|READY => START] {quest.GetName(YisoLocale.Locale.KR)}({questId})");
            return;

            void UpdateRequirement(YisoQuestRequirement.Types type, object value) {
                if (!quest.TryCheckCompleteRequirement(type, value, YisoLocale.Locale.KR, out var arg)) return;
                args.Add(arg);
                FloatingProgressText(arg);
                CheckQuestIsComplete(quest, ref args);
            }
        }
        

        public void UpdateQuestRequirement(int stageId, YisoQuestRequirement.Types type, object value) {
            var questsInStage = YisoServiceProvider.Instance.Get<IYisoStageService>().GetQuestsInStage(stageId);
            var questIds = questsInStage.Select(q => q.Id).ToList();

            var orderedArgs = new QuestOrderedEventArgs();
            
            foreach (var questId in questIds) {
                orderedArgs.Clear();
                
                var quest = quests[questId];
                QuestUpdateEventArgs args = null;
                if (progresses[YisoQuestStatus.READY].Contains(questId)) {
                    if (quest.TryCheckStartRequirement(type, value, YisoLocale.Locale.KR, out args)) {
                        if (args != null) orderedArgs.Add(args);
                        if (!type.CanMultipleCheck()) {
                            RaiseOrderedArgs();
                            break;
                        }
                    }
                } else if (progresses[YisoQuestStatus.PROGRESS].Contains(questId)) {
                    if (quest.TryCheckCompleteRequirement(type, value, YisoLocale.Locale.KR, out args)) {
                        CheckQuestIsComplete(quest, ref orderedArgs);
                        if (args != null) {
                            orderedArgs.Add(args);
                            FloatingProgressText(args);
                        }
                        if (!type.CanMultipleCheck()) {
                            RaiseOrderedArgs();
                            break;
                        }
                    }
                }
                
                RaiseOrderedArgs();
            }

            return;

            void RaiseOrderedArgs() {
                orderedArgs.ReOrder();
                RaiseEvent(orderedArgs);
            }
        }

        private void FloatingProgressText(QuestUpdateEventArgs args) {
            if (cachedUpdateArgs == null) {
                cachedUpdateArgs = args;
            } else {
                if (args.IsSameArgs(cachedUpdateArgs)) return;
                cachedUpdateArgs = args;
            }
            
            var builder = new StringBuilder();
            var value = string.Empty;
            if (args.UpdateValue != string.Empty)
                value = args.IsComplete ? $"<s>{args.UpdateValue}</s>" : args.UpdateValue;
            if (args.IsComplete) {
                builder.Append($"<s>{args.OriginalTarget}</s> ");
            } else {
                builder.Append(args.OriginalTarget).Append(" ");
            }

            builder.Append(value);
            YisoServiceProvider.Instance.Get<IYisoGlobalUIService>()
                .FloatingText(builder.ToString(), Color.white);
        }

        private void CheckQuestIsComplete(YisoQuest quest, ref QuestOrderedEventArgs args) {
            if (!quest.IsComplete) return;
            var status = quest.AutoComplete ? YisoQuestStatus.COMPLETE : YisoQuestStatus.PRE_COMPLETE;
            var index = progresses[YisoQuestStatus.PROGRESS].IndexOf(quest.Id);
            progresses[YisoQuestStatus.PROGRESS].RemoveAt(index);
            progresses[status].Add(quest.Id);
            args.Add(new QuestStatusChangeEventArgs(quest, status));
        }

        public void DrawStage(int stageId) {
            var checkStatuses = new[] {
                YisoQuestStatus.READY,
                YisoQuestStatus.PROGRESS,
                YisoQuestStatus.PRE_COMPLETE,
                YisoQuestStatus.COMPLETE
            };

            var questsInStage = YisoServiceProvider.Instance.Get<IYisoStageService>().GetQuestsInStage(stageId);

            foreach (var quest in questsInStage) {
                var id = quest.Id;
                var status = GetStatusByQuestId(id);
                if (!checkStatuses.Contains(status)) continue;
                if (!TryFindQuestIdIndex(status, id, out var index))
                    throw new Exception($"Quest(id={id}|status={status}) not exists!");
                progresses[YisoQuestStatus.IDLE].Add(id);
                progresses[status].RemoveAt(index);
                RaiseEvent(new QuestStatusChangeEventArgs(quest, status, YisoQuestStatus.IDLE, true));
            }
        }
        
        public void DrawQuests(int stageId) {
            var checkStatuses = new[] {
                YisoQuestStatus.READY,
                YisoQuestStatus.PROGRESS,
                YisoQuestStatus.PRE_COMPLETE
            };

            var questsInStage = YisoServiceProvider.Instance.Get<IYisoStageService>().GetQuestsInStage(stageId);

            foreach (var quest in questsInStage) {
                var id = quest.Id;
                var status = GetStatusByQuestId(id);
                if (!checkStatuses.Contains(status)) continue;
                if (!TryFindQuestIdIndex(status, id, out var index))
                    throw new Exception($"Quest(id={id}|status={status}) not exists!");
                progresses[YisoQuestStatus.IDLE].Add(id);
                progresses[status].RemoveAt(index);
                RaiseEvent(new QuestStatusChangeEventArgs(quest, YisoQuestStatus.IDLE));
            }
        }

        public void ChangeQuestStatus(int questId, YisoQuestStatus to = YisoQuestStatus.IDLE) {
            var status = GetStatusByQuestId(questId);
            if (!TryFindQuestIdIndex(status, questId, out var index))
                throw new Exception($"Quest(id={questId}) not in {status}");
            var quest = quests[questId];
            progresses[to].Add(questId);
            progresses[status].RemoveAt(index);
            RaiseEvent(new QuestStatusChangeEventArgs(quest, to));
        }

        public void CompleteQuest(int questId) {
            if (!TryFindQuestIdIndex(YisoQuestStatus.PRE_COMPLETE, questId, out var questIndex)) 
                throw new ArgumentException($"QuestID: {questId} not in pre complete states");
            
            progresses[YisoQuestStatus.PRE_COMPLETE].RemoveAt(questIndex);
            var quest = quests[questId];
            progresses[YisoQuestStatus.COMPLETE].Add(questId);
            RaiseEvent(new QuestStatusChangeEventArgs(quest, YisoQuestStatus.COMPLETE));
            quest.CompleteQuest(player);
        }

        public bool IsQuestComplete(int questId) {
            return progresses[YisoQuestStatus.COMPLETE].Any(id => id == questId);
        }

        public bool IsQuestPreComplete(int questId) {
            return progresses[YisoQuestStatus.PRE_COMPLETE].Any(id => id == questId);
        }

        public bool IsStartRequirementAllDone(int questId) {
            if (!quests.TryGetValue(questId, out var quest))
                throw new ArgumentException($"Quest({questId}) Not Found!");
            return quest.IsStartRequirementAllDone;
        }
        
        public bool IsCompleteRequirementAllDone(int questId) {
            if (!quests.TryGetValue(questId, out var quest))
                throw new ArgumentException($"Quest({questId}) Not Found!");
            return quest.IsCompleteRequirementAllDone;
        }

        public List<YisoQuest> GetQuestsByStatuses(params YisoQuestStatus[] statuses) {
            var result = new List<YisoQuest>();
            
            foreach (var status in statuses) {
                result.AddRange(progresses[status].Select(id => quests[id]));
            }

            return result;
        }

        public int GetQuestCountByStatus(YisoQuestStatus status) => progresses[status].Count;

        public YisoQuestStatus GetStatusByQuestId(int questId) {
            foreach (var (status, ids) in progresses) {
                foreach (var id in ids) {
                    if (id != questId) continue;
                    return status;
                }
            }

            return YisoQuestStatus.IDLE;
        }
        
        private bool TryFindQuestIdIndex(YisoQuestStatus status, int questId, out int index) {
            index = -1;

            for (var i = 0; i < progresses[status].Count; i++) {
                if (progresses[status][i] != questId) continue;
                index = i;
                break;
            }

            return index != -1;
        }

        private void RaiseEvent(QuestEventArgs args) {
            OnQuestEvent?.Invoke(args);
        }

        private void RaiseEvent(QuestOrderedEventArgs args) {
            foreach (var arg in args) {
                OnQuestEvent?.Invoke(arg);
            }
        }
    }
}