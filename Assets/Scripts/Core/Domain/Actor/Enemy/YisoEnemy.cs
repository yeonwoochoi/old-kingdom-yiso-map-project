using System;
using System.Collections.Generic;
using Core.Domain.Actor.Attack;
using Core.Domain.Actor.Enemy.Drop;
using Core.Domain.Actor.Player;
using Core.Domain.Entity;
using Core.Domain.Locale;
using Core.Domain.Settings;
using Core.Domain.Types;
using Core.Logger;
using Core.Service;
using Core.Service.Factor.Combat;
using Core.Service.Factor.HonorRating;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Utils.Extensions;
using Utils.ObjectId;

namespace Core.Domain.Actor.Enemy {
    public class YisoEnemy : IYisoCombatableEntity {
        public int Id { get; private set; }
        public int MaxHp { get; set; }
        public int Hp { get; set; }
        public double CombatRating { get; set; }
        public double Exp { get; set; }
        public double AttackPower { get; set; }
        public double Defence { get; set; }
        public List<YisoEnemyBaseDrop> DropItems { get; }
        public YisoEnemyTypes Type { get; }
        
        public string ObjectId { get; }

        public event UnityAction<string> OnDeathEvent; 

        private readonly YisoLocale name;
        private readonly YisoLocale description;

        public YisoEnemy(YisoEnemySO so) {
            Id = so.id;
            name = so.name;
            description = so.description;
            Type = so.type;
            DropItems = so.CreateDrops();
            ObjectId = YisoObjectID.GenerateString();
        }

        public YisoEnemy(YisoEnemySO so, double stageCR, IYisoHonorRatingFactorService honorRatingFactorService) : this(so) {
            CombatRating = stageCR * honorRatingFactorService.GetEnemyHonorRatingFactor(Type);
            MaxHp = Mathf.CeilToInt((float) (stageCR * honorRatingFactorService.GetEnemyMaxHpFactor(Type)));
            Hp = MaxHp;
        }

        public string GetName(YisoLocale.Locale locale) => name[locale];

        public string GetDescription(YisoLocale.Locale locale) => description[locale];
        
        public int GetMaxHp() => MaxHp;

        public int GetHp() => Hp;
        public double GetCombatRating() => CombatRating;

        public YisoAttack CreateAttack(IYisoEntity entity) {
            if (entity is not IYisoCombatableEntity combatableEntity) return null;
            var combatFactorService = YisoServiceProvider.Instance.Get<IYIsoCombatFactorService>();
            
            var attack = new YisoAttack();
            var damageRate = 0d;

            if (combatableEntity is YisoPlayer player)
                damageRate = combatFactorService.GetEnemyToPlayerDamageRate(Type);
            else damageRate = combatFactorService.GetEnemyToAllyDamageRate(Type);
            
            var takeMore = ((IYisoCombatableEntity)this).CalculateTakeMore(combatableEntity.GetCombatRating());
            damageRate *= takeMore;
            if (damageRate <= 0) damageRate = 1 / (float) combatableEntity.GetMaxHp();
            attack.Damages.Add(new YisoAttack.DamageInfo {
                Damage = combatableEntity.GetMaxHp() * damageRate,
                DamageRate = damageRate,
                IsCritical = false
            });
            // YisoLogger.Log($"[ENEMY=>PLAYER] Damage: {attack.Damage}, Damage Rate: {(damageRate * 100).ToPercentage()}");
            return attack;
        }

        public YisoAttack CreateSkillAttack(IYisoEntity entity, int skillId) {
            return CreateAttack(entity);
        }

        public YisoAttackResult GetAttack(YisoAttack attack) {
            var death = false;
            foreach (var info in attack.Damages) {
                var damage = info.Damage;
                var afterHp = Hp - damage;
                afterHp = Mathf.Max(0, (float)afterHp);
                Hp = Mathf.RoundToInt((float)afterHp);
                death = Hp <= 0;
                if (death) break;
            }
            
            if (death) {
                OnDeathEvent?.Invoke(ObjectId);
            } 
            return new YisoAttackResult(death);
        }

        public int GetId() => Id;
    }
}