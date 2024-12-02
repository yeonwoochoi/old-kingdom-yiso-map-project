using System;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Settings {
    [CreateAssetMenu(fileName = "CombatRatingSettings", menuName = "Yiso/Settings/Combat Rating Weight")]
    public class YisoCombatRatingWeightSO : ScriptableObject {
        [Title("Player Settings")]
        public int initAttack = 20;
        public int initDefence = 5;
        public int initMaxHp = 100;
        public int attackIncrease = 5;
        public int defenceIncrease = 1;
        public int maxHpIncrease = 10;

        [Title("Enemy Settings")] 
        public double combatRatingWeight = 0.5d;
        public double maxHpWeight = 1.5d;
        public double expWeight = 0.5d;
        public double defenceWeight = 0.05d;
        public double attackPowerWeight = 0.5d;
        public double normalWeight = 1f;
        public double eliteWeight = 1.5f;
        public double fieldBossWeight = 2f;
        public double bossWeight = 3f;

        [Title("Ally")] public double erryWeight = 1.1d;

        public YisoCombatRatingWeights CreateSettings(double externalWeight) => new(this, externalWeight);
    }

    public sealed class YisoCombatRatingWeights {
        public int InitAttack { get; }
        public int InitDefence { get; }
        public int InitMaxHp { get; }
        public int AttackIncrease { get; }
        public int DefenceIncrease { get; }
        public int MaxHpIncrease { get; }
        public double CombatRatingWeight { get; }
        public double MaxHpWeight { get; }
        public double DefenceWeight { get; }
        public double ExpWeight { get; }
        public double AttackPowerWeight { get; }
        public double ErryWeight { get; }

        private readonly double normalWeight;
        private readonly double eliteWeight;
        private readonly double fieldBossWeight;
        private readonly double bossWeight;

        public double DifficultyWeight { get; internal set; }

        public YisoCombatRatingWeights(YisoCombatRatingWeightSO so, double difficultyWeight) {
            InitAttack = so.initAttack;
            InitDefence = so.initDefence;
            InitMaxHp = so.initMaxHp;
            AttackIncrease = so.attackIncrease;
            DefenceIncrease = so.defenceIncrease;
            MaxHpIncrease = so.maxHpIncrease;

            CombatRatingWeight = so.combatRatingWeight;
            MaxHpWeight = so.maxHpWeight;
            DefenceWeight = so.defenceWeight;
            ExpWeight = so.expWeight;
            AttackPowerWeight = so.attackPowerWeight;

            ErryWeight = so.erryWeight;

            normalWeight = so.normalWeight;
            eliteWeight = so.eliteWeight;
            fieldBossWeight = so.fieldBossWeight;
            bossWeight = so.bossWeight;

            DifficultyWeight = difficultyWeight;
        }

        public double GetWeightByType(YisoEnemyTypes type) => type switch {
            YisoEnemyTypes.NORMAL => normalWeight,
            YisoEnemyTypes.ELITE => eliteWeight,
            YisoEnemyTypes.FIELD_BOSS => fieldBossWeight,
            YisoEnemyTypes.BOSS => bossWeight,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public int GetAttack(int level) => InitAttack + (level - 1) * AttackIncrease;
        public int GetDefence(int level) => InitDefence + (level - 1) * DefenceIncrease;
        public int GetMaxHp(int level) => InitMaxHp + (level - 1) * MaxHpIncrease;
    }
}