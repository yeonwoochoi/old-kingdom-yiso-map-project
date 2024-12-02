using System.Collections.Generic;

namespace Utils.Randoms {
    public abstract class BaseRandomizer<T> {
        public abstract T GetRandomItem();

        public abstract void AddItem(T item, double weight);
        public abstract void AddItems(Dictionary<T, double> weights);
        public abstract void RemoveItem(T item);

        public abstract void Clear();
    }

    public class WeightedRandomizer<T> : BaseRandomizer<T> {
        private double totalWeight = 0;
        private readonly List<T> items = new();
        private readonly List<double> weights = new();
        private readonly List<double> originalWeights = new();

        public WeightedRandomizer(Dictionary<T, double> weights) {
            foreach (var (key, weight) in weights) {
                items.Add(key);
                totalWeight += weight;
                this.weights.Add(totalWeight);
                originalWeights.Add(weight);
            }
        }

        public WeightedRandomizer() {
            
        }
        
        public override T GetRandomItem() {
            var random = Randomizer.Next() * totalWeight;
            var index = BinarySearchBoundary(weights, random);
            return items[index];
        }
        

        public override void AddItem(T item, double weight) {
            items.Add(item);
            totalWeight += weight;
            weights.Add(totalWeight);
            originalWeights.Add(weight);
        }

        public override void AddItems(Dictionary<T, double> weights) {
            foreach (var (item, weight) in weights) {
                AddItem(item, weight);
            }
        }

        public override void RemoveItem(T item) {
            var index = items.IndexOf(item);
            items.RemoveAt(index);
            var weight = weights[index];
            weights.RemoveAt(index);
            originalWeights.RemoveAt(index);
            for (var i = index; i < weights.Count; i++) {
                weights[i] -= weight;
            }
        }

        public double GetWeight(T item) {
            var index = items.IndexOf(item);
            if (index == -1) return -1;
            return originalWeights[index];
        }

        public override void Clear() {
            weights.Clear();
            totalWeight = 0d;
        }

        private int BinarySearchBoundary(IReadOnlyList<double> list, double value) {
            var l = 0;
            var r = list.Count - 1;

            while (l < r) {
                var mid = (l + r) / 2;
                if (list[mid] < value) l = mid + 1;
                else r = mid;
            }

            return r;
        }
    } 
}