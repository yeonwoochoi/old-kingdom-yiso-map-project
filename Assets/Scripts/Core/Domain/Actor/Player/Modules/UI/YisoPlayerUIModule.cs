using System;
using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Domain.Actor.Player.Modules.Base;
using Core.Domain.Data;
using Core.Domain.Item;
using Core.Domain.Skill;
using Unity.VisualScripting;
using UnityEngine.Events;
using Utils.Extensions;

namespace Core.Domain.Actor.Player.Modules.UI {
    public class YisoPlayerUIModule : YisoPlayerBaseModule {
        public static readonly int SLOT_COUNT = 4;

        public event UnityAction<YisoPlayerSlotUIEventArgs> OnSlotEvent;

        private readonly Dictionary<YisoWeapon.AttackType, SkillSlotItem[]> slotItems = new();

        public YisoPlayerUIModule(YisoPlayer player) : base(player) {
            foreach (var weapon in EnumExtensions.Values<YisoWeapon.AttackType>()
                         .Where(t => t != YisoWeapon.AttackType.None)) {
                slotItems[weapon] = new SkillSlotItem[SLOT_COUNT];
                for (var i = 0; i < SLOT_COUNT; i++) {
                    slotItems[weapon][i] = new SkillSlotItem();
                }
            }
        }

        public bool TryGetSkill(YisoWeapon.AttackType weapon, int position, out YisoSkill skill) {
            skill = null;
            if (!slotItems[weapon][position].Exist) return false;
            skill = slotItems[weapon][position].Skill;
            return true;
        }

        public void SetSkill(YisoWeapon.AttackType weapon, int position, YisoSkill skill) {
            if (TryFindSkillPosition(weapon, skill.Id, position, out var otherPosition)) UnSetSkill(weapon, otherPosition, false);

            if (!slotItems[weapon][position].Exist) {
                slotItems[weapon][position].SetSKill(skill);
                RaiseEvent(new YisoPlayerSlotSkillSetEventArgs(weapon, position, skill));
                player.SaveData();
                return;
            }
            
            slotItems[weapon][position].SetSKill(skill);
            RaiseEvent(new YisoPlayerSlotSkillReplaceEventArgs(weapon, position, skill));
            player.SaveData();
        }
        
        public void UnSetSkill(YisoWeapon.AttackType weapon, int position, bool save = true) {
            if (!slotItems[weapon][position].Exist)
                throw new ArgumentException($"{position} does not have skill!", nameof(position));
            var skill = slotItems[weapon][position].Skill;
            slotItems[weapon][position].Reset();
            RaiseEvent(new YisoPlayerSlotSkillUnSetEventArgs(weapon, position, skill));
            if (save) player.SaveData();
        }

        public bool TryFindSkillPosition(YisoWeapon.AttackType weapon, int id, out int position) =>
            TryFindSkillPosition(weapon, id, -1, out position);

        public void SaveData(ref YisoPlayerData data) {
            foreach (var type in EnumExtensions.Values<YisoWeapon.AttackType>().Where(t => t != YisoWeapon.AttackType.None)) {
                var typeInt = (int) type;
                if (!data.uiData.slots.ContainsKey(typeInt))
                    data.uiData.slots[typeInt] = new int[SLOT_COUNT];

                for (var i = 0; i < SLOT_COUNT; i++) {
                    var slot = slotItems[type][i];
                    data.uiData.slots[typeInt][i] = slot.Exist ? slot.Skill.Id : -1;
                }
            }
        }

        public void LoadData(YisoPlayerData data) {
            foreach (var type in EnumExtensions.Values<YisoWeapon.AttackType>()
                         .Where(t => t != YisoWeapon.AttackType.None)) {
                var typeInt = (int) type;
                for (var i = 0; i < SLOT_COUNT; i++) {
                    var skillId = data.uiData.slots[typeInt][i];
                    if (skillId == -1) continue;
                    var skill = player.SkillModule.GetSkillOrElseThrow(skillId);
                    slotItems[type][i].SetSKill(skill);
                }
            }
        }

        private bool TryFindSkillPosition(YisoWeapon.AttackType weapon, int id, int ignore, out int position) {
            position = -1;

            for (var i = 0; i < SLOT_COUNT; i++) {
                if (i == ignore) continue;
                if (!slotItems[weapon][i].TryGetId(out var skillId)) continue;
                if (id != skillId) continue;
                position = i;
                break;
            }

            return position != -1;
        }

        private void RaiseEvent(YisoPlayerSlotUIEventArgs args) {
            OnSlotEvent?.Invoke(args);
        }

        private class SkillSlotItem {
            public bool Exist => Skill != null;
            public int Id => Skill?.Id ?? -1;

            public YisoSkill Skill { get; private set; } = null;

            public void SetSKill(YisoSkill skill) {
                this.Skill = skill;
            }

            public void Reset() {
                Skill = null;
            }

            public bool TryGetId(out int id) {
                id = -1;
                if (Skill == null) return false;
                id = Skill.Id;
                return true;
            }
        }
    }
}