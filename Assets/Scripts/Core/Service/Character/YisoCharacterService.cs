using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.Constants;
using Core.Domain.Actor.Ally;
using Core.Domain.Actor.Enemy;
using Core.Domain.Actor.Enemy.Drop;
using Core.Domain.Actor.Erry;
using Core.Domain.Actor.Npc;
using Core.Domain.Actor.Player;
using Core.Domain.Actor.Player.SO;
using Core.Domain.Drop;
using Core.Domain.Entity;
using Core.Domain.Item;
using Core.Domain.Locale;
using Core.Domain.Settings;
using Core.Domain.Types;
using Core.Logger;
using Core.Service.Bounty;
using Core.Service.Data;
using Core.Service.Data.Item;
using Core.Service.Domain;
using Core.Service.Factor.HonorRating;
using Core.Service.Factor.Item;
using Core.Service.Game;
using Core.Service.Scene;
using Core.Service.Stage;
using UnityEngine;
using Utils;
using Utils.Extensions;
using Utils.Randoms;

namespace Core.Service.Character {
    public class YisoCharacterService : IYisoCharacterService {
        private readonly YisoPlayer player;
        private YisoErry erry;
        private readonly Dictionary<string, YisoEnemy> enemies = new();
        private readonly Dictionary<string, YisoAlly> allies = new();
        private readonly Dictionary<int, YisoNpc> npcs = new();
        
        private readonly YisoErrySO errySO;
        
        public YisoPlayer GetPlayer() => player;
        
        private readonly IYisoDataService dataService;
        private readonly IYisoStageService stageService;
        private readonly IYisoDomainService domainService;
        private readonly IYisoSceneService sceneService;
        private readonly IYisoHonorRatingFactorService honorRatingFactorService;
        private readonly IYisoItemFactorService itemFactorService;

        private readonly YisoPlayerInventoryItemsSO inventoryItemsSO = null;

        private bool isDataVersionMatched = true;

        private YisoPlayerModes playerMode = YisoPlayerModes.BASE_CAMP;

        public bool IsDataVersionMatched() => isDataVersionMatched;
        
        public YisoEnemy CreateEnemy(YisoEnemySO enemySO, bool developMode = true) {
            var stageCR = stageService.GetCurrentStageCR();
            var enemy = new YisoEnemy(enemySO, stageCR, honorRatingFactorService);
            enemy.OnDeathEvent += OnDeath;
            enemies[enemy.ObjectId] = enemy;
            return enemy;
        }

        public YisoAlly CreateAlly(YisoAllySO allySO, bool developMode = true) {
            var stageCR = stageService.GetCurrentStageCR();
            var ally = new YisoAlly(allySO, stageCR, honorRatingFactorService);
            ally.OnDeathEvent += OnDeathAlly;
            allies[ally.ObjectId] = ally;
            return ally;
        }

        public YisoNpc CreateNpc(YisoNpcSO npcSO) {
            var npc = new YisoNpc(npcSO);
            npcs[npc.Id] = npc;
            return npc;
        }

        public void ResetPlayerData() {
            var saveData = dataService.LoadPlayerData();
            
            saveData.Reset();
            player.ResetData(domainService.GetQuests());
            stageService.SetCurrentStageId(saveData.stageId, true);
            player.LoadData(saveData, inventoryItemsSO);
            
            saveData.initData = false;
            saveData.version.Copy(dataService.GetDataVersionInfo());
            
            player.ToSaveData(saveData);
            dataService.SavePlayerData(saveData);
            
            var stageCR = stageService.GetCurrentStageId();
            erry = new YisoErry(errySO, stageCR, honorRatingFactorService);
            player.Erry = erry;
        }

