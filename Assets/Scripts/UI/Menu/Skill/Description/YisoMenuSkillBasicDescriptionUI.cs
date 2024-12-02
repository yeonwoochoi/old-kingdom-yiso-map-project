using Core.Domain.Locale;
using Core.Domain.Skill;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Skill.Description {
    public class YisoMenuSkillBasicDescriptionUI : YisoUIController {
        [SerializeField] private Image skillImage;
        [SerializeField] private TextMeshProUGUI skillTitleText;
        [SerializeField] private TextMeshProUGUI skillTypeText;
        [SerializeField] private TextMeshProUGUI masterLevelText;
        [SerializeField] private TextMeshProUGUI currentLevelText;
        [SerializeField, Title("Lock")] private CanvasGroup lockPanelCanvas;
        [SerializeField] private TextMeshProUGUI lockPanelText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private CanvasGroup currentLevelTextCanvas;

        protected override void Start() {
            base.Start();
            currentLevelTextCanvas = currentLevelText.GetComponent<CanvasGroup>();
        }

        public void SetSkill(YisoSkill skill, int stageId) {
            skillImage.sprite = skill.Icon;
            skillTitleText.SetText(skill.GetName(CurrentLocale));
            skillTypeText.SetText(GetSkillType(skill));
            masterLevelText.SetText($"{GetMasterLevelStr()}: {skill.MasterLevel}");
            descriptionText.SetText(skill.GetDescription(CurrentLocale));
            
            SetCurrentOrLock(skill, stageId);
        }

        public void UpdateSkill(YisoSkill updateSkill, int stageId) {
            SetCurrentOrLock(updateSkill, stageId);
        }

        private void SetCurrentOrLock(YisoSkill skill, int stageId) {
            if (skill.IsLocked(stageId)) {
                currentLevelTextCanvas.Visible(false);
                lockPanelCanvas.Visible(true);
                lockPanelText.SetText(GetUnlockStr(skill));
            } else {
                lockPanelCanvas.Visible(false);
                currentLevelTextCanvas.Visible(true);
                var currentLevel = skill.IsLearned ? skill.Level : 0;
                currentLevelText.SetText($"{GetCurrentLevelStr()}: {currentLevel}");
            }
        }

        public void Clear() {
            currentLevelTextCanvas.Visible(false);
            lockPanelCanvas.Visible(false);
        }
        
        private string GetMasterLevelStr() => CurrentLocale == YisoLocale.Locale.KR ? "최종 레벨" : "Master Level";
        private string GetCurrentLevelStr() => CurrentLocale == YisoLocale.Locale.KR ? "현재 레벨" : "Current Level";
        
        
        private string GetSkillType(YisoSkill skill) {
            var type = skill.Type;
            if (type == YisoSkill.Types.ACTIVE)
                return CurrentLocale == YisoLocale.Locale.KR ? "공격형" : "Active";
            return CurrentLocale == YisoLocale.Locale.KR ? "지속형" : "Passive";
        }
        
        private string GetUnlockStr(YisoSkill skill) {
            var stageId = skill.UnlockStageId;
            return CurrentLocale == YisoLocale.Locale.KR ? $"스테이지 {stageId} 이후 잠금해제 가능" : $"Unlockable after stage {stageId}";
        }
    }
}