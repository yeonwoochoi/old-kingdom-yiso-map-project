using System.Collections.Generic;
using Core.Behaviour;
using UnityEngine;

namespace Character.Health.Damage.Resistance {
    [AddComponentMenu("Yiso/Character/Health/Damage Resistance Processor")]
    public class YisoDamageResistanceProcessor : RunIBehaviour {
        public List<YisoDamageResistance> damageResistanceList;

        // TODO
        /// <summary>
        /// Damage 계산
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="typedDamages"></param>
        /// <param name="damagedApplied">Damage 받았을 때 YisoFeedbacks 실행할건지 말지 (cf. 독 데미지 같이 지속적으로 데미지 받는 경우는 매번 Feedback 실행 안함)</param>
        /// <returns></returns>
        public virtual float ProcessDamage(float damage, List<YisoTypedDamage> typedDamages, bool damagedApplied) {
            // TODO
            var totalDamage = 0f;
            return totalDamage;
        }

        /// <summary>
        /// 속성 데미지인 경우 저항성 제크해서 Character Condition 변경 여부 확인
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckPreventCharacterConditionChange(YisoDamageType typedDamage) {
            // TODO
            return false;
        }

        public virtual bool CheckPreventMovementModifier(YisoDamageType typedDamage) {
            // TODO
            return false;
        }

        public virtual bool CheckPreventKnockBack(List<YisoTypedDamage> typedDamages) {
            // TODO
            return false;
        }

        public virtual Vector3 ProcessKnockBackForce(Vector3 knockBack, List<YisoTypedDamage> typedDamages) {
            // TODO
            return knockBack;
        }

        public virtual void InterruptDamageOverTime(YisoDamageType damageType) {
            // TODO
            foreach (var resistance in damageResistanceList) {
            }
        }
    }
}