using System;
using Character.Ability;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace Test.UI.Render {
    public class TestRenderUI : MonoBehaviour {
        public YisoCharacterHandleSkill handleSkill;
        
        private CanvasGroup renderCanvas;

        private void Awake() {
            renderCanvas = GetComponent<CanvasGroup>();
        }

        [Button]
        public void ShowRenderPanel() {
            renderCanvas.Visible(true);
        }

        public void OnClickSkillButton(int index) {
            switch (index) {
                case 1:
                    handleSkill.SpinAttack();
                    break;
                case 2:
                    handleSkill.TeleportAttack();
                    break;
                case 3:
                    handleSkill.ShatterThrust();
                    break;
                case 4:
                    handleSkill.DaggerTempest();
                    break;
                case 5:
                    handleSkill.EvasiveAttack();
                    break;
            }
        }
    }
}