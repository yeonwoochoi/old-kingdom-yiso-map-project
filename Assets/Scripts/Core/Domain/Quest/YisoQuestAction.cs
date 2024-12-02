using System;
using Core.Domain.Actor.Player;
using Core.Domain.Locale;
using Core.Service;
using Core.Service.Data;
using Core.Service.Data.Item;
using Core.Service.Domain;
using UnityEngine;
using Utils.Extensions;

namespace Core.Domain.Quest {
    public abstract class YisoQuestAction {
        public Types Type { get; }

        protected YisoQuestAction(Types type) {
            Type = type;
        }

        public abstract bool TryGive(YisoPlayer player, out YisoQuestGiveReasons reason);

        public abstract bool CanGive(YisoPlayer player, out YisoQuestGiveReasons reason);

        public abstract (Sprite icon, string target, string value) GetRewardUI(YisoLocale.Locale locale);

        public enum Types {
            EXP, ITEM, MONEY, BUFF, ITEM_REMOVE, ITEM_ADD, COMBAT_RATING
        }
    }

    public class YisoQuestMoneyAction : YisoQuestAction {
        private readonly double amount;

        public YisoQuestMoneyAction(double amount) : base(Types.MONEY) {
            this.amount = amount;
        }
        public override bool TryGive(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            player.InventoryModule.Money += amount;
            return true;
        }

        public override bool CanGive(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            return true;
        }

        public override (Sprite icon, string target, string value) GetRewardUI(YisoLocale.Locale locale) {
            var domainService = YisoServiceProvider.Instance.Get<IYisoDomainService>();
            var sprite = domainService.GetMoneyIcon();
            var title = locale == YisoLocale.Locale.KR ? "량" : "NYANG";
            return (sprite, title, amount.ToCommaString());
        }
    }

    public class YisoQuestExpAction : YisoQuestAction {
        private readonly double exp;

        public YisoQuestExpAction(double exp) : base(Types.EXP) {
            this.exp = exp;
        }
        
        public override bool TryGive(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            return true;
        }

        public override bool CanGive(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            return true;
        }

        public override (Sprite icon, string target, string value) GetRewardUI(YisoLocale.Locale locale) {
            var domainService = YisoServiceProvider.Instance.Get<IYisoDomainService>();
            var sprite = domainService.GetExpIcon();
            var title = locale == YisoLocale.Locale.KR ? "경험치" : "EXP";
            return (sprite, title, exp.ToCommaString());
        }
    }

    public class YisoQuestCombatRatingAction : YisoQuestAction {
        private readonly double combatRating;

        public YisoQuestCombatRatingAction(double combatRating) : base(Types.COMBAT_RATING) {
            this.combatRating = combatRating;
        }
        public override bool TryGive(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            player.StatModule.AdditionalCombatRating += combatRating;
            return true;
        }

        public override bool CanGive(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            return true;
        }

        public override (Sprite icon, string target, string value) GetRewardUI(YisoLocale.Locale locale) {
            var domainService = YisoServiceProvider.Instance.Get<IYisoDomainService>();
            var sprite = domainService.GetCombatRatingIcon();
            var title = locale == YisoLocale.Locale.KR ? "명예점수" : "Honor Rating";
            return (sprite, title, combatRating.ToCommaString());
        }
    }

    public class YisoQuestItemAction : YisoQuestAction {
        private readonly int itemId;
        private readonly int amount;

        public YisoQuestItemAction(int itemId, int amount) : base(Types.ITEM) {
            this.itemId = itemId;
            this.amount = amount;
        }
        public override bool TryGive(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            var dataService = YisoServiceProvider.Instance.Get<IYisoItemService>();
            var item = dataService.GetItemOrElseThrow(itemId);
            if (!player.InventoryModule.CanAdd(item)) {
                reason = YisoQuestGiveReasons.INVENTORY_FULL;
                return false;
            }
            
            player.InventoryModule.AddItem(item, amount);
            return true;
        }

        public override bool CanGive(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            var dataService = YisoServiceProvider.Instance.Get<IYisoItemService>();
            var item = dataService.GetItemOrElseThrow(itemId);
            if (!player.InventoryModule.CanAdd(item)) {
                reason = YisoQuestGiveReasons.INVENTORY_FULL;
            }
            
            return reason == YisoQuestGiveReasons.NONE;
        }

        public override (Sprite icon, string target, string value) GetRewardUI(YisoLocale.Locale locale) {
            var dataService = YisoServiceProvider.Instance.Get<IYisoItemService>();
            var item = dataService.GetItemOrElseThrow(itemId);
            var icon = item.Icon;
            var title = item.GetName(locale);
            return (icon, title, amount.ToCommaString());
        }
    }

    public class YisoQuestItemRemoveAction : YisoQuestAction {
        private readonly bool removeAll;
        private readonly int amount;

        public int ItemId { get; }

        public YisoQuestItemRemoveAction(int itemId, bool removeAll, int amount) : base(Types.ITEM_REMOVE) {
            this.ItemId = itemId;
            this.removeAll = removeAll;

            if (!removeAll && amount < 1) throw new ArgumentException("Remove Amount must greater than 0");
            
            this.amount = amount;
        }
        
        public override bool TryGive(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            var count = removeAll ? -1 : amount;
            if (!player.InventoryModule.ExistItem(ItemId, count)) {
                reason = YisoQuestGiveReasons.NO_ITEM_IN_INVENTORY;
                return false;
            }
            
            if (removeAll) player.InventoryModule.RemoveItemAll(ItemId);
            else player.InventoryModule.RemoveItem(ItemId, count);
            
            return true;
        }

        public override bool CanGive(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            var count = removeAll ? -1 : amount;
            if (!player.InventoryModule.ExistItem(ItemId, count)) {
                reason = YisoQuestGiveReasons.NO_ITEM_IN_INVENTORY;
                return false;
            }
            
            return true;
        }

        public override (Sprite icon, string target, string value) GetRewardUI(YisoLocale.Locale locale) {
            return (null, null, null);
        }
    }

    public class YisoQuestItemAddAction : YisoQuestAction {
        public int ItemId { get; }
        public int Amount { get; }

        public YisoQuestItemAddAction(int itemId, int amount) : base(Types.ITEM_ADD) {
            ItemId = itemId;
            Amount = amount;
        }
        public override bool TryGive(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            var dataService = YisoServiceProvider.Instance.Get<IYisoItemService>();
            var item = dataService.GetItemOrElseThrow(ItemId);
            if (!player.InventoryModule.CanAdd(item)) {
                reason = YisoQuestGiveReasons.INVENTORY_FULL;
                return false;
            }
            
            player.InventoryModule.AddItem(item, Amount);
            return true;
        }

        public override bool CanGive(YisoPlayer player, out YisoQuestGiveReasons reason) {
            reason = YisoQuestGiveReasons.NONE;
            var dataService = YisoServiceProvider.Instance.Get<IYisoItemService>();
            var item = dataService.GetItemOrElseThrow(ItemId);
            if (!player.InventoryModule.CanAdd(item)) {
                reason = YisoQuestGiveReasons.INVENTORY_FULL;
                return false;
            }
            return true;
        }

        public override (Sprite icon, string target, string value) GetRewardUI(YisoLocale.Locale locale) {
            return (null, null, null);
        }
    }
}