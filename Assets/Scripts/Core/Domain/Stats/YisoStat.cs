using System;
using Core.Domain.Types;
using JetBrains.Annotations;
using UnityEngine.Events;
using Utils.Extensions;

namespace Core.Domain.Stats {
    public sealed class YisoStat {
        [CanBeNull] public event UnityAction<ChangedEventArgs> ChangedEvent;
        public event UnityAction<YisoEquipStat, int> OnValueChangedEvent; 
        private readonly bool isPercentage;
        private Expressions expression;
        private readonly YisoEquipStat stat;
        public int Value { get; private set; }
        
        public YisoStat(YisoEquipStat stat, int initialValue = 0, bool isPercentage = false, Expressions expression = Expressions.SUM) {
            this.stat = stat;
            Value = initialValue;
            this.isPercentage = isPercentage;
            this.expression = expression;
        }

        public void SetValue(int value, bool notify = true) {
            var beforeValue = Value;
            Value = value;
            OnValueChangedEvent?.Invoke(stat, value);
            if (!notify) return;
            RaiseEvent(new ChangedEventArgs(beforeValue, value));
        }

        public void ApplyValue(int value, bool notify = true) {
            var result = Value;
            switch (expression) {
                case Expressions.SUM:
                    result += value;
                    break;
                case Expressions.MULTIPLY:
                    if (isPercentage) result = Multiply(result, value);
                    else result *= value;
                    break;
            }
            
            SetValue(result, notify);
        }

        private int Multiply(int value, int applyValue) {
            var currentProb = value.ToProb();
            var applyProb = applyValue.ToProb();
            var appliedProb = 1 - ((1 - currentProb) * (1 - applyProb));
            return (int)(appliedProb * 100);
        }
        
        public void RaiseEvent(ChangedEventArgs args) {
            ChangedEvent?.Invoke(args);
        }

        public enum Expressions {
            SUM, MULTIPLY
        }
        
        public class ChangedEventArgs : EventArgs {
            public int BeforeValue { get; }
            public int AfterValue { get; }

            public ChangedEventArgs(int beforeValue, int afterValue) {
                BeforeValue = beforeValue;
                AfterValue = afterValue;
            }
        }
    }
}