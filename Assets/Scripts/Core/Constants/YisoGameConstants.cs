using System.Collections.Generic;
using Core.Domain.Item;
using Core.Domain.Types;
using UnityEngine;
using Utils;

namespace Core.Constants {
    public static class YisoGameConstants {
        private static readonly double DEFAULT_EXP_VALUE = 100;
        private static readonly double LEVEL_COEFFICIENT = 1.1;

        public static readonly int DEFAULT_ATTACK = 30;
        public static readonly int DEFAULT_DEFENCE = 5;
        
        public const float REINFORCE_NORMAL_RATE_RATIO = 0.2f;
        public const float REINFORCE_NORMAL_COST_RATIO = 0.3f;
        
        public static readonly Dictionary<int, int> REINFORCE_NORMAL_LEVEL_COST_MAP = new() {
            { 10, 1000 },
            { 20, 2000 },
            { 30, 4000 },
            { 40, 8000 },
            { 50, 16000 },
            { 60, 32000 },
            { 70, 64000 },
            { 80, 128000 },
            { 90, 256000 },
            { 100, 512000 }
        };
        
        public static readonly Dictionary<int, float> REINFORCE_NORMAL_LEVEL_SUCCESS_RATE_MAP = new() {
            { 10, 0.9f },
            { 20, 0.85f },
            { 30, 0.8f },
            { 40, 0.75f },
            { 50, 0.7f },
            { 60, 0.65f },
            { 70, 0.6f },
            { 80, 0.55f },
            { 90, 0.5f },
            { 100, 0.45f }
        };

        public static double GetItemDropWeight(int diff) {
            var weight = 0d;
            const double increaseRate = 0.8d;

            if (diff <= 0) weight = 1 + ((-diff / increaseRate));
            else weight = 1 * Mathf.Exp(-diff / 2f);

            return weight;
        }
        
        public static double GetMoneyDropValue(int level) {
            var min = level * 10;
            var max = level * 14;
            return Randomizer.Next(min, max);
        }

        public static double CalculateExp(int level) => DEFAULT_EXP_VALUE * Mathf.Pow((float)LEVEL_COEFFICIENT, level);
    }
}