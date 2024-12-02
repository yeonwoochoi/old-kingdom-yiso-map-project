using System;
using System.Text;

namespace Core.Domain.Data {
    [Serializable]
    public class YisoPlayerData {
        public int stageId = 1;
        public YisoPlayerStatData statData = new();
        public YisoPlayerInventoryData inventoryData = new();
        public YisoPlayerStorageData storageData = new();
        public YisoPlayerSkillData skillData = new();
        public YisoPlayerUIData uiData = new();
        public bool initData = true;
        public DataVersionInfo version = new();

        public void Reset() {
            stageId = 1;
            inventoryData.Reset();
            initData = true;
        }
    }

    [Serializable]
    public class YisoPlayerStatData {
        public int maxHp = 100;
        public int hp = 100;
    }
}