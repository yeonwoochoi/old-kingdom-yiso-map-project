using Core.Service;
using Core.Service.Character;
using UnityEngine;

namespace Items.Pickable {
    [AddComponentMenu("Yiso/Items/Coin Pickable Object")]
    public class YisoCoinPickableObject : YisoPickableObject {
        protected double quantity;

        public virtual void SetMoney(double value) {
            quantity = value;
        }

        protected override void Pick(GameObject picker) {
            var characterService = YisoServiceProvider.Instance.Get<IYisoCharacterService>();
            if (characterService.IsReady()) {
                characterService.GetPlayer().InventoryModule.AddMoney(quantity);
            }
        }
    }
}