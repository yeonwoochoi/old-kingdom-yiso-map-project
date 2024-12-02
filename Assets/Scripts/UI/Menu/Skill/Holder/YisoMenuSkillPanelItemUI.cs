using Core.Domain.Skill;
using Core.Service;
using Core.Service.Stage;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Skill.Holder {
    public class YisoMenuSkillPanelItemUI : YisoUIController {
        [SerializeField, Title("Toggle")] private Toggle panelToggle;
        [SerializeField, Title("Skill")] private Image skillImage;
        [SerializeField] private TextMeshProUGUI skillTitleText;
        [SerializeField] private TextMeshProUGUI skillPointText;
        [SerializeField, Title("Image Canvas")] private CanvasGroup lockCanvas;
        [SerializeField] private CanvasGroup checkCanvas;

        public Toggle Toggle => panelToggle;

        public YisoSkill Skill { get; private set; } = null;

        public void SetSkill(YisoSkill skill) {
            this.Skill = skill;
            skillImage.sprite = skill.Icon;
            skillTitleText.SetText(skill.GetName(CurrentLocale));
            SetPointText();
        }

        public void UpdateSkill(YisoSkill updateSkill) {
            Skill = updateSkill;
            SetPointText();
        }

        private void SetPointText() {
            skillPointText.SetText($"[{Skill.Level}/{Skill.MasterLevel}]");
        }

        public void SetSlot(bool slot) {
            checkCanvas.Visible(slot);
        }

        public void SetLock(bool locked) {
            lockCanvas.Visible(locked);
        }

        public void Clear() {
            Skill = null;
            skillTitleText.SetText("");
            skillPointText.SetText("");
            lockCanvas.Visible(false);
            checkCanvas.Visible(false);
        }
    }
}