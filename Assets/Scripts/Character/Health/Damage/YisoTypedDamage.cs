using System;
using Character.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Character.Health.Damage {
    /// <summary>
    /// 속성 데미지
    /// </summary>
    [Serializable]
    public class YisoTypedDamage {
        public YisoDamageType associatedDamageType;

        // Damage 받았을 때, Character Condition 강제로 변경할거냐 (ex. 전기 공격 -> Stunned 상태로 강제 변경)
        public bool forceCharacterCondition = false;
        [ShowIf("forceCharacterCondition")] public YisoCharacterStates.CharacterConditions forcedCondition;
        [ShowIf("forceCharacterCondition")] public float forcedConditionDuration = 3f;

        [ShowIf("forceCharacterCondition")]
        public bool
            resetControllerForces =
                false; // 강제로 Condition 변경 후 원래상태로 돌아올 때 Controller 리셋 시킬거냐 (Movement = Vector3.zero)

        // Damage 받았을 때, Movement Multiplier 적용할거냐 (ex. 독 공격 -> 속도 2초간 0.5배로 느려짐)
        public bool applyMovementMultiplier = false;
        [ShowIf("applyMovementMultiplier")] public float movementMultiplier = 0.5f;
        [ShowIf("applyMovementMultiplier")] public float movementMultiplierDuration = 2f;

        // Typed Damage인 경우 추가 데미지
        public float minDamageCaused = 10f;
        public float maxDamageCaused = 10f;

        protected int lastRandomFrame = -1000;
        protected float lastRandomValue = 0f;

        /// <summary>
        /// 속성 데미지라고 생각하면 됨.
        /// 기본 데미지에 이 Damage가 더해짐
        /// </summary>
        public virtual float DamageCaused {
            get {
                if (Time.frameCount != lastRandomFrame) {
                    lastRandomValue = Random.Range(minDamageCaused, maxDamageCaused);
                    lastRandomFrame = Time.frameCount;
                }

                return lastRandomValue;
            }
        }
    }
}