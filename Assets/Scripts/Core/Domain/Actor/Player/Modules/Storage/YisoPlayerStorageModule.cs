using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Actor.Player.Modules.Base;
using Core.Domain.Actor.Player.Modules.Inventory;
using Core.Domain.Data;
using Core.Domain.Item;
using Core.Service;
using Core.Service.Data;
using Core.Service.Data.Item;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace Core.Domain.Actor.Player.Modules.Storage {
    public class YisoPlayerStorageModule : YisoPlayerBaseModule {
        public event UnityAction<YisoPlayerInventoryEventArgs> OnStorageEvent;
        
        public Dictionary<YisoItem.InventoryType, YisoPlayerInventoryUnit> StorageUnits { get; } = new();

        public double Money { get; set; } = 0;

        public YisoPlayerStorageModule(YisoPlayer player) : base(player) {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                StorageUnits[type] = new YisoPlayerInventoryUnit(type);
            }
        }

        public void ResetData() {
            foreach (var type in EnumExtensions.Values<YisoItem.InventoryType>()) {
                StorageUnits[type] = new YisoPlayerInventoryUnit(type);
            }
        }

        public void AddItem(YisoItem item, int count = 1, bool isLoad = false) {
            if (item.InvType == YisoItem.InventoryType.EQUIP) count = 1;

            StorageUnits[item.InvType].TryAddItem(item, count, out var argsList);
            
            if (isLoad) return;
            
            foreach (var args in argsList) RaiseEvent(args);
        }

        public void PullItem(YisoItem item, int count) {
            var position = item.Position;
            var quantity = count;
            
            StorageUnits[item.InvType].TryRemoveItem(position, quantity, out _, out var args);
            RaiseEvent(args);
            item.Quantity = quantity;
            player.InventoryModule.AddItem(item, quantity);
        }

        public void SaveData(ref YisoPlayerData data) {
            foreach (var unit in StorageUnits.Values) {
                foreach (var (position, item) in unit.ItemDict) {
                    var saveItem = item.Save();
                    data.storageData.items.Add(saveItem);
                }
            }
        }

        public void LoadData(YisoPlayerData data) {
            var dataService = YisoServiceProvider.Instance.Get<IYisoItemService>();
            foreach (var itemData in data.storageData.items) {
                YisoItemSO so = null;
                var item = dataService.GetItemOrElseThrow(itemData.itemId);
                if (item.InvType == YisoItem.InventoryType.EQUIP)
                    so = dataService.GetItemSOOrElseThrow(itemData.itemId);
                
                item.Load(itemData, so);
                AddItem(item, isLoad: true);
            }
        }

        public bool CanAdd(YisoItem item) => StorageUnits[item.InvType].CanAdd();
        
        public T GetItem<T>(YisoItem.InventoryType type, int position) where T : YisoItem =>
            StorageUnits[type][position] as T;

        private void RaiseEvent(YisoPlayerInventoryEventArgs args) {
            OnStorageEvent?.Invoke(args);
        }
    }
}