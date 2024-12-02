using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Domain.Actor.Attack;
using Core.Domain.Actor.Enemy;
using Core.Domain.Actor.Player.Modules.Base;
using Core.Domain.Data;
using Core.Domain.Item;
using Core.Domain.Item.Equip;
using Core.Domain.Skill;
using Core.Domain.Types;
using Core.Logger;
using Core.Service;
using Core.Service.Data.Item;
using Core.Service.Factor.HonorRating;
using Core.Service.Game;
using Core.Service.Log;
using Core.Service.Stage;
using UI.Menu.Stats.Event;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Utils.Extensions;

namespace Core.Domain.Actor.Player.Modules.Stat {
    public class YisoPlayerStatModule : YisoPlayerBaseModule {
        public event UnityAction<YisoWeapon.AttackType, double> OnWeaponCombatRatingChangedEvent; 
        public event UnityAction<StatsUIEventArgs> OnStatsUIEvent;
        public event UnityAction<float> OnHpChangedEvent;

        public event UnityAction<float> OnMoveSpeedChangedEvent;
        public event UnityAction<float> OnAttackSpeedChangedEvent; 
        
        public int MaxHp { get; private set; } = 0;

        private int hp = 100;

        public int ItemDropRate { get; private set; } = 30;
        public int MoneyDropRate { get; private set; } = 100;

        public int CriticalPercent { get; private set; } = 0;
        public int CriticalDamageRate { get; private set; } = 125;

        public int SkillCoolDownReduceRate { get; private set; } = 0;

        public int Hp => hp;
        
        public float MoveSpeed {
            get => weaponMoveSpeeds[player.InventoryModule.GetCurrentEquippedWeaponType()];
            private set {
                weaponMoveSpeeds[player.InventoryModule.GetCurrentEquippedWeaponType()] = value;
                OnMoveSpeedChangedEvent?.Invoke(value);
            }
        }
        
        public float AttackSpeed {
            get => weaponAttackSpeeds[player.InventoryModule.GetCurrentEquippedWeaponType()];
            private set {
                weaponAttackSpeeds[player.InventoryModule.GetCurrentEquippedWeaponType()] = value;
                OnAttackSpeedChangedEvent?.Invoke(value);
            }
        }


        public Dictionary<YisoWeapon.AttackType, double> WeaponCombatRatings { get; } = new();

        private double BaseCombatRating { get; set; }

        public double AdditionalCombatRating { get; set; } = 0;

        private double baseWithItemCombatRating;

        private readonly List<(int sourceId, (YisoBuffEffectTypes type, int value))> buffs = new();

        private readonly Dictionary<YisoWeapon.AttackType, float> weaponMoveSpeeds = new();
        private readonly Dictionary<YisoWeapon.AttackType, float> weaponAttackSpeeds = new();
        private readonly Dictionary<YisoWeapon.AttackType, Dictionary<YisoBuffEffectTypes, int>> weaponAdditionalStats = new();
        private readonly Dictionary<YisoBuffEffectTypes, int> buffStats = new();
        public Dictionary<YisoWeapon.AttackType, (int, int)> WeaponAttackDefences { get; } = new();

        private const float ATTACK_DEFENCE_FACTOR = 0.7654321f;

        private readonly Dictionary<int, int> equipSetHandles = new();

        public YisoPlayerStatModule(YisoPlayer player) : base(player) {
            ResetSpeed(YisoWeapon.AttackType.None, true, true);
            ResetSpeed(YisoWeapon.AttackType.None, false, true);
            ResetAdditionalStats(YisoWeapon.AttackType.None, true);
            ResetBuffStats();
            ResetCombatRatings();
        }

        public void StartBuff(int sourceId, YisoBuffEffectTypes type, int value) {
            buffs.Add((sourceId, (type, value)));
            buffStats[type] += value;
            HandleBuff(type);
        }

        public void CompleteBuff(int sourceId) {
            if (buffs.Count(b => b.sourceId == sourceId) == 0) return;

            var idx = -1;
            for (var i = 0; i < buffs.Count; i++) {
                if (buffs[i].sourceId != sourceId) continue;
                idx = i;
                break;
            }

            var type = buffs[idx].Item2.type;
            var value = buffs[idx].Item2.value;
            buffs.RemoveAt(idx);
            buffStats[type] -= value;
            HandleBuff(type);
        }

