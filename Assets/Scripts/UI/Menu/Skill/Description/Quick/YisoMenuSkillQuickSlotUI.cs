using Core.Behaviour;
using Core.Domain.Skill;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu.Skill.Description.Quick {
    public class YisoMenuSkillQuickSlotUI : RunIBehaviour {
        [SerializeField] private Image skillImage;
        [SerializeField] private Button button;

        public int SkillId { get; set; } = -1;
        
        public Button SlotButton => button;

        public void SetSkill(YisoSkill skill) {
            skillImage.sprite = skill.Icon;
            skillImage.enabled = true;
            SkillId = skill.Id;
        }

        public void Clear() {
            SkillId = -1;
            skillImage.enabled = false;
            skillImage.sprite = null;
        }
    }
}