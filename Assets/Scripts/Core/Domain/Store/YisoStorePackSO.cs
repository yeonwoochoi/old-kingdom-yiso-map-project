using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Domain.Store {
    [CreateAssetMenu(fileName = "StorePack", menuName = "Yiso/Store/Pack")]
    public class YisoStorePackSO : ScriptableObject {
        public List<YisoStoreSO> stores;

        public Dictionary<int, YisoStore> ToDictionary() =>
            stores.ToDictionary(store => store.id, store => store.CreateStore());
    }
}