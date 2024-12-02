using System;
using Core.Behaviour;
using Core.Domain.Locale;
using Core.Domain.Quest;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Quest {
    public class YisoMenuQuestDrawPopupUI : YisoUIController {
        [SerializeField, Title("Content")] private TextMeshProUGUI contentText;
        [SerializeField, Title("Buttons")] private Button drawButton;
        [SerializeField] private Button cancelButton;

        private CanvasGroup canvasGroup;
        private YisoQuest quest;
        private Action onClickCancel = null;
        private Action<YisoQuest> onClickDraw = null;

        private void Start() {
            canvasGroup = GetOrAddComponent<CanvasGroup>();
            
            drawButton.onClick.AddListener(OnClickDraw);
            cancelButton.onClick.AddListener(OnClickCancel);
        }

        public void Active(bool flag, YisoQuest quest = null, Action<YisoQuest> onClickDraw = null, Action onClickCancel = null) {
            this.onClickCancel = onClickCancel;
            this.onClickDraw = onClickDraw;
            this.quest = quest;

            if (flag) {
                var questName = quest.GetName(CurrentLocale);
                var content = CurrentLocale == YisoLocale.Locale.KR
                    ? $"정말 '{questName}'을 포기하겠습니까?"
                    : $"Are you sure to draw '{questName}'?";
                contentText.SetText(content);
            }
            
            canvasGroup.Visible(flag);
        }

        private void OnClickCancel() {
            onClickCancel?.Invoke();
        }

        private void OnClickDraw() {
            onClickDraw?.Invoke(quest);
        }
    }
}