using System.Collections.Generic;
using Core.Domain.Actor.Attack;
using Core.Domain.Actor.Enemy;
using Core.Domain.Actor.Erry;
using Core.Domain.Actor.Player.Modules.Inventory.V2;
using Core.Domain.Actor.Player.Modules.Pet;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Actor.Player.Modules.Skill;
using Core.Domain.Actor.Player.Modules.Stat;
using Core.Domain.Actor.Player.Modules.Storage;
using Core.Domain.Actor.Player.Modules.UI;
using Core.Domain.Actor.Player.SO;
using Core.Domain.Data;
using Core.Domain.Entity;
using Core.Domain.Quest;
using Core.Domain.Skill;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Data;
using Core.Service.Factor.Combat;

namespace Core.Domain.Actor.Player {
    public class YisoPlayer : IYisoCombatableEntity {
        #region MODULES

        public YisoPlayerInventoryV2Module InventoryModule { get; }
        public YisoPlayerStorageModule StorageModule { get; }
        public YisoPlayerStatModule StatModule { get; }
        public YisoPlayerUIModule UIModule { get; }
        
        public YisoPlayerQuestModule QuestModule { get; }
        
        public YisoPlayerPetModule PetModule { get; }
        
        public YisoPlayerSkillModule SkillModule { get; }

        #endregion

        public YisoErry Erry { get; set; }
        
        internal YisoPlayer(IReadOnlyList<YisoQuest> quests) {
            InventoryModule = new YisoPlayerInventoryV2Module(this);
            StorageModule = new YisoPlayerStorageModule(this);
            StatModule = new YisoPlayerStatModule(this);
            UIModule = new YisoPlayerUIModule(this);
            QuestModule = new YisoPlayerQuestModule(this, quests);
            PetModule = new YisoPlayerPetModule(this);
            SkillModule = new YisoPlayerSkillModule(this);
        }

        public void ResetData(IReadOnlyList<YisoQuest> quests) {
            InventoryModule.ResetData();
            QuestModule.ResetData(quests);
            PetModule.ResetData();
            SkillModule.ResetData();
        }

        public int GetHp() => StatModule.Hp;
        public double GetCombatRating() {
            var equippedWeaponType = InventoryModule.GetCurrentEquippedWeaponType();
            return StatModule.WeaponCombatRatings[equippedWeaponType];
        }

        public YisoAttack CreateAttack(IYisoEntity entity) {
            if (entity is not YisoEnemy enemy) return null;
            return CreateAttack(enemy);
        }

        public YisoAttack CreateSkillAttack(IYisoEntity entity, int skillId) {
            if (entity is not YisoEnemy enemy) return null;
            var skill = SkillModule.GetSkillOrElseThrow<YisoActiveSkill>(skillId);
            return CreateAttack(enemy, skill);
        }

        private YisoAttack CreateAttack(YisoEnemy enemy, YisoActiveSkill skill = null) {
            var combatFactorService = YisoServiceProvider.Instance.Get<IYIsoCombatFactorService>();
            var attack = new YisoAttack();
            var attackCount = skill?.AttackCount ?? 1;
            // var damageRate = enemy.Type.GetHitFromPlayerDamageRate();
            var damageRate = combatFactorService.GetPlayerToEnemyDamageRate(enemy.Type);
            // var takeMore = ((IYisoCombatableEntity)this).CalculateTakeMore(enemy.GetCombatRating());
            var takeMore = combatFactorService.CalculateTakeMore(GetCombatRating(), enemy.GetCombatRating());
            damageRate *= takeMore;
            if (damageRate <= 0) damageRate = 1 / (float) enemy.GetMaxHp();
            
            for (var i = 0; i < attackCount; i++) {
                var critical = false;
                if (skill != null) {
                    StatModule.CalculateDamage(enemy, skill, ref damageRate, out critical);
                } else {
                    StatModule.CalculateDamage(enemy, ref damageRate, out critical);
                }
                attack.Damages.Add(new YisoAttack.DamageInfo {
                    Damage = enemy.GetMaxHp() * damageRate,
                    DamageRate = damageRate,
                    IsCritical = critical
                });
            }
            
            return attack;
        }

        public YisoAttackResult GetAttack(YisoAttack attack) {
            StatModule.OnHit(attack, out var death);
            return new YisoAttackResult(death);
        }

        public YisoPlayerData ToSaveData() {
            var data = new YisoPlayerData();
            InventoryModule.SaveData(ref data);
            StorageModule.SaveData(ref data);
            StatModule.SaveData(ref data);
            SkillModule.SaveData(ref data);
            UIModule.SaveData(ref data);
            return data;
        }
        
        public int GetMaxHp() => StatModule.MaxHp;

        public void Revive() {
            StatModule.Revive();
        }

        public void ToSaveData(YisoPlayerData data) {
            InventoryModule.SaveData(ref data);
            StorageModule.SaveData(ref data);
            StatModule.SaveData(ref data);
            SkillModule.SaveData(ref data);
            UIModule.SaveData(ref data);
        }

        public void LoadData(YisoPlayerData data, YisoPlayerInventoryItemsSO inventoryItemsSO = null) {
            InventoryModule.LoadData(data, inventoryItemsSO);
            StorageModule.LoadData(data);
            StatModule.LoadData(data);
            SkillModule.LoadData(data);
            UIModule.LoadData(data);
            StatModule.CalculateCombatRatingAllWeapons();
        }

        public void OnStageChanged(bool next) {
            if (next) {
                SkillModule.AddSkillPoint(3);
            }
            StatModule.OnStageChanged();
        }

        public void SaveData(bool showUI = false) {
            YisoServiceProvider.Instance.Get<IYisoDataService>()
                .SavePlayerData(this, showUI);
        }

        public int GetId() => -1;
    }
}