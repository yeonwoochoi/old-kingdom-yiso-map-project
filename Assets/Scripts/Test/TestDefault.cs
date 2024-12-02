using Core.Domain.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Test {
    public class TestDefault : MonoBehaviour {
        [Button]
        public void TestExcludeRandom(YisoEquipSlots[] excludes) {
            Debug.Log(Randomizer.NextEnum<YisoEquipSlots>(excludes));
        }

        [Button]
        public void TestRandomInt(int start = 0, int end = 10) {
            Debug.Log(Randomizer.NextInt(start, end));
        }
    }
}