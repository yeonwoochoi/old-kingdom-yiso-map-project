using System;
using Core.Domain.Item;
using Sirenix.OdinInspector;

namespace Core.Domain.Quest.SO {
    [Serializable]
    public class YisoQuestActionSO {
        [Title("Basic")] public YisoQuestAction.Types type;

        [ShowIf("type", YisoQuestAction.Types.EXP)]
        public long exp;

        [ShowIf("type", YisoQuestAction.Types.ITEM)]
        public YisoItemSO itemSO;
        [ShowIf("type", YisoQuestAction.Types.ITEM)]
        public int amount;

        [ShowIf("type", YisoQuestAction.Types.ITEM_REMOVE)]
        public YisoItemSO removeItemSO;

        [ShowIf("type", YisoQuestAction.Types.ITEM_REMOVE)]
        public bool removeAll = false;

        [ShowIf("@this.type == YisoQuestAction.Types.ITEM_REMOVE && !this.removeAll"), MinValue(1)]
        public int removeAmount = 1;

        [ShowIf("type", YisoQuestAction.Types.ITEM_ADD)]
        public YisoItemSO addItemSO;

        [ShowIf("type", YisoQuestAction.Types.ITEM_ADD), MinValue(1)]
        public int addAmount = 1;
        
        [ShowIf("type", YisoQuestAction.Types.MONEY)]
        public double money;

        [ShowIf("type", YisoQuestAction.Types.COMBAT_RATING)]
        public double combatRating;
        
        public YisoQuestAction CreateAction() {
            switch (type) {
                case YisoQuestAction.Types.EXP:
                    return new YisoQuestExpAction(exp);
                case YisoQuestAction.Types.ITEM:
                    return new YisoQuestItemAction(itemSO.id, amount);
                case YisoQuestAction.Types.MONEY:
                    return new YisoQuestMoneyAction(money);
                case YisoQuestAction.Types.ITEM_ADD:
                    return new YisoQuestItemAddAction(addItemSO.id, addAmount);
                case YisoQuestAction.Types.ITEM_REMOVE:
                    return new YisoQuestItemRemoveAction(removeItemSO.id, removeAll, removeAmount);
                case YisoQuestAction.Types.COMBAT_RATING:
                    return new YisoQuestCombatRatingAction(combatRating);
            }
            return null;
        }
    }
}