        private void HandleBuff(YisoBuffEffectTypes type) {
            var currentWeapon = player.InventoryModule.GetCurrentEquippedWeaponType();
            switch (type) {
                case YisoBuffEffectTypes.MOVE_SPEED_INC:
                    SetSpeed(true);
                    OnMoveSpeedChangedEvent?.Invoke(MoveSpeed);
                    break;
                case YisoBuffEffectTypes.ATTACK_SPEED_INC:
                    SetSpeed(false);
                    OnAttackSpeedChangedEvent?.Invoke(AttackSpeed);
                    break;
                case YisoBuffEffectTypes.CR_INC:
                    CalculateCombatRatingAllWeapons();
                    break;
            }
        }

        public void SetHp(int hp) {
            this.hp = hp;
            RaiseHpChanged();
        }

        public void OnHit(YisoAttack attack, out bool death) {
            death = false;
            foreach (var info in attack.Damages) {
                var damage = info.Damage;
                var afterDamageHp = hp - Mathf.RoundToInt((float)damage);
                afterDamageHp = Mathf.Max(0, afterDamageHp);
                SetHp(afterDamageHp);
                death = hp <= 0;
                if (death) break;
            }
        }

        private void RaiseHpChanged() {
            var changedProgress = hp / (float)MaxHp;
            OnHpChangedEvent?.Invoke(changedProgress);
        }

        public void Revive() {
            SetHp(MaxHp);
        }

        public double GetCombatRatingDiff(YisoEquipItem item) {
            if (!player.InventoryModule.EquippedUnit.TryGetItem(item.Slot, out var equippedItem))
                return -item.CombatRating;

            return equippedItem.CombatRating - item.CombatRating;
        }

        public void CalculateCombatRatingAllWeapons() {
            var length = YisoEquipItemTypesUtils.VALID_ATTACK_TYPES.Length;
            for (var i = 0; i < length; i++) {
                var weapon = YisoEquipItemTypesUtils.VALID_ATTACK_TYPES[i];
                CalculateCombatRating(weapon, i == length - 1);
            }

            var swordCR = WeaponCombatRatings[YisoWeapon.AttackType.Slash];
            var spearCR = WeaponCombatRatings[YisoWeapon.AttackType.Thrust];
            var bowCR = WeaponCombatRatings[YisoWeapon.AttackType.Shoot];
            var logger = YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoPlayerStatModule>();
            logger.Debug($"[Player] Stage: {GetService<IYisoStageService>().GetCurrentStageId()}, Max HP: {MaxHp.ToCommaString()}, Click and check Combat Rating\n\nSword: {swordCR.ToCommaString()}\nBow: {bowCR.ToCommaString()}\nSpear: {spearCR.ToCommaString()}");
        }

