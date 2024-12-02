using System;
using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Domain.Actor.Player.Modules.Skill;
using Core.Domain.Actor.Player.Modules.UI;
using Core.Domain.Skill;
using Core.Service;
using Core.Service.Stage;
using Sirenix.OdinInspector;
using UI.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Menu.Skill.Holder {
    public class YisoMenuSkillPanelItemsUI : YisoPlayerUIController {
        public event UnityAction<YisoMenuSkillUIEventArgs> OnSkillUIEvent; 
        
        [SerializeField, Title("Prefab")] private GameObject itemPrefab;
        [SerializeField] private GameObject passiveContent;
        [SerializeField] private GameObject activeContent;
        [SerializeField, Title("Toggle")] private ToggleGroup toggleGroup;
        [SerializeField, Title("Scroll")] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRect;

        private readonly Dictionary<YisoSkill.Types, List<SkillItem>> itemDict = new();

        private YisoWeapon.AttackType currentWeapon = YisoWeapon.AttackType.Slash;

        private (YisoSkill.Types type, int index) cachedIndex = (YisoSkill.Types.ACTIVE, -1);
        
        protected override void Start() {
            base.Start();
            foreach (var type in EnumExtensions.Values<YisoSkill.Types>()) {
                itemDict[type] = new List<SkillItem>();
            }
        }

        public void RegisterEvents() {
            player.UIModule.OnSlotEvent += OnSlotUIEvent;
        }

        public void UnregisterEvents() {
            player.UIModule.OnSlotEvent -= OnSlotUIEvent;
        }

        public void SetSkills(YisoWeapon.AttackType weapon) {
            currentWeapon = weapon;
            Clear();
            
            foreach (var type in itemDict.Keys) SetSkills(type);
        }

        public void UpdateSkill(YisoSkill updateSkill) {
            var index = GetItemIndexBySkillId(updateSkill.Type, updateSkill.Id);
            var stageId = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();
            itemDict[updateSkill.Type][index].Item.UpdateSkill(updateSkill);
            itemDict[updateSkill.Type][index].Item.SetLock(updateSkill.IsLocked(stageId));
        }

        public void Clear() {
            foreach (var item in itemDict.Values.SelectMany(i => i)) {
                item.SetActive(false);
            }

            scrollRect.verticalNormalizedPosition = 1f;

            if (cachedIndex.index != -1) {
                itemDict[cachedIndex.type][cachedIndex.index].Item.Toggle.isOn = false;
            }
        }

        private void OnSlotUIEvent(YisoPlayerSlotUIEventArgs args) {
            if (args.Weapon != player.InventoryModule.GetCurrentEquippedWeaponType()) return;
            var position = args.Position;
            var itemIndex = -1;
            
            YisoSkill.Types type;
            var skillId = -1;
            var slotFlag = false;
            switch (args) {
                case YisoPlayerSlotSkillSetEventArgs setArgs:
                    type = setArgs.Skill.Type;
                    skillId = setArgs.Skill.Id;
                    slotFlag = true;
                    break;
                case YisoPlayerSlotSkillUnSetEventArgs unSetArgs:
                    type = unSetArgs.Skill.Type;
                    skillId = unSetArgs.Skill.Id;
                    break;
                case YisoPlayerSlotSkillReplaceEventArgs replaceArgs:
                    type = replaceArgs.Skill.Type;
                    skillId = replaceArgs.Skill.Id;
                    slotFlag = true;
                    break;
                default:
                    throw new ArgumentException("No Args exist", nameof(args));
            }
            
            itemIndex = GetItemIndexBySkillId(type, skillId);
            itemDict[type][itemIndex].Item.SetSlot(slotFlag);
        }

        private void SetSkills(YisoSkill.Types type) {
            var skills = player.SkillModule.GetSkillsByTypeAndWeapon(currentWeapon, type);
            var stageService = YisoServiceProvider.Instance.Get<IYisoStageService>();
            var stageId = stageService.GetCurrentStageId();
            
            foreach (var skill in skills) {
                var index = GetEmptyItemOrCreate(type);
                var item = itemDict[type][index];
                item.SetActive(true);
                item.Item.SetSkill(skill);
                item.Item.SetLock(skill.IsLocked(stageId));
                item.Item.SetSlot(player.UIModule.TryFindSkillPosition(skill.AttackType, skill.Id, out _));
                item.Item.Toggle.group = toggleGroup;
                item.Item.Toggle.onValueChanged.AddListener(OnClickSkill(type, index));
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        private int GetItemIndexBySkillId(YisoSkill.Types type, int id) {
            var items = itemDict[type];

            for (var i = 0; i < items.Count; i++) {
                if (items[i].Item.Skill.Id != id) continue;
                return i;
            }

            throw new ArgumentException($"Skill(id={id}) not exists on {type}");
        }
        
        private int GetEmptyItemOrCreate(YisoSkill.Types type) {
            var items = itemDict[type];
            
            for (var i = 0; i < items.Count; i++) {
                if (items[i].Active) continue;
                return i;
            }

            var newItem = CreateItem(type);
            itemDict[type].Add(new SkillItem(newItem));
            return items.Count - 1;
        }

        private UnityAction<bool> OnClickSkill(YisoSkill.Types type, int index) => flag => {
            if (!flag) {
                cachedIndex.index = -1;
                cachedIndex.type = type;
                OnSkillUIEvent?.Invoke(new YisoMenuSkillUnSelectedUIEventArgs());
                return;
            }

            cachedIndex.index = index;
            cachedIndex.type = type;
            OnSkillUIEvent?.Invoke(new YisoMenuSkillSelectedUIEventArgs(itemDict[type][index].Item.Skill));
        };

        private YisoMenuSkillPanelItemUI CreateItem(YisoSkill.Types type) {
            var content = type == YisoSkill.Types.PASSIVE ? passiveContent : activeContent;
            return CreateObject<YisoMenuSkillPanelItemUI>(itemPrefab, content.transform);
        }

        private class SkillItem {
            private readonly YisoMenuSkillPanelItemUI item = null;

            public bool Active { get; private set; } = false;

            public YisoMenuSkillPanelItemUI Item => item;

            public SkillItem(YisoMenuSkillPanelItemUI item) {
                this.item = item;
                this.item.gameObject.SetActive(false);
            }

            public void SetActive(bool flag) {
                Active = flag;
                item.gameObject.SetActive(flag);
                if (!flag) item.Clear();
            }
        }
    }
}