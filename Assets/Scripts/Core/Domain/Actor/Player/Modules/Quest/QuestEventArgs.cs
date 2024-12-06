using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Quest;
using UniRx;
using Unity.VisualScripting;

namespace Core.Domain.Actor.Player.Modules.Quest {
    public abstract class QuestEventArgs {
        public YisoQuest Quest { get; }
        
        public int QuestId => Quest.Id;

        protected QuestEventArgs(YisoQuest quest) {
            Quest = quest;
        }
    }

    public class QuestStatusChangeEventArgs : QuestEventArgs {
        public YisoQuestStatus To { get; }
        public YisoQuestStatus Before { get; }
        
        public bool IsDraw { get; }

        public bool IsMainQuestsInStageAllComplete { get; internal set; } = false;

        public bool IsProgress => To == YisoQuestStatus.PROGRESS;

        public bool StatusIs(YisoQuestStatus status) => status == To;

        public QuestStatusChangeEventArgs(YisoQuest quest, YisoQuestStatus to, bool isDraw = false) : base(quest) {
            To = to;
            IsDraw = isDraw;
        }

        public QuestStatusChangeEventArgs(YisoQuest quest, YisoQuestStatus before, YisoQuestStatus to,
            bool isDraw = false) : this(quest, to, isDraw) {
            Before = before;
        }
    }

    public class QuestDrawEventArgs : QuestEventArgs {
        public QuestDrawEventArgs(YisoQuest quest) : base(quest) { }
    }
    

    public class QuestOrderedEventArgs : IEnumerable<QuestEventArgs> {
        private readonly List<QuestEventArgs> argsList = new();
        private readonly List<QuestStatusChangeEventArgs> changeArgsList = new();
        private readonly List<QuestUpdateEventArgs> updateArgsList = new();
        
        public QuestOrderedEventArgs() { }

        public void Add(QuestEventArgs args) {
            if (args is QuestStatusChangeEventArgs changeArgs)
                changeArgsList.Add(changeArgs);
            else
                updateArgsList.Add(args as QuestUpdateEventArgs);
        }

        public QuestEventArgs this[int index] {
            get => argsList[index];
            set => argsList[index] = value;
        }

        public void ReOrder() {
            var existCompleteIndex =
                changeArgsList.FindIndex(args => args.To is YisoQuestStatus.PRE_COMPLETE or YisoQuestStatus.COMPLETE);

            if (existCompleteIndex != -1) {
                argsList.Add(changeArgsList[existCompleteIndex]);
                return;
            }

            argsList.AddRange(changeArgsList.DistinctBy(args => args.To));
            argsList.AddRange(updateArgsList.DistinctBy(args => args.Index));
        }

        public void Clear() {
            changeArgsList.Clear();
            updateArgsList.Clear();
            argsList.Clear();
        }

        public IEnumerator<QuestEventArgs> GetEnumerator() => argsList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => argsList.GetEnumerator();
    }

    public class QuestUpdateEventArgs : QuestEventArgs {
        public int Index { get; }
        public string UpdateValue => info.UpdateValue;
        public string OriginalTarget => info.OriginalTarget;
        public bool IsComplete => info.State == YisoQuestRequirement.UpdateStates.COMPLETED;

        private readonly UpdateInfo info;
        
        public QuestUpdateEventArgs(YisoQuest quest, int index, UpdateInfo info) : base(quest) {
            Index = index;
            this.info = info;
        }

        public bool IsSameArgs(QuestUpdateEventArgs other) => ToString() == other.ToString();
        
        public override string ToString() => $"Target: {OriginalTarget}, Update: {UpdateValue}, Complete: {IsComplete}";

        public class UpdateInfo {
            public YisoQuestRequirement.UpdateStates State { get; } 
            
            public string UpdateValue { get; }
            public string OriginalTarget { get; }
            

            private UpdateInfo(YisoQuestRequirement.UpdateStates state) {
                State = state;
            }

            public UpdateInfo(YisoQuestRequirement.UpdateStates state, string originalTarget, string updateValue) :
                this(state) {
                OriginalTarget = originalTarget;
                UpdateValue = updateValue;
            }
        }
    }
}