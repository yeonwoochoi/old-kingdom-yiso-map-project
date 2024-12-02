using System;
using Character.Weapon;
using Core.Domain.Actor.Player.Modules.Inventory;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Character;
using Core.Service.Domain;
using Core.Service.Scene;
using Core.Service.Stage;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Common.Inventory;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.HUD.StageInfo {
    public class YisoPlayerHUDStageInfoUI : YisoPlayerUIController {
        [SerializeField, Title("Info")] private TextMeshProUGUI flowText;
        [SerializeField] private TextMeshProUGUI storyCommentText;
        [SerializeField] private TextMeshProUGUI requiredCRRangeText;
        [SerializeField] private TextMeshProUGUI playerCRText;
        [SerializeField, Title("Scroll")] private ScrollRect scrollRect;
        [SerializeField, Title("Button")] private YisoButtonWithCanvas quickButton;
        [SerializeField, Title("Color")] private Color notFitColor;
        [SerializeField] private Color chillColor;
        [SerializeField] private Color enoughColor;
        
        private CanvasGroup canvasGroup;
        private bool hidePanel = true;

        private IYisoStageService stageService;
        private IYisoSceneService sceneService;
        private IYisoDomainService domainService;

        protected override void Awake() {
            base.Awake();
            stageService = YisoServiceProvider.Instance.Get<IYisoStageService>();
            sceneService = YisoServiceProvider.Instance.Get<IYisoSceneService>();
            domainService = YisoServiceProvider.Instance.Get<IYisoDomainService>();
        }

        protected override void OnEnable() {
            base.OnEnable();
            sceneService.RegisterOnSceneChanged(OnSceneChanged);
            player.StatModule.OnWeaponCombatRatingChangedEvent += OnWeaponCombatRatingChanged;
        }

        protected override void OnDisable() {
            base.OnDisable();
            sceneService.UnregisterOnSceneChanged(OnSceneChanged);
            player.StatModule.OnWeaponCombatRatingChangedEvent -= OnWeaponCombatRatingChanged;
        }

        protected override void Start() {
            canvasGroup = GetComponent<CanvasGroup>();
            quickButton.onClick.AddListener(OnClickHUDButton);
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private void SetInfo() {
            var currentStageId = stageService.GetCurrentStageId();

            var flowComment = domainService.GetStageFlowComment(currentStageId, CurrentLocale);
            var storyComment = domainService.GetStoryLoadingComment(currentStageId, CurrentLocale);
            
            flowText.SetText(flowComment);
            storyCommentText.SetText(storyComment);
            
            var currentWeapon = player.InventoryModule.GetCurrentEquippedWeaponType();
            var playerCR = Mathf.CeilToInt((float)player.StatModule.WeaponCombatRatings[currentWeapon]);

            SetCombatRating(playerCR);
        }

        private void SetCombatRating(double playerCR) {
            var currentStageCR = stageService.GetCurrentStageCR();
            var minCR = Mathf.CeilToInt((float)currentStageCR);
            var maxCR = Mathf.CeilToInt((float)currentStageCR * 2);
            
            requiredCRRangeText.SetText($"{minCR.ToCommaString()}~{maxCR.ToCommaString()}");
            playerCRText.SetText(playerCR.ToCommaString());

            Color color;
            if (playerCR < minCR) color = notFitColor;
            else if (playerCR > minCR && playerCR < maxCR) color = chillColor;
            else color = enoughColor;
            
            playerCRText.color = color;
        }

        private void OnSceneChanged(YisoSceneTypes beforeSceneType, YisoSceneTypes afterSceneType) {
            quickButton.Visible = afterSceneType == YisoSceneTypes.STORY;
            if (afterSceneType == YisoSceneTypes.STORY) {
                SetInfo();
                OnClickHUDButton();
                return;
            }
            Clear();
        }

        private void OnWeaponCombatRatingChanged(YisoWeapon.AttackType type, double combatRating) {
            if (hidePanel) return;
            var currentWeapon = player.InventoryModule.GetCurrentEquippedWeaponType();
            if (type != currentWeapon) return;
            SetCombatRating(combatRating);
        }

        private void Clear() {
            OnClickCloseButton();
            requiredCRRangeText.SetText("");
            playerCRText.SetText("");
            playerCRText.color = Color.white;
            storyCommentText.SetText("");
            flowText.SetText("");
        }

        private void OnClickHUDButton() {
            hidePanel = false;
            canvasGroup.Visible(true);
            quickButton.Visible = false;
        }

        public void OnClickCloseButton() {
            hidePanel = true;
            canvasGroup.Visible(false);
            quickButton.Visible = true;
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}