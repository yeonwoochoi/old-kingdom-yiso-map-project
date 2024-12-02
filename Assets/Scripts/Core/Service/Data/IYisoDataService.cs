using System.Collections.Generic;
using Core.Domain.Actor.Player;
using Core.Domain.Cabinet;
using Core.Domain.Data;
using Core.Domain.Dialogue;
using Core.Domain.Direction;
using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Domain.Quest;
using Core.Domain.Settings;
using Core.Domain.Store;
using Core.Domain.Wanted;
using UnityEngine;

namespace Core.Service.Data {
    public interface IYisoDataService : IYisoService {
        public YisoPlayerData LoadPlayerData();
        public void SavePlayerData(YisoPlayerData data, bool showUI = false);
        public void SavePlayerData(YisoPlayer player, bool showUI = false);
        public YisoGameData GetGameData();
        public DataVersionInfo GetDataVersionInfo();
        public void SaveGameData(bool showUI = false);
    }
}