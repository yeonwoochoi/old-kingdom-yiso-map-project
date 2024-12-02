using System.Collections.Generic;
using Core.Domain.Quest;
using Core.Domain.Stage;
using UnityEngine.Events;

namespace Core.Service.Stage {
    public interface IYisoStageService : IYisoService {
        public bool TryGetStage(int id, out YisoStage stage);
        public int GetCurrentStageId();
        public void SetCurrentStageId(int currentStageId, bool init = false);
        public int GetLastStageId();
        public bool TryGetNextStageAndUpdate(int id, out YisoStage stage);
        public bool ExistNextStage();
        public List<YisoQuest> GetQuestsInStage(int stageId);
        public List<int> GetRelevantStageIds(int stageId = -1);
        public double GetCurrentStageCR();
        public void RegisterOnStageChanged(UnityAction handler);
        public void UnregisterOnStageChanged(UnityAction handler);
    }
}