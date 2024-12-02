using System;
using System.Collections.Generic;
using Core.Domain.Quest;
using Core.Domain.Stage;
using Core.Logger;
using Core.Service.Character;
using Core.Service.Data;
using Core.Service.Factor.HonorRating;
using Core.Service.Log;
using Core.Service.Server;
using UnityEngine.Events;
using Utils.Extensions;

namespace Core.Service.Stage {
    public class YisoStageService : IYisoStageService {
        private readonly Dictionary<int, YisoStage> stages;
        private event UnityAction OnStageChangeEvent;
        
        private int beforeStageId = 1;
        private int currentStageId = 1;

        public bool IsReady() => true;

        public bool TryGetStage(int id, out YisoStage stage) => stages.TryGetValue(id, out stage);

        public bool TryGetNextStageAndUpdate(int id, out YisoStage stage) {
            stage = null;
            if (currentStageId + 1 > stages.Count) {
                return false;
            }

            currentStageId += 1;
            stage = stages[currentStageId];
            return true;
        }

        public bool ExistNextStage() => currentStageId + 1 <= stages.Count;

        public int GetCurrentStageId() => currentStageId;

        public void SetCurrentStageId(int currentStageId, bool init = false) {
            if (this.currentStageId == currentStageId) return;
            beforeStageId = currentStageId;
            this.currentStageId = currentStageId;
            if (init) return;
            OnStageChangeEvent?.Invoke();
            var player = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer();
            player.OnStageChanged(currentStageId > beforeStageId);
            var serviceProvider = YisoServiceProvider.Instance;
            serviceProvider.Get<IYisoDataService>().SavePlayerData(player, true);
            var logger = serviceProvider.Get<IYisoLogService>().GetLogger<YisoStageService>();
            logger.Debug($"Stage Changed to '{currentStageId}', Required Combat Rating: '{GetCurrentStageCR().ToCommaString()}'");
        }

        public int GetLastStageId() => stages.Count;

        public double GetCurrentStageCR() => stages[currentStageId].CombatRating;

        public List<YisoQuest> GetQuestsInStage(int stageId) {
            if (!TryGetStage(stageId, out var stage))
                throw new Exception($"Stage(id={stageId}) not found!");
            return stage.MainQuests;
        }

        public List<int> GetRelevantStageIds(int stageId = -1) {
            if (stageId == -1) stageId = currentStageId;
            if (!TryGetStage(stageId, out var stage))
                throw new Exception($"Stage(id={stageId}) not found!");
            return stages[stageId].RelevantStageIds;
        }

        private YisoStageService(Settings settings) {
            var honorRatingFactorService = YisoServiceProvider.Instance.Get<IYisoHonorRatingFactorService>();
            stages = settings.stagePackSO.CreateDict();
            foreach (var (key, stage) in stages) {
                var (cr, crDiff) = honorRatingFactorService.GetStageHonorRating(key);
                stage.CombatRating = cr;
                stage.CRDiff = crDiff;
            }
        }

        public void RegisterOnStageChanged(UnityAction handler) {
            OnStageChangeEvent += handler;
        }

        public void UnregisterOnStageChanged(UnityAction handler) {
            OnStageChangeEvent -= handler;
        }

        public void OnDestroy() { }

        
        internal static YisoStageService CreateService(Settings settings) => new(settings);

        [Serializable]
        public class Settings {
            public YisoStagePackSO stagePackSO;
        }
    }
}