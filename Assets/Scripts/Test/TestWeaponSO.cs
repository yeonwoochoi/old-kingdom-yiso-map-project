using Core.Domain.Item;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Test {
    public class TestWeaponSO : MonoBehaviour {
        [SerializeField] private YisoItemPackSO equipPackSO;

        private string sword = "검";
        private string bow = "활";
        private string spear = "창";

        [Button]
        public void FindSword() {
            var item = equipPackSO.items.FindAll(item => item.name.kr.EndsWith(sword));
            Debug.Log(item.Count);
        }
    }
}