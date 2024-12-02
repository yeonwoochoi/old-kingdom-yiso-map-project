using System;
using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Domain.Actor.Player.Modules.Base;
using Core.Domain.Data;
using Core.Domain.Skill;
using Core.Service;
using Core.Service.CoolDown;
using Core.Service.Domain;
using UnityEngine.Events;
using Utils.Extensions;

namespace Core.Domain.Actor.Player.Modules.Skill {
    public class YisoPlayerSkillModule : YisoPlayerBaseModule {
        public event UnityAction<YisoPlayerSkillEventArgs> OnSkillEvent;
        
        private readonly Dictionary<int, YisoSkill> skills;
        private readonly Dictionary<YisoWeapon.AttackType, Dictionary<YisoSkill.Types, List<int>>> organizedSkills = new();

        private int skillPoint = 50;
        
        public int SkillPoint {
            get => skillPoint;
            private set {
                var temp = skillPoint;
                skillPoint = value;
                RaiseOnSKillEvent(new YisoPlayerSkillPointChangedEventArgs(temp, value));
            }
        }

        public void AddSkillPoint(int sp) {
            SkillPoint += sp;
        }

        public YisoPlayerSkillModule(YisoPlayer player) : base(player) {
            var sortedSkills = GetService<IYisoDomainService>().GetSkills()
                .OrderBy(x => x.Value.UnlockStageId);
            
            skills = new Dictionary<int, YisoSkill>(sortedSkills);
            Organize();
        }

        public List<YisoSkill> GetSkillsByTypeAndWeapon(YisoWeapon.AttackType weapon, YisoSkill.Types type) {
            var skillIds = organizedSkills[weapon][type];
            return skillIds.Select(id => skills[id]).ToList();
        }

        public void CastSkill(int skillId) {
            var skill = skills[skillId];
            if (skill is not YisoActiveSkill)
                throw new ArgumentException($"Skill(id={skillId}) is not active skill! casting skill must be active", nameof(skillId));
            RaiseOnSKillEvent(new YisoPlayerSkillCastEventArgs(skillId));
        }

        public void UpgradeSkill(int skillId) {
            if (skillPoint <= 0) throw new ArgumentException($"Skill point not enough to upgrade skill(id={skillId})");
            var skill = skills[skillId];
            skill.LevelUp();
            SkillPoint--;
            RaiseOnSKillEvent(new YisoPlayerSkillUpgradedEventArgs(skill));
            player.SaveData();
        }

        public bool TryGetSkillById(int id, out YisoSkill skill) => skills.TryGetValue(id, out skill);
        
        public YisoSkill GetSkillOrElseThrow(int id) => skills.TryGetValue(id, out var skill) ? skill : throw new ArgumentException($"Skill(id={id}) not exists");

        public T GetSkillOrElseThrow<T>(int id) where T : YisoSkill => (T) GetSkillOrElseThrow(id);

        public bool SkillIs(int id, YisoWeapon.AttackType attackType) => GetSkillOrElseThrow(id).AttackType == attackType;

        public bool SkillIs(int id, YisoSkill.Types type) => GetSkillOrElseThrow(id).Type == type;

        public bool SkillIs(int id, YisoWeapon.AttackType attackType, YisoSkill.Types type) {
            var skill = GetSkillOrElseThrow(id);
            return skill.AttackType == attackType && skill.Type == type;
        }

        public bool SkillIs(int id, YisoSkill.Types type, YisoWeapon.AttackType attackType) =>
            SkillIs(id, attackType, type);

        public void ResetData() {
            foreach (var skill in skills.Values) skill.Reset();
        }

        private void Organize() {
            ResetOrganizeDict();
            
            foreach (var (id, skill) in skills) {
                var weapon = skill.AttackType;
                var type = skill.Type;
                organizedSkills[weapon][type].Add(id);
            }
        }

        private void ResetOrganizeDict() {
            foreach (var weapon in EnumExtensions.Values<YisoWeapon.AttackType>()
                         .Where(t => t != YisoWeapon.AttackType.None)) {
                organizedSkills[weapon] = new Dictionary<YisoSkill.Types, List<int>>();
                foreach (var type in EnumExtensions.Values<YisoSkill.Types>()) {
                    organizedSkills[weapon][type] = new List<int>();
                }
            }
        }

        public void LoadData(YisoPlayerData data) {
            var skillData = data.skillData.skillData;
            SkillPoint = data.skillData.skillPoint;
            foreach (var (id, level) in skillData) {
                if (!skills.TryGetValue(id, out var skill))
                    throw new ArgumentException($"Skill(id={id}) not exists!", nameof(id));
                skill.Load(level);
            }
        }
 
        public void SaveData(ref YisoPlayerData data) {
            var savedData = skills.Where(pair => pair.Value.IsLearned)
                .ToDictionary(pair => pair.Key, pair => pair.Value.Level);
            
            data.skillData.skillData = new Dictionary<int, int>(savedData);
            data.skillData.skillPoint = SkillPoint;
        }

        private void RaiseOnSKillEvent(params YisoPlayerSkillEventArgs[] argsArray) {
            foreach (var args in argsArray) RaiseOnSKillEvent(args);
        }
        
        private void RaiseOnSKillEvent(YisoPlayerSkillEventArgs args) {
            OnSkillEvent?.Invoke(args);
        }
    }
}