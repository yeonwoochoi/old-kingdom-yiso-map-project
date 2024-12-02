using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Behaviour;
using Core.Domain.Actor.Player;
using Core.Domain.Actor.Player.Modules.UI;
using Core.Service;
using Core.Service.UI.Menu;
using DG.Tweening;
using Sirenix.OdinInspector;
using UI.Base;
using UI.HUD.QuickSlots;
using UI.Menu.Skill.Description.Quick;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;
using Utils.Extensions;

namespace UI.Menu.Skill.Description {
    public class YisoMenuSkillQuickSlotsUI : YisoPlayerUIController {
        [SerializeField, Title("Image")] private Image defaultImage;
        [SerializeField, Title("Sprites")] private Sprite swordSprite;
        [SerializeField] private Sprite spearSprite;
        [SerializeField] private Sprite bowSprite;
        public event UnityAction<int> OnClickSlotEvent;
        
        [SerializeField] private List<YisoMenuSkillQuickSlotUI> slotButtons;

        private readonly List<Material> buttonMaterials = new();
        private readonly List<IEnumerator> shineCoroutines = new();

        private Canvas canvas = null;
        private GraphicRaycaster graphicRaycaster = null;
        
        private CanvasGroup canvasGroup;

        public bool Visible {
            get => canvasGroup.IsVisible();
            set => canvasGroup.Visible(value);
        }

        public void RegisterEvents() {
            player.UIModule.OnSlotEvent += OnUISlotEvent;
        }

        public void UnregisterEvents() {
            player.UIModule.OnSlotEvent -= OnUISlotEvent;
        }

        public void ChangeWeapon(YisoWeapon.AttackType weapon) {
            var uiModule = player.UIModule;
            
            for (var i = 0; i < 4; i++) {
                if (!uiModule.TryGetSkill(weapon, i, out var skill)) continue;
                slotButtons[i].SetSkill(skill);
            }

            defaultImage.sprite = weapon switch {
                YisoWeapon.AttackType.Thrust => spearSprite,
                YisoWeapon.AttackType.Shoot => bowSprite,
                _ => swordSprite
            };
        }
        
        protected override void Start() {
            base.Start();
            canvasGroup = GetComponent<CanvasGroup>();

            for (var i = 0; i < slotButtons.Count; i++) {
                buttonMaterials.Add(slotButtons[i].SlotButton.image.material);
                slotButtons[i].SlotButton.onClick.AddListener(OnClickButton(i));
            }

            foreach (var button in slotButtons) {
                buttonMaterials.Add(button.SlotButton.image.material);
            }
        }

        private void OnUISlotEvent(YisoPlayerSlotUIEventArgs args) {
            switch (args) {
                case YisoPlayerSlotSkillSetEventArgs setArgs:
                    slotButtons[setArgs.Position].SetSkill(setArgs.Skill);
                    break;
                case YisoPlayerSlotSkillUnSetEventArgs unSetArgs:
                    slotButtons[unSetArgs.Position].Clear();
                    break;
                case YisoPlayerSlotSkillReplaceEventArgs replaceArgs:
                    slotButtons[replaceArgs.Position].SetSkill(replaceArgs.Skill);
                    break;
            }
        }

        public void ActiveSelectionMode(bool flag) {
            if (flag) {
                Interact(true);
                CreateCanvas();
                ShowSines();
                return;
            }
            
            Interact(false);
            RemoveCanvas();
            HideShines();
        }

        private void Interact(bool flag) {
            foreach (var button in slotButtons) {
                button.SlotButton.interactable = flag;
            }
        }

        private void CreateCanvas() {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 11;
            graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
        }

        private void RemoveCanvas() {
            if (graphicRaycaster != null) Destroy(graphicRaycaster);
            if (canvas != null) Destroy(canvas);
        }

        private void ShowSines() {
            foreach (var coroutine in buttonMaterials.Select(DOShine)) {
                StartCoroutine(coroutine);
                shineCoroutines.Add(coroutine);
            }
        }

        private void HideShines() {
            foreach (var coroutine in shineCoroutines) {
                StopCoroutine(coroutine);
            }
            
            shineCoroutines.Clear();
        }

        private UnityAction OnClickButton(int index) => () => {
            OnClickSlotEvent?.Invoke(index);
        };

        public void Clear() {

        }

        private IEnumerator DOShine(Material material) {
            material.ActiveShine(true);
            while (true) {
                yield return material.DOShine(1f, 0.5f).WaitForCompletion();
                material.SetShineLocation(0f);
                yield return YieldInstructionCache.WaitForSeconds(0.5f);
            }
        }
    }
}