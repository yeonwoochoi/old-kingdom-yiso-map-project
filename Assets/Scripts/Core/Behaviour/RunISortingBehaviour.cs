using System.Collections.Generic;

namespace Core.Behaviour {
    public abstract class RunISortingBehaviour : RunIBehaviour {
        private static readonly Dictionary<string, int> SORTING_DICT = new();

        protected override void Awake() {
            SORTING_DICT[GetSortingId()] = GetInitialSorting();
        }
        
        protected abstract string GetSortingId();

        protected virtual int GetInitialSorting() => 1000;
        
        protected int GetNextSortingOrder() {
            var sortingId = GetSortingId();
            
            if (SORTING_DICT[sortingId] + 1 < int.MaxValue) {
                SORTING_DICT[sortingId] += 1;
            } else {
                SORTING_DICT[sortingId] = GetInitialSorting();
            }
            
            return SORTING_DICT[sortingId];
        }
    }
}