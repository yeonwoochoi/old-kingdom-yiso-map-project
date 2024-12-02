using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Domain.Data;
using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Domain.Types;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Extensions;
using Utils.Security;

namespace Test {
    public class YisoPlayerDataTest : MonoBehaviour {
        [SerializeField, Title("PackSO")] private YisoItemPackSO itemPack;

        [SerializeField, Title("Data")] private YisoPlayerData data = default;

        private bool loadedData = false;

        [Button]
        public void PrintDataPath() {
            var dataPath = Application.dataPath.Replace("Assets", "Data");
            Debug.Log(dataPath);
        }

        [Button, ShowIf("loadedData")]
        public void SaveToJson() {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var dataPath = Application.dataPath.Replace("Assets", "Data");
            var savePath = Path.Join(dataPath, "player_data.json");
            var savedData = JsonConvert.SerializeObject(data, settings);
            File.WriteAllText(savePath, savedData);
        }
        
        [Button]
        public void LoadToJson() {
            var dataPath = Application.dataPath.Replace("Assets", "Data");
            var savedPath = Path.Join(dataPath, "player_data.json");
            var texts = File.ReadAllText(savedPath);
            data = JsonConvert.DeserializeObject<YisoPlayerData>(texts);
            loadedData = true;
        }

        [Button, HideInEditorMode]
        public void ResetData() {
            SecurePlayerPrefs.DeleteKey("saveData");
        }

        [Button, HideInEditorMode]
        public void SaveData() {
            SecurePlayerPrefs.Set("saveData", data);
        }

        [Button, HideInEditorMode]
        public void LoadData() {
            if (!SecurePlayerPrefs.TryGet("saveData", out data)) {
                SecurePlayerPrefs.Set("saveData", new YisoPlayerData());
            }

            loadedData = true;
        }

        [Button, HideInEditorMode]
        public void AddInitEquips() {
            var statsSo = new List<YisoEquipItemSO>();
            var slots = EnumExtensions.Values<YisoEquipSlots>();

            foreach (var slot in slots) {
                var slotStats = FindStats(slot);
                statsSo.AddRange(slotStats);
            }

            for (var i = 0; i < statsSo.Count; i++) {
                var so = statsSo[i];
                var item = (YisoEquipItem) so.CreateItem();
                var savedItem = item.Save();
                savedItem.position = i;
                savedItem.id = i;
                savedItem.quantity = 1;

                data.inventoryData.items ??= new List<YisoPlayerItemData>();
                data.inventoryData.items.Add(savedItem);
            }
        }

        [Button, HideInEditorMode]
        public void CheckSavedItem() {
            if (data.inventoryData.items == null) {
                Debug.Log("No saved items.");
                return;
            }

            foreach (var item in data.inventoryData.items) {
                if (item is YisoPlayerEquipItemData equipItem) {
                    var check = false;
                    foreach (var so in itemPack.items) {
                        if (so.id != item.itemId) continue;
                        Debug.Log($"Equip Item: {so.name[YisoLocale.Locale.KR]}");
                        check = true;
                        break;
                    }

                    if (!check) {
                        Debug.Log($"Saved Item: {item.itemId} not found!");
                    }
                }
            }
        }

        [Button, ShowIf("loadedData")]
        public void AddItems(YisoItemSO[] items) {
            if (items == null || items.Length == 0) return;
            var inventoryItemCount = data.inventoryData.items.Count;
            for (var i = 0; i < items.Length; i++) {
                var so = items[i];
                var item = (YisoUseItem) so.CreateItem();
                var savedItem = item.Save();
                savedItem.position = i;
                savedItem.id = i + inventoryItemCount;
                savedItem.quantity = 50;
                data.inventoryData.items.Add(savedItem);
            }
        }

        private IEnumerable<YisoEquipItemSO> FindStats(YisoEquipSlots slot, int count = 2) {
            var result = new List<YisoEquipItemSO>();
            var currentCount = 0;
            foreach (var item in itemPack.items.TakeWhile(i => currentCount != count)) {
                if (!(item is YisoEquipItemSO equipItem)) continue;
                if (equipItem.slot != slot) continue;
                result.Add(equipItem);
                currentCount++;
            }

            return result;
        }
    }
}