        private void CalculateCombatRating(YisoWeapon.AttackType type, bool applyHp = false) {
            var itemService = GetService<IYisoItemService>();
            var stageService = GetService<IYisoStageService>();
            var honorRatingService = GetService<IYisoHonorRatingFactorService>();
            var playerFactor = honorRatingService.GetPlayerFactors();
            var stageCR = stageService.GetCurrentStageCR();
            BaseCombatRating = (stageCR * playerFactor.honorRating) + AdditionalCombatRating;
            MaxHp = Mathf.CeilToInt((float)(BaseCombatRating * playerFactor.maxHp));
            var equippedItems = player.InventoryModule.EquippedUnit.GetEquippedItems().ToList();
            var weaponItem = player.InventoryModule.EquippedUnit.GetWeapon(type);
            var itemCombatRating = 0d;
            
            equipSetHandles.Clear();
            ResetSpeed(type, true);
            ResetSpeed(type, false);
            ResetAdditionalStats(type);

            if (weaponItem != null) equippedItems.Add(weaponItem);
            
            foreach (var item in equippedItems) {
                itemCombatRating += item.CombatRating;

                if (item.Potentials.PotentialExist) {
                    foreach (var key in YisoEquipPotentials.KEYS) {
                        if (!item.Potentials.TryGetValue(key, out var potential)) continue;
                        weaponAdditionalStats[type][potential.Type] += potential.Value;
                    }
                }

                var setItemId = item.SetItemId;
                if (setItemId == -1) continue;
                if (!equipSetHandles.TryAdd(setItemId, 1)) equipSetHandles[setItemId]++;
            }

            foreach (var (id, value) in equipSetHandles) {
                if (!itemService.TryGetSetItem(id, out var setItem)) continue;
                if (value == 1) continue;
                var keys = new int[value - 1];
                for (var i = 0; i < keys.Length; i++) keys[i] = i + 2;

                var effects = keys.SelectMany(k => setItem.Effects[k]);
                foreach (var effect in effects) {
                    weaponAdditionalStats[type][effect.effect] += effect.value;
                }
            }

            baseWithItemCombatRating = Mathf.CeilToInt((float)BaseCombatRating + (float)itemCombatRating);

            // SKILLS
            var skills = player.SkillModule.GetSkillsByTypeAndWeapon(type, YisoSkill.Types.PASSIVE);
            foreach (var skill in skills.Cast<YisoPassiveSkill>()) {
                if (!skill.IsLearned) continue;
                foreach (var passiveEffectInfo in skill.EffectInfos) {
                    var passiveEffect = passiveEffectInfo.Effect;
                    var passiveValue = passiveEffectInfo.GetCurrentValue(skill.Level);
                    weaponAdditionalStats[type][passiveEffect] += passiveValue;
                }
            }
            
            WeaponCombatRatings[type] = baseWithItemCombatRating;
            if (applyHp) SetHp(MaxHp);

            if (GetService<IYisoGameService>().IsDevelopMode()) 
                WeaponCombatRatings[type] = 99999999;
            else
                WeaponCombatRatings[type] *= (1 + GetBuffAndAdditionalValue(YisoBuffEffectTypes.CR_INC, type));

            OnWeaponCombatRatingChangedEvent?.Invoke(type, WeaponCombatRatings[type]);
            var attack = Mathf.CeilToInt((float)WeaponCombatRatings[type] * ATTACK_DEFENCE_FACTOR);
            var defence = Mathf.CeilToInt((float)WeaponCombatRatings[type] * (1 - ATTACK_DEFENCE_FACTOR));
            WeaponAttackDefences[type] = (attack, defence);

            var moveSpeedValue = GetBuffAndAdditionalValue(YisoBuffEffectTypes.MOVE_SPEED_INC, type);
            var attackSpeedValue = GetBuffAndAdditionalValue(YisoBuffEffectTypes.ATTACK_SPEED_INC, type);
            weaponMoveSpeeds[type] *= (1 + moveSpeedValue);
            weaponAttackSpeeds[type] *= (1 + attackSpeedValue);
            
            OnStatsUIEvent?.Invoke(new StatsUIEventArgs(type, attack, defence, WeaponCombatRatings[type]));
        }


        public void CalculateDamage(YisoEnemy enemy, ref double damageRate, out bool critical) {
            var currentWeapon = player.InventoryModule.GetCurrentEquippedWeaponType();
            
            var criPercent = CriticalPercent.ToNormalized() +
                             GetBuffAndAdditionalValue(YisoBuffEffectTypes.CRI_PERCENT_INC, currentWeapon);
            critical = Randomizer.Below(criPercent);

            damageRate *= (1 + GetBuffAndAdditionalValue(YisoBuffEffectTypes.DMG_INC, currentWeapon));

            var criDamageRate = CriticalDamageRate.ToNormalized() +
                                GetBuffAndAdditionalValue(YisoBuffEffectTypes.CRI_DMG_INC, currentWeapon);

            if (critical) damageRate *= (1 + criDamageRate);

            if (enemy.Type == YisoEnemyTypes.BOSS) {
                var bossDmgInc = GetBuffAndAdditionalValue(YisoBuffEffectTypes.BOSS_DMG_INC, currentWeapon);
                damageRate *= (1 + bossDmgInc);
            }
        }

