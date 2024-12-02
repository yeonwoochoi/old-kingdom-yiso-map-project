using System.Collections;
using Core.Domain.Actor.Attack;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Utils.Beagle;
using Random = UnityEngine.Random;

namespace Character.Ability {
    [AddComponentMenu("Yiso/Character/Abilities/Character Death")]
    public class YisoCharacterDeath : YisoCharacterAbility {
        public enum DeathAnimationType {
            DefaultDeath = 0,
            KnockBackDeath = 1,
        }

        [ReadOnly] public DeathAnimationType currentDeathActionType;
        public float knockBackImpactMin = 1000f;
        public float knockBackImpactMax = 1500f;

        protected const string DeathAnimationParameterName = "IsDeath";
        protected const string DeathTypeAnimationParameterName = "DeathType";
        protected int deathAnimationParameter;
        protected int deathTypeAnimationParameter;

        public virtual void PlayDeathAction(GameObject attacker, YisoAttack attackInfo, UnityAction callback) {
            StartCoroutine(PlayDeathActionCo(attacker, attackInfo, callback));
        }

        protected virtual IEnumerator PlayDeathActionCo(GameObject attacker, YisoAttack attackInfo, UnityAction callback) {
            var knockBackVector = character.transform.position - attacker.transform.position;
            var knockBackForce = Random.Range(knockBackImpactMin, knockBackImpactMax);
            var rotateAngle = knockBackVector.x >= 0f ? -90f : 90f;

            SetCurrentDeathActionType(attackInfo.ExistCritical);

            switch (currentDeathActionType) {
                case DeathAnimationType.DefaultDeath:
                    PlayDeathAnimation();
                    break;
                case DeathAnimationType.KnockBackDeath:
                    controller.Impact(knockBackVector, knockBackForce);
                    PlayDeathAnimation();
                    yield return StartCoroutine(RotateOverTime(character.characterModel.transform, rotateAngle, 0.2f));
                    yield return new WaitForSeconds(0.5f);
                    break;
            }

            yield return null;

            callback?.Invoke();
        }

        protected virtual void SetCurrentDeathActionType(bool isCritical) {
            currentDeathActionType = isCritical ? DeathAnimationType.KnockBackDeath : DeathAnimationType.DefaultDeath;
        }

        #region Physics
        
        private IEnumerator RotateOverTime(Transform target, float targetRotation, float duration) {
            var startRotation = target.rotation.eulerAngles.z;
            var elapsedTime = 0f;

            while (elapsedTime < duration) {
                // Interpolate between the start rotation and target rotation
                var currentRotation = Mathf.LerpAngle(startRotation, targetRotation, elapsedTime / duration);
                target.rotation = Quaternion.Euler(0, 0, currentRotation);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the final rotation is set to the target rotation
            target.rotation = Quaternion.Euler(0, 0, targetRotation);
        }

        private IEnumerator RiseAndFallOverTime(Transform target, float height, float duration) {
            var elapsedTime = 0f;
            var startPosition = target.localPosition;

            while (elapsedTime < duration / 2) {
                elapsedTime += Time.deltaTime;
                var t = elapsedTime / (duration / 2f);
                var yPos = Mathf.Lerp(0, height, t) - (0.5f * Mathf.Pow(t - 1, 2) * height);
                target.localPosition = new Vector3(startPosition.x, yPos, startPosition.z);
                yield return null;
            }

            elapsedTime = 0f;
            while (elapsedTime < duration / 2) {
                elapsedTime += Time.deltaTime;
                var t = elapsedTime / (duration / 2f);
                var yPos = Mathf.Lerp(height, 0, t) - (0.5f * Mathf.Pow(t - 1, 2) * height);
                target.localPosition = new Vector3(startPosition.x, yPos, startPosition.z);
                yield return null;
            }

            target.localPosition = startPosition;
        }

        #endregion

        #region Animator

        protected override void InitializeAnimatorParameters() {
            base.InitializeAnimatorParameters();
            RegisterAnimatorParameter(DeathAnimationParameterName, AnimatorControllerParameterType.Bool,
                out deathAnimationParameter);
            RegisterAnimatorParameter(DeathTypeAnimationParameterName, AnimatorControllerParameterType.Int,
                out deathTypeAnimationParameter);
        }

        protected virtual void PlayDeathAnimation() {
            YisoAnimatorUtils.UpdateAnimatorInteger(animator, deathTypeAnimationParameter, (int) currentDeathActionType,
                character.AnimatorParameters);
            YisoAnimatorUtils.UpdateAnimatorBool(animator, deathAnimationParameter, true, character.AnimatorParameters);
        }

        protected virtual void ChangeDeathAnimationType(DeathAnimationType deathAnimationType) {
            YisoAnimatorUtils.UpdateAnimatorInteger(animator, deathTypeAnimationParameter, (int) deathAnimationType,
                character.AnimatorParameters);
        }

        #endregion
    }
}