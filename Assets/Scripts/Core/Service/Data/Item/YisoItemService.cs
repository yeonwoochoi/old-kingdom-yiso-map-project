using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Character.Weapon;
using Core.Domain.Actor.Player.SO;
using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Store;
using Core.Domain.Types;
using Core.Service.Factor.HonorRating;
using Core.Service.Factor.Item;
using Core.Service.Stage;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using Utils;
using Utils.Extensions;

namespace Core.Service.Data.Item {
    public class YisoItemService : IYisoItemService {
        private readonly Dictionary<int, YisoItem> items = new();
        private readonly Dictionary<int, YisoItemSO> itemSOs = new();
        private readonly Dictionary<int, YisoSetItem> setItems = new();
        private readonly Dictionary<int, YisoStore> stores;

        private readonly IYisoItemFactorService itemFactorService;
        private readonly IYisoHonorRatingFactorService honorRatingFactorService;

        private YisoItemService(Settings settings) {
            var provider = YisoServiceProvider.Instance;
            honorRatingFactorService = provider.Get<IYisoHonorRatingFactorService>();
            itemFactorService = provider.Get<IYisoItemFactorService>();
            
            var equipItemPack = settings.equipItemPack.CreatePack();
            var setItemPack = settings.setItemPack.CreatePack();
            var useItemPack = settings.useItemPack.CreatePack();
            var etcItemPack = settings.etcItemPack.CreatePack();

            foreach (var item in settings.equipItemPack.items) {
                itemSOs[item.id] = item;
            }

            foreach (var item in equipItemPack.Items) items[item.Id] = item;
            foreach (var item in useItemPack.Items) items[item.Id] = item;
            foreach (var item in etcItemPack.Items) items[item.Id] = item;
            foreach (var item in setItemPack.SetItems) {
                var includeItems = item.ItemIds;
                
                foreach (var itemId in includeItems) {
                    ((YisoEquipItem)items[itemId]).SetItemId = item.Id;
                }
                
                setItems[item.Id] = item;
            }
            
            stores = settings.storePackSO.ToDictionary();
            CreateRandomItemToStore(stores[0]);
        }

        public YisoItem GetItemOrElseThrow(int id) {
            if (!items.TryGetValue(id, out var item))
                throw new Exception($"Item(id={id} not found!");
            return item;
        }

        public YisoItemSO GetItemSOOrElseThrow(int id) {
            if (!itemSOs.TryGetValue(id, out var so))
                throw new Exception($"Item SO(id={id}) not found!");
            return so;
        }

        public bool TryGetSetItem(int id, out YisoSetItem item) => setItems.TryGetValue(id, out item);

        public YisoItem CreateRandomWeapon(YisoWeapon.AttackType type, YisoEquipRanks rank) {
            var faction = itemFactorService.GetRandomFaction();
            var stageId = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();
            return CreateRandomItem(stageId, faction, YisoEquipSlots.WEAPON, rank);
        }



        private YisoItem CreateRandomWeaponItem(int stageId, YisoEquipFactions faction,
            YisoWeapon.AttackType attackType, YisoEquipSubTypes subType, YisoEquipRanks rank) {
            var minRank = faction.ToRank();
            YisoItem randomItem = null;
            do {
                var itemConst = 3 * (int)(Math.Pow(10, 6));
                var factionConst = (int)faction * (int)(Math.Pow(10, 5));
                var slotConst = (int)(YisoEquipSlots.WEAPON) * (int)(Math.Pow(10, 3));
                var subTypeConst = subType.IndexOf() * 10;
                var rankConst = (int)minRank + 1;

                var id = itemConst + factionConst + slotConst + subTypeConst + rankConst;
                items.TryGetValue(id, out randomItem);
            } while (randomItem == null);
            
            var randomEquip = (YisoEquipItem)randomItem;
            randomEquip.Rank = rank;
            randomEquip.CalculateCombatRating(honorRatingFactorService);
            
            SetSellPrice(stageId, ref randomItem);
            
            return randomItem;
        }

        private YisoItem CreateRandomItem(int stageId, YisoEquipFactions faction, YisoEquipSlots slot, YisoEquipRanks rank) {
            var minRank = faction.ToRank();
            
            YisoItem randomItem = null;
            do {
                YisoEquipSubTypes subType;
                if (slot == YisoEquipSlots.WEAPON) {
                    var attackType = Randomizer.NextEnum(YisoWeapon.AttackType.None);
                    subType = YisoEquipItemTypeConst.GetSubTypesByAttack(attackType)
                        .ToList().TakeRandom(1).First();
                } else {
                    subType = YisoEquipItemTypeConst.GetSubTypesBySlots(slot)
                        .ToList().TakeRandom(1).First();
                }
                
                var itemConst = 3 * (int)(Math.Pow(10, 6));
                var factionConst = (int)faction * (int)(Math.Pow(10, 5));
                var slotConst = (int)slot * (int)(Math.Pow(10, 3));
                var subTypeConst = subType.IndexOf() * 10;
                var rankConst = (int)minRank + 1;

                var id = itemConst + factionConst + slotConst + subTypeConst + rankConst;

                items.TryGetValue(id, out randomItem);
            } while (randomItem == null);
            
            var randomEquip = (YisoEquipItem)randomItem;
            randomEquip.Rank = rank;
            randomEquip.CalculateCombatRating(honorRatingFactorService);
            
            SetSellPrice(stageId, ref randomItem);
            
            return randomItem;
        }
        
