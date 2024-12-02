using System;
using System.Collections.Generic;
using System.Linq;
using Character.Weapon;
using Core.Bootstrap;
using Core.Domain.Actor.Player;
using Core.Domain.Cabinet;
using Core.Domain.Data;
using Core.Domain.Dialogue;
using Core.Domain.Direction;
using Core.Domain.Locale;
using Core.Domain.Quest;
using Core.Domain.Quest.SO;
using Core.Domain.Settings;
using Core.Domain.Types;
using Core.Domain.Wanted;
using Core.Logger;
using Core.Service.Character;
using Core.Service.Data.Item;
using Core.Service.Game;
using Core.Service.Stage;
using Core.Service.UI.Global;
using Core.UI.Story;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Utils.Security;

namespace Core.Service.Data {
    public class YisoDataService : IYisoDataService {
        private static readonly string PLAYER_DATA_KEY = "player_data";
        private static readonly string GAME_DATA_KEY = "game_data";

        private readonly YisoGameData gameData;
        private readonly YisoPlayerData playerData;
        private readonly SaveTimings saveSettings;
        private readonly DataVersionInfo dataVersionInfo;
        
        private JsonSerializerSettings serializerSettings;
        
        private YisoDataService() { }

        private YisoDataService(Settings settings, YisoBootstrap.BuildVersionInfo versionInfo) {
            saveSettings = settings.saveSettings;
            dataVersionInfo = settings.dataVersionInfo;
            serializerSettings = new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.All
            };

            if (!TryGet(PLAYER_DATA_KEY, out playerData)) {
                playerData = new YisoPlayerData();
                Set(PLAYER_DATA_KEY, playerData);
            }

            if (!TryGet(GAME_DATA_KEY, out gameData)) {
                gameData = new YisoGameData();
                Set(GAME_DATA_KEY, gameData);
            }

            if (gameData.versionData.CompareVersion(versionInfo)) return;
            gameData.versionData.UpdateVersion(versionInfo);
            Set(GAME_DATA_KEY, gameData);
        }

        internal static YisoDataService CreateService(Settings settings, YisoBootstrap.BuildVersionInfo versionInfo) {
            return new YisoDataService(settings, versionInfo);
        }

        public void SavePlayerData(YisoPlayer player, bool showUI = false) {
            player.ToSaveData(playerData);
            
            if (showUI) {
                ShowUI(SaveData);
                return;
            }
            
            SaveData();
            return;
            
            void SaveData() {
                var existSaveData = LoadPlayerData();
                var saveData = player.ToSaveData();
                saveData.version.Copy(dataVersionInfo);
                saveData.initData = existSaveData.initData;
                saveData.stageId = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId();
                SavePlayerData(saveData);
            }
        }

        public void SavePlayerData(YisoPlayerData data, bool showUI = false) {
            if (!YisoServiceProvider.Instance.Get<IYisoGameService>().IsSaveData()) return;
            Set(PLAYER_DATA_KEY, data);
        }

        public YisoPlayerData LoadPlayerData() => playerData;

        public void RemovePlayerData() {
            PlayerPrefs.DeleteKey(PLAYER_DATA_KEY);
        }

        public void SaveData(SaveTimings timings, bool force = false) {
            var save = false;

            if (force) save = true;
            else {
                save = saveSettings.HasFlag(timings);
            }

            if (!save) return;
            var globalUIService = YisoServiceProvider.Instance.Get<IYisoGlobalUIService>();
            globalUIService.ShowSaving(() => {
                if (timings == SaveTimings.GAME_SETTING_CHANGE) {
                    SaveGameData();
                }
                else {
                    var player = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer();
                    SavePlayerData(player);
                }
            });
        }

        public YisoGameData GetGameData() {
            return gameData;
        }

        public void SaveGameData(bool showUI = false) {
            Set(GAME_DATA_KEY, gameData);
        }

        public void OnDestroy() { }

        public bool IsReady() => true;

        public DataVersionInfo GetDataVersionInfo() => dataVersionInfo;

        private void ShowUI(UnityAction onShowed) {
            var globalUIService = YisoServiceProvider.Instance.Get<IYisoGlobalUIService>();
            globalUIService.ShowSaving(onShowed);
        }
        
        private T Get<T>(string key) {
            if (!PlayerPrefs.HasKey(key)) return default;
            var value = PlayerPrefs.GetString(key, string.Empty);
            if (value == string.Empty) return default;
            return JsonConvert.DeserializeObject<T>(value, serializerSettings);
        }

        private bool TryGet<T>(string key, out T value) {
            value = Get<T>(key);
            return value != null;
        }

        private void Set(string key, object value) {
            var jsonValue = JsonConvert.SerializeObject(value, Formatting.Indented, serializerSettings);
            PlayerPrefs.SetString(key, jsonValue);
        }
        
        [Serializable]
        public class Settings {
            [Title("Save Settings"), EnumToggleButtons]
            public SaveTimings saveSettings = SaveTimings.DEFAULT;

            [Title("Version")] public DataVersionInfo dataVersionInfo;
        }

        [Flags]
        public enum SaveTimings {
            NONE = 0,
            PLAYER_LEVEL_UP = 1 << 0,
            PLAYER_STAGE_CHANGE = 1 << 1,
            PLAYER_MONEY_CHANGE = 1 << 2,
            PLAYER_QUEST_COMPLETE = 1 << 3,
            GAME_SETTING_CHANGE = 1 << 4,
            DEFAULT = PLAYER_LEVEL_UP | PLAYER_STAGE_CHANGE | GAME_SETTING_CHANGE,

            ALL = PLAYER_LEVEL_UP | PLAYER_STAGE_CHANGE | PLAYER_MONEY_CHANGE | PLAYER_QUEST_COMPLETE |
                  GAME_SETTING_CHANGE
        }
    }
}