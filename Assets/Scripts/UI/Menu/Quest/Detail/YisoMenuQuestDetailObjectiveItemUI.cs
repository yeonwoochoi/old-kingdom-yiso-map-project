using Core.Behaviour;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace UI.Menu.Quest.Detail {
    public class YisoMenuQuestDetailObjectiveItemUI : RunIBehaviour {
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField, Title("Color")] private Color completeColor;
        [SerializeField] private Color nonCompleteColor;
        
        public int Index { get; private set; }

        public void ShowObjective(int index, string target, string value, bool complete) {
            Index = index;
            target = complete ? $"<s>{target}</s>" : target;
            if (value != string.Empty) value = complete ? $"<s>{value}</s>" : value;

            var color = complete ? completeColor : nonCompleteColor;
            targetText.SetText(target);
            valueText.SetText(value);
            valueText.color = color;
        }
        
        public void UpdateObjective(string target, string updateValue, bool complete) {
            var value = string.Empty;
            if (updateValue != string.Empty) value = complete ? $"<s>{updateValue}</s>" : updateValue;
            var color = complete ? completeColor : nonCompleteColor;
            
            if (complete) targetText.SetText($"<s>{target}</s>");
            valueText.SetText(value);
            valueText.color = color;
        }
    }
}