        public void CalculateDamage(YisoEnemy enemy, YisoActiveSkill skill, ref double damageRate, out bool critical) {
            var currentWeapon = player.InventoryModule.GetCurrentEquippedWeaponType();
            var criPercent = CriticalPercent.ToNormalized() +
                             GetBuffAndAdditionalValue(YisoBuffEffectTypes.CRI_PERCENT_INC, currentWeapon) +
                             skill.CriticalRate.ToPercentage();
            critical = Randomizer.Below(CriticalPercent);
            damageRate *= (1 + GetBuffAndAdditionalValue(YisoBuffEffectTypes.DMG_INC, currentWeapon));
            damageRate *= (1 + skill.DamageRate.ToNormalized());
            var criDamageRate = CriticalDamageRate.ToNormalized()
                                + GetBuffAndAdditionalValue(YisoBuffEffectTypes.CRI_DMG_INC, currentWeapon)
                                + skill.CriticalDamageRate.ToNormalized();

            if (critical) damageRate *= (1 + criDamageRate);
            
            if (enemy.Type != YisoEnemyTypes.BOSS) return;
            var bossDmgInc = GetBuffAndAdditionalValue(YisoBuffEffectTypes.BOSS_DMG_INC, currentWeapon);
            damageRate *= (1 + bossDmgInc);
        }

        public void OnStageChanged() {
            SetHp(MaxHp);
        }

        private void SetSpeed(bool move) {
            var dict = move ? weaponMoveSpeeds : weaponAttackSpeeds;
            foreach (var attackType in EnumExtensions.Values<YisoWeapon.AttackType>().Where(t => t != YisoWeapon.AttackType.None)) {
                var value = GetBuffAndAdditionalValue(
                    move ? YisoBuffEffectTypes.MOVE_SPEED_INC : YisoBuffEffectTypes.ATTACK_SPEED_INC, attackType);
                dict[attackType] *= (1 + value);
            }
        }

        private void ResetSpeed(YisoWeapon.AttackType type, bool move, bool all = false) {
            var dict = move ? weaponMoveSpeeds : weaponAttackSpeeds;
            if (all) {
                foreach (var attackType in EnumExtensions.Values<YisoWeapon.AttackType>().Where(t => t != YisoWeapon.AttackType.None)) {
                    dict[attackType] = 1f;
                }
                
                return;
            }

            dict[type] = 1f;
        }

        private void ResetAdditionalStats(YisoWeapon.AttackType type, bool all = false) {
            if (all) {
                foreach (var attackType in EnumExtensions.Values<YisoWeapon.AttackType>()
                             .Where(t => t != YisoWeapon.AttackType.None)) {
                    if (!weaponAdditionalStats.ContainsKey(attackType))
                        weaponAdditionalStats[attackType] = new Dictionary<YisoBuffEffectTypes, int>();
                    foreach (var effect in EnumExtensions.Values<YisoBuffEffectTypes>()) {
                        weaponAdditionalStats[attackType][effect] = 0;
                    }
                }
                
                return;
            }
            
            
            foreach (var effect in EnumExtensions.Values<YisoBuffEffectTypes>()) {
                weaponAdditionalStats[type][effect] = 0;
            }
        }

        private void ResetBuffStats() {
            foreach (var type in EnumExtensions.Values<YisoBuffEffectTypes>()) {
                buffStats[type] = 0;
            }
        }

        private void ResetCombatRatings() {
            foreach (var weapon in EnumExtensions.Values<YisoWeapon.AttackType>()
                         .Where(t => t != YisoWeapon.AttackType.None)) {
                WeaponCombatRatings[weapon] = 0d;
            }
        }

        private float GetBuffAndAdditionalValue(YisoBuffEffectTypes type, YisoWeapon.AttackType attackType) {
            var buff = buffStats[type].ToNormalized();
            var additional = weaponAdditionalStats[attackType][type].ToNormalized();
            return buff + additional;
        }

        public void SaveData(ref YisoPlayerData data) {
            data.statData.maxHp = MaxHp;
            data.statData.hp = hp;
        }

        public void LoadData(YisoPlayerData data) {
            MaxHp = data.statData.maxHp;
            hp = data.statData.hp;
        }
    }
}