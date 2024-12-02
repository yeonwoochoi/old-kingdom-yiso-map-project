using System.Collections;
using System.Collections.Generic;
using Character.Ability;
using Core.Behaviour;
using MEC;
using UI.Menu.Skill.Cam;
using UnityEngine;
using Utils;
using Utils.Extensions;
using Utils.ObjectId;

namespace UI.Menu.Skill.Description.Preview {
    public class YisoMenuSkillPreviewUI : RunIBehaviour {
        [SerializeField] private YisoMenuSkillUICamera uiCamera;
        [SerializeField] private YisoCharacterHandleSkill skillHandler;

        private CanvasGroup canvasGroup;

        private IEnumerator loopCoroutine = null;
        private int skillId;

        public bool Visible {
            get => canvasGroup.IsVisible();
            set => canvasGroup.Visible(value);
        }

        protected override void OnEnable() {
            base.OnEnable();
            skillHandler.OnSkillEndEvent += OnSkillEnd;
        }

        protected override void OnDisable() {
            base.OnDisable();
            skillHandler.OnSkillEndEvent -= OnSkillEnd;
        }

        protected override void Start() {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void CastSkill(int skillId) {
            this.skillId = skillId;
            loopCoroutine = DOSkillLoop2(skillId);
            StartCoroutine(loopCoroutine);
        }

        public void StopSkill() {
            if (loopCoroutine != null) {
                StopCoroutine(loopCoroutine);
                loopCoroutine = null;
            }
            
            skillHandler.StopSkillCast(skillId);
            skillEnd = false;
        }

        private bool skillEnd = false;

        private void OnSkillEnd() {
            skillEnd = true;
        }

        private IEnumerator DOSkillLoop2(int skillId) {
            yield return YieldInstructionCache.WaitForSeconds(0.5f);
            
            while (true) {
                var timer = 1.5f;
                skillEnd = false;
                skillHandler.StartSkillCast(skillId);
                while (timer > 0 || !skillEnd) {
                    timer -= Time.deltaTime;
                    yield return Timing.WaitForOneFrame;
                }
            }
        }
    }
}