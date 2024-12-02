using Character.Weapon;
using Core.Domain.Actor.Player.Modules.Skill;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Stage;
using Sirenix.OdinInspector;
using TMPro;
using UI.Menu.Base;
using UI.Menu.Skill.Description;
using UI.Menu.Skill.Holder;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Skill {
    public class YisoMenuSkillContentUI : YisoMenuBasePanelUI {
        [SerializeField, Title("Tabs")] private Toggle[] tabs;
        [SerializeField] private YisoMenuSkillPanelItemsUI itemsUI;
        [SerializeField] private YisoMenuSkillDescriptionUI descriptionUI;
        [SerializeField] private TextMeshProUGUI skillPointText;

        private YisoWeapon.AttackType currentWeapon = YisoWeapon.AttackType.None;
        
        protected override void RegisterEvents() {
            base.RegisterEvents();
            itemsUI.OnSkillUIEvent += OnSkillUIEvent;
            player.SkillModule.OnSkillEvent += OnSkillEvent;
            descriptionUI.RegisterEvents();
            itemsUI.RegisterEvents();
        }

        protected override void UnregisterEvents() {
            base.UnregisterEvents();
            itemsUI.OnSkillUIEvent -= OnSkillUIEvent;
            player.SkillModule.OnSkillEvent -= OnSkillEvent;
            descriptionUI.UnregisterEvents();
            itemsUI.UnregisterEvents();
        }

        protected override void Start() {
            base.Start();
            for (var i = 0; i < tabs.Length; i++) 
                tabs[i].onValueChanged.AddListener(OnClickTab(i));
        }

        public override void ClearPanel() {
            tabs[0].isOn = true;
            itemsUI.Clear();
            descriptionUI.Clear();
            descriptionUI.Visible = false;
        }

        protected override void OnVisible() {
            itemsUI.SetSkills(YisoWeapon.AttackType.Slash);
            descriptionUI.ChangeWeapon(YisoWeapon.AttackType.Slash);
            SetSkillPoint(player.SkillModule.SkillPoint);
        }

        private void OnSkillUIEvent(YisoMenuSkillUIEventArgs args) {
            var stageId = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();
            switch (args) {
                case YisoMenuSkillSelectedUIEventArgs selectedArgs:
                    descriptionUI.SetSkill(selectedArgs.Skill, stageId);
                    descriptionUI.Visible = true;
                    break;
                case YisoMenuSkillUnSelectedUIEventArgs unSelectedArgs:
                    descriptionUI.Clear();
                    descriptionUI.Visible = false;
                    break;
            }
        }

        private void OnSkillEvent(YisoPlayerSkillEventArgs args) {
            switch (args) {
                case YisoPlayerSkillPointChangedEventArgs spChangedArgs:
                    SetSkillPoint(spChangedArgs.AfterPoint);
                    break;
                case YisoPlayerSkillUpgradedEventArgs upgradedArgs:
                    itemsUI.UpdateSkill(upgradedArgs.UpgradedSkill);
                    if (!descriptionUI.Visible) return;
                    descriptionUI.UpdateSkill(upgradedArgs.UpgradedSkill);
                    break;
            }
        }

        private void SetSkillPoint(int skillPoint) {
            skillPointText.SetText(skillPoint.ToCommaString());
        }

        public override YisoMenuTypes GetMenuType() => YisoMenuTypes.SKILL;

        private UnityAction<bool> OnClickTab(int index) => flag => {
            currentWeapon = index switch {
                0 => YisoWeapon.AttackType.Slash,
                1 => YisoWeapon.AttackType.Thrust,
                _ => YisoWeapon.AttackType.Shoot
            };

            if (!flag) {
                itemsUI.Clear();
                descriptionUI.Clear();
                descriptionUI.Visible = false;
                return;
            }
            itemsUI.SetSkills(currentWeapon);
            descriptionUI.ChangeWeapon(currentWeapon);
        };
    }
}