using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Domain.Locale;
using Core.Domain.Skill;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Stage;
using Core.Service.UI.Menu;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UI.Common.Inventory;
using UI.Menu.Skill.Description.Preview;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Skill.Description {
    public class YisoMenuSkillDescriptionUI : YisoPlayerUIController {
        [SerializeField, Title("Basic")] private YisoMenuSkillBasicDescriptionUI basicDescriptionUI;
        [SerializeField, Title("Diff")] private YisoMenuSkillDiffDescriptionUI diffDescriptionUI;
        [SerializeField, Title("Quick Slot")] private YisoMenuSkillQuickSlotsUI quickSlotsUI;
        [SerializeField, Title("Preview")] private YisoMenuSkillPreviewUI previewUI; 
        [SerializeField, Title("Buttons")] private YisoButtonWithCanvas quickButton;
        [SerializeField] private YisoButtonWithCanvas levelUpButton;
        
        private YisoSkill skill = null;
        private CanvasGroup canvasGroup;

        private IYisoMenuUIService menuUIService;

        public bool Visible {
            get => canvasGroup.IsVisible();
            set => canvasGroup.Visible(value);
        }

        public void RegisterEvents() {
            quickSlotsUI.OnClickSlotEvent += OnClickSlot;
            quickSlotsUI.RegisterEvents();
        }

        public void UnregisterEvents() {
            quickSlotsUI.OnClickSlotEvent -= OnClickSlot;
            quickSlotsUI.UnregisterEvents();
        }

        protected override void Start() {
            base.Start();
            canvasGroup = GetComponent<CanvasGroup>();
            
            quickButton.onClick.AddListener(OnClickQuickSlot);
            levelUpButton.onClick.AddListener(OnClickLevelUp);

            menuUIService = YisoServiceProvider.Instance.Get<IYisoMenuUIService>();
        }

        public void ChangeWeapon(YisoWeapon.AttackType weapon) {
            quickSlotsUI.ChangeWeapon(weapon);
        }

        public void SetSkill(YisoSkill skill, int stageId) {
            this.skill = skill;
            basicDescriptionUI.SetSkill(skill, stageId);
            diffDescriptionUI.SetSkill(skill, stageId);

            SetButtonVisible(stageId);
            
            quickSlotsUI.Visible = skill.Type == YisoSkill.Types.ACTIVE;
            previewUI.Visible = this.skill.Type == YisoSkill.Types.ACTIVE;
            
            if (previewUI.Visible) {
                previewUI.Visible = true;
                previewUI.CastSkill(skill.Id);
            }
        }

        public void UpdateSkill(YisoSkill updateSkill) {
            if (skill.Id != updateSkill.Id) return;
            skill = updateSkill;
            var stageId = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();
            basicDescriptionUI.UpdateSkill(updateSkill, stageId);
            diffDescriptionUI.SetSkill(updateSkill, stageId);
            SetButtonVisible(stageId);
        }

        private void SetButtonVisible(int stageId) {
            quickButton.Visible = skill.Type == YisoSkill.Types.ACTIVE && skill.IsLearned;
            // quickButton.Visible = skill.Type == YisoSkill.Types.ACTIVE;
            levelUpButton.Visible = !skill.IsLocked(stageId) && !skill.IsMasterLevel && (player.SkillModule.SkillPoint > 0);
        }

        public void Clear() {
            skill = null;
            basicDescriptionUI.Clear();
            diffDescriptionUI.Clear();
            
            if (previewUI.Visible) {
                previewUI.StopSkill();
                previewUI.Visible = false;
            }

            if (quickSlotsUI.Visible) {
                quickSlotsUI.Visible = false;
            }

            levelUpButton.Visible = false;
            quickButton.Visible = false;
        }
        
        private void OnClickQuickSlot() {
            previewUI.StopSkill();
            quickSlotsUI.ActiveSelectionMode(true);
            menuUIService.RaiseOnVisibleOverlayUI(true, ClearOverlay);
        }

        private void OnClickLevelUp() {
            player.SkillModule.UpgradeSkill(skill.Id);
        }

        private void OnClickSlot(int index) {
            player.UIModule.SetSkill(player.InventoryModule.GetCurrentEquippedWeaponType(), index, skill);
            ClearOverlay();
        }

        private void ClearOverlay() {
            menuUIService.RaiseOnVisibleOverlayUI(false);
            quickSlotsUI.ActiveSelectionMode(false);
            previewUI.CastSkill(skill.Id);
        }
    }
}