using System;
using Core.Behaviour;
using Core.Domain.CoolDown;
using Core.Domain.Skill;
using Core.Service;
using Core.Service.CoolDown;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD.QuickSlots {
    public class YisoPlayerHudQuickSkillSlotButtonUI : RunIBehaviour {
        [SerializeField, Title("Image")] private Image skillImage;
        [SerializeField] private Image coolTimeImage;
        [SerializeField, Title("Cooldown")] private TextMeshProUGUI coolDownText;
        [SerializeField] private Button button;
        private YisoPlayerCoolDownHolder holder;

        private readonly YisoPlayerCoolDownHolder.Index coolDownIndex = new();
        private IYisoCoolDownService coolDownService;
        private YisoActiveSkill activeSkill;
        
        public int SkillId { get; set; } = -1;
        public int SlotIndex { get; set; } = -1;

        public Button SlotButton => button;

        protected override void Start() {
            coolDownService = YisoServiceProvider.Instance.Get<IYisoCoolDownService>();
        }

        public void Clear() {
            SkillId = -1;
            activeSkill = null;
            if (skillImage.enabled) skillImage.enabled = false;
            if (coolTimeImage.enabled) {
                ClearCooldown();
            }
            button.interactable = false;
            if (holder == null) return;
            holder.Clear(coolDownIndex);
            holder = null;
        }

        public void Interact(bool flag) {
            if (flag && coolDownIndex.IsCool) return;
            if (button == null) return;
            button.interactable = flag;
        }

        public void SetSkill(YisoSkill skill) {
            activeSkill = skill as YisoActiveSkill;
            SkillId = skill.Id;
            button.interactable = true;
            skillImage.sprite = skill.Icon;
            skillImage.enabled = true;
            if (!coolDownService.TryGetCoolDown(SkillId, out holder)) return;
            CooldownMode(true);
            AddCooldownListener();
        }

        public void CreateCooldown() {
            if (!activeSkill.ExistCoolDown) return;
            var id = coolDownService.CreateCoolDown(SkillId, activeSkill.CoolDown);
            coolDownService.TryGetCoolDown(id, out holder);
            CooldownMode(true);
            AddCooldownListener();
        }

        private void AddCooldownListener() {
            coolDownIndex.Progress = holder.AddCoolDownProgress(progress => {
                if (holder == null) return;
                coolTimeImage.fillAmount = progress;
                var originalProgress = progress * holder.CoolDown;
                var text = originalProgress >= 1 ? $"{originalProgress:0}" : $"{originalProgress:0.0}";
                coolDownText.SetText(text);
            });

            coolDownIndex.Complete = holder.AddCoolDownComplete(ClearCooldown);
        }

        private void ClearCooldown() {
            CooldownMode(false);
        }

        private void CooldownMode(bool flag) {
            coolDownIndex.IsCool = flag;
            coolTimeImage.fillAmount = 0;
            coolTimeImage.enabled = flag;
            coolDownText.enabled = flag;
            button.interactable = !flag;
            if (!flag) holder?.Clear(coolDownIndex);
        }
    }
}