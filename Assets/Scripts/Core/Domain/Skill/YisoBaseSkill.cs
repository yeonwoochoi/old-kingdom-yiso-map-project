using System;
using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Domain.Locale;
using Core.Domain.Types;
using UnityEngine;
using Utils.Extensions;

namespace Core.Domain.Skill {
    public abstract class YisoSkill {
        private readonly YisoLocale name;
        private readonly YisoLocale description;
        public int Id { get; }

        public int UnlockStageId { get; }
        public Sprite Icon { get; }
        public YisoWeapon.AttackType AttackType { get; }
        public Types Type { get; }
        
        public int MasterLevel { get; }

        private int level;

        public int Level {
            get => level;
            private set {
                if (IsMasterLevel) return;
                if (value > MasterLevel || value < 0) return;
                level = value;
            }
        }

        public bool IsLearned => level > 0;

        public bool IsMasterLevel => level == MasterLevel;

        public bool IsLocked(int currentStageId) => currentStageId < UnlockStageId;

        public void LevelUp() {
            if (IsMasterLevel) return;
            Level += 1;
            OnLevelUp();
            if (level == 1) OnUnlocked();
        }

        protected YisoSkill(YisoSkillSO so) {
            Id = so.id;
            MasterLevel = so.masterLevel;
            UnlockStageId = so.unlockStageId;
            name = so.name;
            description = so.description;
            Icon = so.icon;
            Type = so.type;
            AttackType = so.attackType;
        }
        
        protected virtual void OnLevelUp() { }
        protected virtual void OnUnlocked() { }

        public string GetName(YisoLocale.Locale locale) => name[locale];
        public string GetDescription(YisoLocale.Locale locale) => description[locale];

        public virtual void Load(int level) {
            this.level = level;
        }

        public virtual void Reset() {
            Level = 0;
        }
        
        public enum Types {
            PASSIVE, ACTIVE
        }
    }
    
    public sealed class YisoPassiveSkill : YisoSkill {
        public List<EffectInfo> EffectInfos { get; } = new();
            
        public YisoPassiveSkill(YisoSkillSO so) : base(so) {
            EffectInfos.AddRange(so.passiveEffects.Select(effect => new EffectInfo(effect)));
        }

        public class EffectInfo {
            public YisoBuffEffectTypes Effect { get; }
            public int InitValue { get; }
            public int IncreasePerLevelValue { get; }

            public EffectInfo(YisoSkillSO.Effects effect) {
                Effect = effect.type;
                InitValue = effect.initValue;
                IncreasePerLevelValue = effect.increasePerLevelValue;
            }

            public int GetCurrentValue(int level) => InitValue + (1 * IncreasePerLevelValue);
        }
    }
    
    public sealed class YisoActiveSkill : YisoSkill {
        public int AttackCount { get; }
        public int DamageRate { get; }
        public int CriticalRate { get; }
        public int CriticalDamageRate { get; }
        public double CoolDown { get; }

        public int IncAttackCount { get; }
        public int IncDamageRate { get; }
        public int IncCriticalRate { get; }
        public int IncCriticalDamageRate { get; }
        
        public int DecCoolDown { get; }

        public bool ExistCoolDown => CoolDown != 0;
        
        

        private Dictionary<int, string[]> effectStrings = new();

        public YisoActiveSkill(YisoSkillSO so) : base(so) {
            AttackCount = so.active.attackCount;
            if (AttackCount == 0) AttackCount = 1;
            DamageRate = so.active.damageRate;
            if (DamageRate == 0) DamageRate = 120;
            CriticalRate = so.active.criticalRate;
            CriticalDamageRate = so.active.criticalDamageRate;
            CoolDown = so.active.cooldown;

            IncAttackCount = so.active.attackIncCount;
            IncDamageRate = so.active.damageIncRate;
            IncCriticalRate = so.active.criticalIncRate;
            IncCriticalDamageRate = so.active.criticalDamageIncRate;
            DecCoolDown = so.active.coolDownDec;
        }

        public string[] GetEffectStrings(int level, YisoLocale.Locale locale) {
            if (effectStrings.Count == 0) CreateEffectStrings(locale);
            return effectStrings[level];
        }

        private void CreateEffectStrings(YisoLocale.Locale locale) {
            for (var level = 1; level <= MasterLevel; level++) {
                effectStrings[level] = CreateEffectString(level, locale).ToArray();
            }
        }

        private List<string> CreateEffectString(int level, YisoLocale.Locale locale) {
            var result = new List<string>();
            
            var attackCountStr = locale == YisoLocale.Locale.KR ? "공격 횟수" : "Attack Count";
            result.Add($"{attackCountStr}: {CalculateInc(level, AttackCount, IncAttackCount)}");

            var damageRateStr = locale == YisoLocale.Locale.KR ? "스킬 대미지(%)" : "Skill Damage";
            result.Add($"{damageRateStr}: {CalculateInc(level, DamageRate, IncDamageRate).ToPercentage()}");

            if (CriticalRate != 0) {
                var criticalRateStr = locale == YisoLocale.Locale.KR ? "치명타 확률(%)" : "Critical Rate(%)";
                result.Add($"{criticalRateStr}: {CalculateInc(level, CriticalRate, IncCriticalRate).ToPercentage()}");
            }

            if (CriticalDamageRate != 0) {
                var criticalDamageStr = locale == YisoLocale.Locale.KR ? "치명타 대미지(%)" : "Critical Damage(%)";
                result.Add($"{criticalDamageStr}: {CalculateInc(level, CriticalDamageRate, IncCriticalDamageRate).ToPercentage()}");
            }

            if (CoolDown != 0) {
                var cooldownValue = CoolDown * Math.Pow(1 - DecCoolDown.ToNormalized(), level - 1);
                var cooldownValueStr = cooldownValue.ToString("0.00") + (locale == YisoLocale.Locale.KR ? "초" : "Sec");
                var coolDownStr = locale == YisoLocale.Locale.KR ? "재사용 대기 시간" : "Cooldown";
                result.Add($"{coolDownStr}: {cooldownValueStr}");
            }

            return result;
            int CalculateInc(int level, int initValue, int incValue) => initValue + (incValue * (level - 1));
        }
    }
}