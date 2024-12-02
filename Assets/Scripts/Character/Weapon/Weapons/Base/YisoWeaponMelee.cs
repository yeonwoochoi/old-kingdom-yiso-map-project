using System.Collections;
using Character.Ability;
using Character.Health.Damage;
using Core.Domain.Actor.Attack;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Character.Weapon.Weapons.Base {
    public abstract class YisoWeaponMelee : YisoWeapon {
        [Title("Damage Area")] public YisoDamageOnTouch.DamageAreaSettings damageAreaSettings;

        [Title("Melee Delay")] public float initialDelay = 0f;
        public float activeDuration = 1f;

        public override void Initialization() {
            base.Initialization();
            if (damageAreaSettings.DamageOnTouch == null) {
                InitializeDamageArea();
                DisableDamageArea();
            }

            if (owner != null) {
                damageAreaSettings.DamageOnTouch.owner = owner.gameObject;
            }
        }

        protected virtual void InitializeDamageArea() {
            damageAreaSettings.Initialization(owner.gameObject, gameObject, CalculateDamage);
        }

        protected override IEnumerator WeaponUseCo() {
            if (weaponUseCoInProgress) yield break;
            weaponUseCoInProgress = true;

            yield return new WaitForSeconds(initialDelay);
            EnableDamageArea();
            yield return new WaitForSeconds(activeDuration);
            DisableDamageArea();
            
            if (owner.LinkedInputManager != null) owner.LinkedInputManager.ResetSecondaryMovement();
            weaponUseCoInProgress = false;
        }

        protected virtual void EnableDamageArea() {
            if (damageAreaSettings.DamageOnTouch != null) damageAreaSettings.EnableDamageArea();
        }

        protected virtual void DisableDamageArea() {
            if (damageAreaSettings.DamageOnTouch != null) damageAreaSettings.DisableDamageArea();
        }

        protected override void OnDisable() {
            base.OnDisable();
            weaponUseCoInProgress = false;
        }

        protected virtual YisoAttack CalculateDamage(GameObject target) {
            if (characterStat != null && target.TryGetComponent<YisoCharacterStat>(out var targetStat)) {
                return characterStat.CombatStat.CreateAttack(targetStat.CombatStat);
            }

            // Debug.LogWarning($"Target \"{target.name}\" or Attacker \"{owner.gameObject.name}\" has no character stat ability");
            return new YisoAttack(Random.Range(damageAreaSettings.minDamage,
                Mathf.Max(damageAreaSettings.maxDamage, damageAreaSettings.minDamage)));
        }
    }
}