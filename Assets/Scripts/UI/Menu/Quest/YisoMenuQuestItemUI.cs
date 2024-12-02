using System;
using Core.Behaviour;
using Core.Domain.Locale;
using Core.Domain.Quest;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Quest {
    public class YisoMenuQuestItemUI : YisoUIController, IInstantiatable {
        [SerializeField] private TextMeshProUGUI titleText;

        public bool Active { get; set; } = false;
        
        public Toggle QuestToggle { get; private set; }
        
        public YisoQuest Quest { get; private set; }

        public void Init() {
            QuestToggle = GetComponent<Toggle>();
            Clear();
        }
        
        public void SetQuest(YisoQuest quest) {
            Quest = quest;
            SetQuestTitle();
        }

        public void UpdateQuest(YisoQuest quest) {
            SetQuestTitle();
        }

        private void SetQuestTitle() {
            var progress = (int)Quest.GetQuestProgress();
            string typeString;
            if (CurrentLocale == YisoLocale.Locale.KR) {
                typeString = Quest.Type == YisoQuest.Types.MAIN ? "메인" : "서브";
            } else {
                typeString = Quest.Type == YisoQuest.Types.MAIN ? "Main" : "Sub";
            }

            typeString = $"[{typeString}]";
            titleText.SetText($"{typeString} {Quest.GetName(CurrentLocale)} ({progress.ToPercentage()})");
        }

        public void Clear() {
            Quest = null;
            Active = false;
        }
    }
}