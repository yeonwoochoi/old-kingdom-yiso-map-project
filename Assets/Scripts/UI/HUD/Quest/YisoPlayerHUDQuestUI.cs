using System.Text;
using Core.Behaviour;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Locale;
using Core.Domain.Quest;
using Core.Logger;
using Core.Service;
using Core.Service.UI.Global;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.HUD.Quest {
    public class YisoPlayerHUDQuestUI : YisoPlayerUIController {
        [SerializeField] private YisoPlayerHUDQuestListPanelUI listPanelUI;
        [SerializeField] private YisoPlayerHUDQuestDetailPanelUI detailPanelUI;
        [SerializeField] private Button hudQuestButton;
        [SerializeField] private CanvasGroup hudQuestButtonAlertCanvas;
        [SerializeField] private Button closeButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI headerTitleText;
        [Title("Floating Colors")]
        [SerializeField] private Color completeTextColor;
        
        private bool hidePanel = true;

        private CanvasGroup hudQuestButtonCanvas;
        
        protected override void Start() {
            base.Start();
            hudQuestButtonCanvas = hudQuestButton.GetComponent<CanvasGroup>();
            
            hudQuestButton.onClick.AddListener(OnClickHUDButton);
            closeButton.onClick.AddListener(OnClickCloseButton);
        }

        protected override void OnEnable() {
            base.OnEnable();
            listPanelUI.OnQuestEvent += OnQuestEvent;
            detailPanelUI.OnBackEvent += OnBackDetail;
            player.QuestModule.OnQuestEvent += OnPlayerQuestEvent;
        }

        protected override void OnDisable() {
            base.OnDisable();
            listPanelUI.OnQuestEvent -= OnQuestEvent;
            detailPanelUI.OnBackEvent -= OnBackDetail;
            player.QuestModule.OnQuestEvent -= OnPlayerQuestEvent;
        }

        private void OnQuestEvent(YisoQuest quest) {
            listPanelUI.Visible(false);
            detailPanelUI.SetQuest(quest);
            headerTitleText.SetText(quest.GetName(CurrentLocale));
            detailPanelUI.Visible(true);
        }

        private void OnPlayerQuestEvent(QuestEventArgs args) {
            switch (args) {
                case QuestStatusChangeEventArgs changeStatusArgs:
                    // YisoLogger.Log($"[QUEST_HUD|CHANGE_STATUS] {args.QuestId}({args.Quest.GetName(YisoLocale.Locale.KR)}): {changeStatusArgs.To}");
                    listPanelUI.ChangeStatus(changeStatusArgs);
                    detailPanelUI.UpdateQuest(changeStatusArgs);
                    
                    if (hidePanel) hudQuestButtonAlertCanvas.Visible(true);
                    
                    if (changeStatusArgs.To is YisoQuestStatus.COMPLETE && args.QuestId == detailPanelUI.QuestId) 
                        OnBackDetail();
                    
                    break;
                case QuestUpdateEventArgs updateArgs:
                    // YisoLogger.Log($"[QUEST_HUD|UPDATE] {args.QuestId}({args.Quest.GetName(YisoLocale.Locale.KR)}): {updateArgs.OriginalTarget} => {updateArgs.UpdateValue}");
                    listPanelUI.UpdateQuest(updateArgs);
                    detailPanelUI.UpdateQuest(updateArgs);
                    break;
            }
        }
        
        private void OnBackDetail() {
            detailPanelUI.Visible(false);
            detailPanelUI.Clear();
            headerTitleText.SetText(GetListTitle());
            listPanelUI.Visible(true);
        }

        private void OnClickHUDButton() {
            hidePanel = false;
            if (detailPanelUI.ExistDetail)
                detailPanelUI.Visible(true);
            else 
                listPanelUI.Visible(true);
            
            canvasGroup.Visible(true);
            hudQuestButtonCanvas.Visible(false);
            
            if (hudQuestButtonAlertCanvas.IsVisible()) {
                hudQuestButtonAlertCanvas.Visible(false);
            }
        }

        private void OnClickCloseButton() {
            hidePanel = true;
            if (detailPanelUI.ExistDetail) {
                detailPanelUI.Visible(false);
            } else
                listPanelUI.Visible(false);
            
            canvasGroup.Visible(false);
            hudQuestButtonCanvas.Visible(true);
        }

        private string GetListTitle() => CurrentLocale == YisoLocale.Locale.KR ? "퀘스트 목록" : "Quest List";
    }
}