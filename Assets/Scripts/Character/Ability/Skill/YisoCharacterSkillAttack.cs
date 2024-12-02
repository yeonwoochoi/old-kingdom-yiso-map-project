using System.Collections;
using Character.Core;
using Character.Health.Damage;
using Character.Weapon.Aim;
using Core.Domain.Actor.Attack;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Character.Ability.Skill {
    public abstract class YisoCharacterSkillAttack : YisoCharacterSkill {
        [Title("Damage Area")]
        public YisoDamageOnTouch.DamageAreaSettings damageAreaSettings;

        [Title("Skill Aim")] public YisoBaseAim.AimControls aimControl = YisoBaseAim.AimControls.Script;

        public YisoSkillAim SkillAim { get; protected set; }
        protected GameObject damageAreaParent;

        protected abstract Vector2 InitialAim { get; }
        protected abstract bool isDamageAreaAttachedToCharacter { get; }

        protected override void Initialization() {
            base.Initialization();
            if (isDamageAreaAttachedToCharacter) {
                if (damageAreaParent == null) {
                    var skillParentObjName = $"[{character.playerID}] Skills";
                    damageAreaParent = new GameObject(skillParentObjName);
                    damageAreaParent.transform.SetParent(character.transform);
                    damageAreaParent.transform.localPosition = Vector3.zero;
                    damageAreaParent.transform.localRotation = Quaternion.identity;
                    damageAreaParent.transform.localScale = Vector3.one;
                }

                if (damageAreaSettings.DamageOnTouch == null) {
                    damageAreaSettings.Initialization(character.gameObject, damageAreaParent, CalculateDamage);
                    damageAreaSettings.DisableDamageArea();
                    damageAreaSettings.DamageOnTouch.owner = character.gameObject;
                }

                if (SkillAim == null) {
                    SkillAim = damageAreaSettings.DamageArea.AddComponent<YisoSkillAim>();
                    SkillAim.Initialization(character, aimControl);
                    SkillAim.ApplyAim(InitialAim, true);
                }
            }
        }

        public override void StopSkillCast(bool isAutoSkillSequenceTermination = false) {
            base.StopSkillCast(isAutoSkillSequenceTermination);
            damageAreaSettings.DisableDamageArea();
            if (inputManager != null) inputManager.ResetMovement();
        }

        protected override IEnumerator PerformSkillSequence(Transform target) {
            yield return null;
        }

        protected virtual Vector2 GetCurrentDirection(Transform target) {
            Vector2 currentDirection;

            if (character.characterType == YisoCharacter.CharacterTypes.Player) {
                currentDirection = controller.currentDirection.normalized;
            }
            else if (target == null) {
                currentDirection = controller.currentDirection.normalized;
            }
            else {
                currentDirection = (target.transform.position - character.characterModel.transform.position).normalized;
            }

            return currentDirection;
        }


        #region Damage Calculate

        protected virtual YisoAttack CalculateDamage(GameObject target) {
            if (character.CharacterStat != null && target.TryGetComponent<YisoCharacterStat>(out var targetStat)) {
                return character.CharacterStat.CombatStat.CreateSkillAttack(targetStat.CombatStat, SkillId);
            }

            // Debug.LogWarning($"Target \"{target.name}\" or Attacker \"{owner.gameObject.name}\" has no character stat ability");
            return new YisoAttack(Random.Range(damageAreaSettings.minDamage,
                Mathf.Max(damageAreaSettings.maxDamage, damageAreaSettings.minDamage)));
        }

        #endregion
    }
}