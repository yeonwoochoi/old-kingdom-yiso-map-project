using System;
using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Domain.Actor.Player.Modules.UI;
using Utils.Extensions;

namespace Core.Domain.Data {
    [Serializable]
    public class YisoPlayerUIData {
        public Dictionary<int, int[]> slots = new();

        public YisoPlayerUIData() {
            foreach (var type in EnumExtensions.Values<YisoWeapon.AttackType>()
                         .Where(t => t != YisoWeapon.AttackType.None)) {
                var typeInt = (int)type;
                slots[typeInt] = new int[YisoPlayerUIModule.SLOT_COUNT];
                
                for (var i = 0; i < YisoPlayerUIModule.SLOT_COUNT; i++) {
                    slots[typeInt][i] = -1;
                }
            }
        }
    }
}