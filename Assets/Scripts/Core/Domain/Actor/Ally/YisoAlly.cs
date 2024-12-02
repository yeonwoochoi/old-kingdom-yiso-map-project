using Core.Domain.Actor.Attack;
using Core.Domain.Actor.Enemy;
using Core.Domain.Entity;
using Core.Domain.Locale;
using Core.Domain.Settings;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Factor.Combat;
using Core.Service.Factor.HonorRating;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Utils.ObjectId;

namespace Core.Domain.Actor.Ally {
    public class YisoAlly : IYisoCombatableEntity {
        public event UnityAction<string> OnDeathEvent; 
        public int Id { get; }
        public int MaxHp { get; set; }
        public int Hp { get; set; }
        public double CombatRating { get; set; }
        public double AttackPower { get; set; }
        public double Defence { get; set; }

        public int GetId() => Id;

        public int GetMaxHp() => MaxHp;

        public int GetHp() => Hp;

        public double GetCombatRating() => CombatRating;
        
        private readonly YisoLocale name;
        private readonly YisoLocale description;
        private double allyWeight;
        
        public string ObjectId { get; }
        
        public YisoAlly(YisoAllySO so) {
            Id = so.id;
            ObjectId = YisoObjectID.GenerateString();
            name = so.name;
            description = so.description;
        }

        public YisoAlly(YisoAllySO so, double stageCR, IYisoHonorRatingFactorService honorRatingFactorService) : this(so) {
            CalculateStageCR(stageCR, honorRatingFactorService);
        }

        public void CalculateStageCR(double stageCR, IYisoHonorRatingFactorService honorRatingFactorService) {
            var allyFactors = honorRatingFactorService.GetAllyFactors();
            CombatRating = stageCR * allyFactors.honorRating;
            MaxHp = Mathf.CeilToInt((float)(stageCR * allyFactors.maxHp));
            Hp = MaxHp;
        }
        
        public string GetName(YisoLocale.Locale locale) => name[locale];

        public string GetDescription(YisoLocale.Locale locale) => description[locale];

        public YisoAttack CreateAttack(IYisoEntity entity) {
            if (entity is not IYisoCombatableEntity combatableEntity) return null;
            
            var combatFactorService = YisoServiceProvider.Instance.Get<IYIsoCombatFactorService>();
            var enemy = (YisoEnemy) entity;
            var attack = new YisoAttack();
            // var damageRate = enemy.Type.GetHitFromAllyDamageRate();
            var damageRate = combatFactorService.GetAllyToEnemyDamageRate(enemy.Type);
            // var takeMore = ((IYisoCombatableEntity)this).CalculateTakeMore(combatableEntity.GetCombatRating());
            var takeMore = combatFactorService.CalculateTakeMore(GetCombatRating(), combatableEntity.GetCombatRating());
            damageRate *= takeMore;
            if (damageRate <= 0) damageRate = 1 / (float) combatableEntity.GetMaxHp();
            attack.Damages.Add(new YisoAttack.DamageInfo {
                Damage = combatableEntity.GetMaxHp() * damageRate,
                DamageRate = damageRate,
                IsCritical = false
            });
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
    }
}