        public List<YisoDropItem> GetDropItems(IYisoEntity entity = null) {
            var itemService = YisoServiceProvider.Instance.Get<IYisoItemService>();
            var stageId = stageService.GetCurrentStageId();
            var result = new List<YisoDropItem>();
            
            // MONEY CALC
            var money = itemFactorService.CalculateMoney(stageId);
            
            var enemy = (YisoEnemy) entity!;
            
            var enemyMoneyFactor = itemFactorService.GetEnemyMoneyDropRate(enemy.Type);

            var moneyDropRate = player.StatModule.MoneyDropRate.ToNormalized();
            
            money *= enemyMoneyFactor;
            money *= moneyDropRate;
            
            var enemyItemProbFactor = itemFactorService.GetEnemyDropItemRate(enemy.Type);
 
            
            result.Add(YisoDropItem.CreateMoneyDrop(money));

            
            // Drop Calc

            if (playerMode == YisoPlayerModes.STORY) {
                if (!TryGenerateRandomItem(out var item)) return result;
                result.Add(YisoDropItem.CreateItemDrop(item));
                return result;
            }

            if (playerMode != YisoPlayerModes.BOUNTY) return result;

            if (enemy.Type != YisoEnemyTypes.FIELD_BOSS) {
                if (!TryGenerateRandomItem(out var item)) return result;
                result.Add(YisoDropItem.CreateItemDrop(item));
                return result;
            }
            
            var currentBounty = YisoServiceProvider.Instance.Get<IYisoBountyService>().GetCurrentBounty();
            
            foreach (var equipItem in currentBounty.RewardItemIds.Select(id => itemService.GetItemOrElseThrow(id)).Cast<YisoEquipItem>()) {
                var createItem = itemService.CreateRandomItem(equipItem);
                result.Add(YisoDropItem.CreateItemDrop(createItem));
            }

            YisoServiceProvider.Instance.Get<IYisoBountyService>().SetCurrentBounty(null);
            return result;
            
            bool TryGenerateRandomItem(out YisoItem item) {
                item = null;
                // var itemDropRate = player.StatModule.ItemDropRate.ToNormalized();
                // var dropProb = ProbUtils.Add(itemDropRate, enemyItemProbFactor);
                if (!Randomizer.Below(enemyItemProbFactor)) return false;
                item = itemService.CreateRandomItem(stageId);
                return true;
            }
        }
        
        private YisoCharacterService(Settings settings) {
            errySO = settings.errySO;
            inventoryItemsSO = settings.inventoryItemsSO;
            
            var provider = YisoServiceProvider.Instance;
            dataService = provider.Get<IYisoDataService>();
            stageService = provider.Get<IYisoStageService>();
            domainService = provider.Get<IYisoDomainService>();
            sceneService = provider.Get<IYisoSceneService>();
            honorRatingFactorService = provider.Get<IYisoHonorRatingFactorService>();
            itemFactorService = provider.Get<IYisoItemFactorService>();
            
            stageService.RegisterOnStageChanged(OnStageChanged);
            sceneService.RegisterOnSceneChanged(OnSceneChanged);
            
            var quests = domainService.GetQuests();
            player = new YisoPlayer(quests);
        }
        
        
        ~YisoCharacterService() {
            stageService.UnregisterOnStageChanged(OnStageChanged);
            sceneService.UnregisterOnSceneChanged(OnSceneChanged);
        }

        public void LoadPlayerData() {
            var saveData = dataService.LoadPlayerData();
            stageService.SetCurrentStageId(saveData.stageId, true);
            player.LoadData(saveData, inventoryItemsSO);

            if (saveData.initData) {
                saveData.version.Copy(dataService.GetDataVersionInfo());
                saveData.initData = false;
                player.ToSaveData(saveData);
                dataService.SavePlayerData(saveData);
            }

            isDataVersionMatched = dataService.GetDataVersionInfo().IsMatched(saveData.version);

            var stageCR = stageService.GetCurrentStageCR();
            erry = new YisoErry(errySO, stageCR, honorRatingFactorService);
            player.Erry = erry;
        }

        private void OnDeath(string id) {
            var enemy = enemies[id];
            enemy.OnDeathEvent -= OnDeath;
            enemies.Remove(id);
        }

        private void OnDeathAlly(string objectId) {
            var ally = allies[objectId];
            ally.OnDeathEvent -= OnDeathAlly;
            allies.Remove(objectId);
        }

        private void OnStageChanged() {
            var stageCR = stageService.GetCurrentStageCR();
            player.Erry.OnChangeStage(stageCR, honorRatingFactorService);
            foreach (var ally in allies.Values) ally.CalculateStageCR(stageCR, honorRatingFactorService);
        }

        private void OnSceneChanged(YisoSceneTypes beforeScene, YisoSceneTypes afterScene) {
            playerMode = afterScene switch {
                YisoSceneTypes.STORY => YisoPlayerModes.STORY,
                YisoSceneTypes.BOUNTY => YisoPlayerModes.BOUNTY,
                _ => YisoPlayerModes.BASE_CAMP
            };
        }

        public void OnDestroy() { }


        internal static YisoCharacterService CreateService(Settings settings) => new (settings);
        public bool IsReady() => true;

        [Serializable]
        public class Settings {
            public YisoErrySO errySO;
            public YisoPlayerInventoryItemsSO inventoryItemsSO;
        }
    }
}