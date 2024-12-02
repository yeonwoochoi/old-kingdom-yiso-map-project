using System.Text;
using Core.Behaviour;
using Core.Domain.Locale;
using Core.Domain.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.HUD.Quest {
    public class YisoPlayerHUDQuestPanelItemUI : RunIBehaviour {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private GameObject arrowObject;
        [SerializeField] private Button button;

        public int Index { get; private set; } = -1;

        public Button ItemButton => button;

        private bool active;

        public YisoQuest Quest { get; private set; }

        public bool CompareId(int questId) => Quest != null && Quest.Id == questId;

        public bool Active {
            get => active;
            set {
                active = value;
                gameObject.SetActive(value);
                Clear();
            }
        }

        public void SetQuest(int index, YisoQuest quest, YisoLocale.Locale locale) {
            Index = index;
            Quest = quest;
        }

        public void SetQuestName(YisoQuest quest, YisoLocale.Locale locale) {
            var prepend = GetPrependType(locale);
            titleText.SetText($"{prepend} {quest.GetName(locale)}");
        }

        public void SetQuestNameWithProgress(YisoQuest quest, YisoLocale.Locale locale) {
            var prepend = GetPrependType(locale);
            var progress = quest.GetQuestProgress();
            var questName = quest.GetName(locale);
            titleText.SetText($"{prepend} {questName} ({progress.ToPercentage()})");
        }

        private string GetPrependType(YisoLocale.Locale locale) {
            var builder = new StringBuilder("[");
            if (locale == YisoLocale.Locale.KR) {
                builder.Append(Quest.Type == YisoQuest.Types.MAIN ? "메인" : "서브");
            } else {
                builder.Append(Quest.Type == YisoQuest.Types.MAIN ? "Main" : "Sub");
            }

            builder.Append("]");
            return builder.ToString();
        }

        private void Clear() {
            Index = -1;
            Quest = null;
            button.onClick.RemoveAllListeners();
        }
    }
}