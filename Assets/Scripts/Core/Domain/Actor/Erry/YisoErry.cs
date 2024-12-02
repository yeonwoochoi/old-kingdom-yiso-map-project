using System;
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
using Utils;

namespace Core.Domain.Actor.Erry {
    public class YisoErry : IYisoCombatableEntity {
        public int MaxHp { get; set; }
        public int Hp { get; set; }
        public double CombatRating { get; set; }
        public double AttackPower { get; set; }
        public double Defence { get; set; }

        public int GetId() => -2;

        public int GetMaxHp() => MaxHp;

        public int GetHp() => Hp;

        public double GetCombatRating() => CombatRating;

        public YisoErry(YisoErrySO so) { }

        public YisoErry(YisoErrySO so, double stageCR, IYisoHonorRatingFactorService honorRatingFactorService) :this(so) { 
            SetCombatRating(stageCR, honorRatingFactorService);
        }

        public void OnChangeStage(double stageCR, IYisoHonorRatingFactorService honorRatingFactorService) {
            SetCombatRating(stageCR, honorRatingFactorService);
        }

        private void SetCombatRating(double stageCR, IYisoHonorRatingFactorService honorRatingFactorService) {
            var erryFactor = honorRatingFactorService.GetErryFactors();
            CombatRating = stageCR * erryFactor.honorRating;
            MaxHp = Mathf.CeilToInt((float)(stageCR * erryFactor.maxHp));
            Hp = MaxHp;
        }

        public string GetName(YisoLocale.Locale locale) => locale == YisoLocale.Locale.KR ? "어리" : "Erry";

        public YisoAttack CreateAttack(IYisoEntity entity) {
            if (entity is not IYisoCombatableEntity combatableEntity) return null;
            var combatFactorService = YisoServiceProvider.Instance.Get<IYIsoCombatFactorService>();
            
            var enemy = (YisoEnemy)entity;
            var attack = new YisoAttack();
            // var damageRate = enemy.Type.GetHitFromAllyDamageRate();
            var damageRate = combatFactorService.GetAllyToEnemyDamageRate(enemy.Type);
            // var takeMore = ((IYisoCombatableEntity)this).CalculateTakeMore(combatableEntity.GetCombatRating());
            var takeMore = combatFactorService.CalculateTakeMore(GetCombatRating(), combatableEntity.GetCombatRating());
            damageRate *= takeMore;
            attack.Damages.Add(new YisoAttack.DamageInfo {
                Damage = combatableEntity.GetMaxHp() * damageRate,
                DamageRate = damageRate,
                IsCritical = false
            });
            return attack;
        }

        public YisoAttack CreateSkillAttack(IYisoEntity entity, int skillId) {
            return null;
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
            
            return new YisoAttackResult(death);
        }

        public Vector3 GetScale(int stage) => Vector3.one * (float)(0.9f + 0.01f * Math.Truncate(stage / 10f));
    }
}