        public YisoItem CreateRandomEquip(YisoEquipRanks rank) {
            var stageId = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();

            var faction = itemFactorService.GetRandomFaction();
            var slot = itemFactorService.GetRandomSlot();
            return CreateRandomItem(stageId, faction, slot, rank);
        }

        public YisoItem CreateRandomItem(int stageId = -1) {
            if (stageId == -1)
                stageId = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();

            var faction = itemFactorService.GetRandomFaction();
            var slot = itemFactorService.GetRandomSlot();
            var rank = itemFactorService.GetRandomRank();
            return CreateRandomItem(stageId, faction, slot, rank);
        }

        public YisoItem CreateRandomItem(YisoEquipItem item) {

            var stageId = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();
            var faction = item.Faction;
            var slot = item.Slot;
            var rank = item.Rank.GetRandomRank(2);
            if (slot == YisoEquipSlots.WEAPON) {
                var subType = item.SubType;
                return CreateRandomWeaponItem(stageId, faction, item.AttackType, subType, rank);
            }
            return CreateRandomItem(stageId, faction, slot, rank);
        }

        private void SetStoreItemPrices(int stageId, YisoStore store) {
            foreach (var item in store.EquipItems) {
                SetStoreItemPrice(stageId, item);
            }

            foreach (var item in store.UseItems) {
                SetStoreItemPrice(stageId, item);
            }
        }

        private void SetStoreItemPrice(int stageId, YisoStoreItem storeItem) {
            if (storeItem.Item is not YisoEquipItem equipItem) return;
            storeItem.Price = itemFactorService.GetPrices(stageId, equipItem).totalBuyPrice;
        }

        private void SetSellPrice(int stageId, ref YisoItem item) {
            var (_, _, sellPrice) = itemFactorService.GetPrices(stageId, item); 
            switch (item) {
                case YisoEquipItem:
                    item.SellPrice = sellPrice * 0.65;
                    break;
                case YisoUseItem:
                    break;
                case YisoEtcItem:
                    break;
            }
        }

        public List<YisoItem> CreateItemFromSO(YisoPlayerInventoryItemsSO so) {
            var result = new List<YisoItem>();
            var stageId = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();

            foreach (var item in so.items) {
                if (item.random) {
                    if (item.RandomAll) {
                        result.Add(CreateRandomItem(stageId));
                        continue;
                    }

                    var faction = item.randomFaction ? itemFactorService.GetRandomFaction() : item.faction;
                    var slot = item.randomSlot ? itemFactorService.GetRandomSlot() : item.slots;
                    var rank = item.randomRank ? itemFactorService.GetRandomRank() : item.rank;
                    
                    result.Add(CreateRandomItem(stageId, faction, slot, rank));
                    continue;
                }

                var equipItem = items[item.itemSO.id];
                equipItem = equipItem.DeepCopy();
                
                ((YisoEquipItem) equipItem).CalculateCombatRating(honorRatingFactorService);
                SetSellPrice(stageId, ref equipItem);
                result.Add(equipItem);
            }

            return result;
        }
        
        public bool TryGetStore(int id, out YisoStore store) => stores.TryGetValue(id, out store);
        
        public bool IsReady() => true;

        public void OnDestroy() { }

        public YisoStore CreateDevStore(int stageId, int count = 15) {
            var storeItems = new List<YisoStoreItem>();

            var store = new YisoStore(stageId);

            
            if (count > 0) {
                for (var i = 0; i < count; i++) {
                    var faction = itemFactorService.GetRandomFaction();
                    var slot = itemFactorService.GetRandomSlot();
                    var randomItem = CreateRandomItem(stageId, faction, slot, YisoEquipRanks.S);
                    storeItems.Add(new YisoStoreItem(randomItem, -1));
                }
                
                store.EquipItems.AddRange(storeItems);
            }

            var arrowItemId = 20000101;
            var arrowItem = GetItemOrElseThrow(arrowItemId);
            
            store.UseItems.Add(new YisoStoreItem(arrowItem, 100));
            SetStoreItemPrices(stageId, store);
            return store;
        }
        
        public YisoStore CreateStageStore(int stageId, int count = 20) {
            var storeItems = new List<YisoStoreItem>();

            for (var i = 0; i < count; i++) {
                storeItems.Add(new YisoStoreItem(CreateRandomItem(stageId), -1));
            }

            var store = new YisoStore(stageId);
            store.EquipItems.AddRange(storeItems);
            SetStoreItemPrices(stageId, store);
            return store;
        } 

        private void CreateRandomItemToStore(YisoStore store, int count = 20) {
            var result = new List<YisoStoreItem>();

            var stageId = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();
            
            for (var i = 0; i < 20; i++) {
                var item = CreateRandomItem(stageId);
                result.Add(new YisoStoreItem(item, -1));
            }

            store.EquipItems.AddRange(result);
            SetStoreItemPrices(stageId, store);
        }

        public static YisoItemService CreateService(Settings settings) => new(settings);

        [Serializable]
        public class Settings {
            public YisoItemPackSO equipItemPack;
            public YisoItemPackSO useItemPack;
            public YisoItemPackSO etcItemPack;
            public YisoSetItemPackSO setItemPack;
            public YisoStorePackSO storePackSO;
        }
    }
}