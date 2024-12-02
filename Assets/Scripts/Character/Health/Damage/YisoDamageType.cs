using UnityEngine;

namespace Character.Health.Damage {
    public enum DamageTypeModes {
        BaseDamage,
        TypedDamage
    }

    [CreateAssetMenu(menuName = "Yiso/Health/DamageType", fileName = "DamageType")]
    public class YisoDamageType : ScriptableObject {
    }
}