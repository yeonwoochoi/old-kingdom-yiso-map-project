using System;

namespace Core.Domain.Types {
    public enum YisoEnemyTypes {
        NORMAL,
        ELITE,
        FIELD_BOSS,
        BOSS
    }

    public static class YisoEnemyTypesUtils {
        
        public static double GetHitFromPlayerDamageRate(this YisoEnemyTypes type, bool normalize = true) {
            var rate = type switch {
                YisoEnemyTypes.NORMAL => 25,
                YisoEnemyTypes.ELITE => 17,
                YisoEnemyTypes.FIELD_BOSS => 10,
                YisoEnemyTypes.BOSS => 5,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            return rate * (normalize ? 0.01 : 1);
        }

        public static double GetHitFromAllyDamageRate(this YisoEnemyTypes type, bool normalize = true) {
            var rate = type switch {
                YisoEnemyTypes.NORMAL => 15,
                YisoEnemyTypes.ELITE => 12,
                YisoEnemyTypes.FIELD_BOSS => 8,
                YisoEnemyTypes.BOSS => 5,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            return rate * (normalize ? 0.01 : 1);
        }

        public static double GetPlayerAttackDamageRate(this YisoEnemyTypes type, bool normalize = true) {
            var rate = type switch {
                YisoEnemyTypes.NORMAL => 13,
                YisoEnemyTypes.ELITE => 17,
                YisoEnemyTypes.FIELD_BOSS => 20,
                YisoEnemyTypes.BOSS => 25,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            return rate * (normalize ? 0.01 : 1);
        }
        
        public static double GetAllyAttackDamageRate(this YisoEnemyTypes type, bool normalize = true) {
            var rate = type switch {
                YisoEnemyTypes.NORMAL => 15,
                YisoEnemyTypes.ELITE => 20,
                YisoEnemyTypes.FIELD_BOSS => 25,
                YisoEnemyTypes.BOSS => 35,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            return rate * (normalize ? 0.01 : 1);
        }

        public static double ToMoneyDropFactor(this YisoEnemyTypes type) => type switch {
            YisoEnemyTypes.NORMAL => 1,
            YisoEnemyTypes.ELITE => 1.2,
            YisoEnemyTypes.FIELD_BOSS => 2,
            YisoEnemyTypes.BOSS => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public static double ToDropFactor(this YisoEnemyTypes type) => type switch {
            YisoEnemyTypes.NORMAL => 0.5,
            YisoEnemyTypes.ELITE => 0.3,
            YisoEnemyTypes.FIELD_BOSS => 0.05,
            YisoEnemyTypes.BOSS => 0.01,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}