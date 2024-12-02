using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using Utils.Extensions;
using Random = Unity.Mathematics.Random;

namespace Utils {
    public static class Randomizer {
        private static readonly System.Random Generator = new System.Random();

        public static int NextInt() => Generator.Next();

        public static int NextInt(int start, int end) => Generator.Next(start, end);

        public static int NextByte() => (byte) Generator.Next();

        public static int NextInt(int arg) => Generator.Next(arg);

        public static bool NextBool() => Generator.NextDouble() >= 0.5;

        public static long NextLong() => (long) Generator.NextDouble();

        public static int Rand(int lBound, int uBound) =>
            (int) ((Generator.NextDouble() * (uBound - lBound + 1)) + lBound);

        public static long BigRand(long lBound, int uBound) =>
            (long) ((Generator.NextDouble() * (uBound - lBound + 1)) + lBound);

        public static int ScrollRand(int min, int max) {
            var a = 0;
            do {
                a = NextInt(max + 1);
            } while (a >= min);

            return a;
        }

        public static bool IsSuccess(int rate) => rate >= NextInt(100);

        public static bool IsSuccess(int rate, int max) => rate > NextInt(max);

        public static string Comma(long r) => r.ToString();
        
        public static float Next() => (float) Generator.NextDouble();

        public static int Next(int from, int to) {
            return @from >= to ? @from : Generator.Next(@from, to);
        }

        public static double Next(double from, double to) {
            if (from >= to) return from;
            var range = to - from;
            var sample = Generator.NextDouble();
            return (sample * range) + from;
        }

        public static float Next(float from, float to) {
            if (from >= to) return from;
            var range = to - from;
            var sample = (float) Generator.NextDouble();
            return (sample * range) + from;
        }

        public static int Next(int to) => Next(0, to);

        public static float Next(float to) => Next(0, to);

        public static double Next(double to) => Next(0, to);

        public static Color NextColor() => new Color(Next(0f, 1f), Next(0f, 1f), Next(0f, 1f), 1f);

        public static T NextEnum<T>() where T : Enum {
            var count = EnumExtensions.Counts<T>();
            var idx = Next(count);
            return idx.ToEnum<T>();
        }

        public static T NextEnum<T>(params T[] excludes) where T : Enum {
            var count = EnumExtensions.Counts<T>();
            var excludeIdxList = excludes.Select(ex => ex.IndexOf()).ToList();
            var idxList = Enumerable.Range(0, count)
                .Where(idx => !excludeIdxList.Contains(idx))
                .ToList();
            if (idxList.IsEmpty()) throw new Exception("Enum value cannot exist!");
            return idxList.TakeRandom(1).First().ToEnum<T>();
        }
        
        public static Vector3 NextVector3(float from = 0, float to = 0.2f) =>
            new Vector3(Next(from, to), Next(from, to));

        public static Vector3 NextVector3(this Vector3 current, float from = 0, float to = 0.2f) {
            var randomVector = NextVector3(from, to);
            return new Vector3(current.x + randomVector.x, current.y + randomVector.y);
        }

        public static Vector3 NextVector3(float xFrom = 0f, float xTo = 0.2f, float yFrom = 0f, float yTo = 0.2f) {
            return new Vector3(
                Next(xFrom, xTo),
                Next(yFrom, yTo)
            );
        }

        public static bool Below(float percent) => Next(1.0f) < percent;

        public static bool Below(double percent) => Next(1.0f) < percent;

        public static bool Above(float percent) => Next(1.0f) > percent;

        public static T TakeRandom<T>(this T[] arr) {
            var nextIndex = Next(0, arr.Count());
            return arr[nextIndex];
        }

        public static List<T> TakeRandom<T>(this List<T> list, int count) {
            var result = new HashSet<T>();
            
            while (result.Count != count) {
                var nextIndex = Next(0, list.Count);
                result.Add(list[nextIndex]);
            }

            return result.ToList();
        }

        public class WeightedRandomSelector<T> {
            private readonly List<(T item, double cumulativeWeight)> weightedItems;
            private readonly System.Random random;

            public WeightedRandomSelector() {
                var now = DateTime.Now.ToLocalTime();
                var span = (now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
                var timestamp = (int)span.TotalSeconds;
                random = new System.Random(timestamp);
                weightedItems = new List<(T item, double cumulativeWeight)>();
            }
            
            public WeightedRandomSelector(Dictionary<T, double> weights) : this() {
                var cumulativeWeight = 0d;

                foreach (var (key, value) in weights) {
                    cumulativeWeight += value;
                    weightedItems.Add((key, cumulativeWeight));
                }
            }

            public WeightedRandomSelector(Dictionary<T, double> weights, int seed) : this(weights) {
                random = new System.Random(seed);
            }

            public void AddItem(T item, double weight) {
                var cumulativeWeight = GetCumulativeWeight();

                cumulativeWeight += weight;
                weightedItems.Add((item, cumulativeWeight));
            }

            public void AddItems(List<(T item, double weight)> weights) {
                var cumulativeWeight = GetCumulativeWeight();

                foreach (var (item, weight) in weights) {
                    cumulativeWeight += weight;
                    weightedItems.Add((item, cumulativeWeight));
                }
            }

            public void ClearItems() {
                weightedItems.Clear();
            }

            public T GetRandomItem() {
                var randomValue = random.NextDouble() * weightedItems.Last().cumulativeWeight;
                return weightedItems.First(item => item.cumulativeWeight >= randomValue).item;
            }

            private double GetCumulativeWeight() {
                var cumulativeWeight = 0d;
                if (weightedItems.Count > 0) {
                    cumulativeWeight = weightedItems.Last().cumulativeWeight;
                }

                return cumulativeWeight;
            }

            public override string ToString() {
                var builder = new StringBuilder("======= Weights ========\n");
                foreach (var (item, weight) in weightedItems) {
                    builder.Append($"{item}\t: {weight}").Append("\n");
                }

                return builder.ToString();
            }
        }
    }
}