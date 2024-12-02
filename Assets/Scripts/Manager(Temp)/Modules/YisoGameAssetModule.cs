using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Manager_Temp_.Modules {
    public class YisoGameAssetModule : YisoGameBaseModule {
        private readonly Settings settings;

        public GameObject QuestRewardPrefab => settings.questRewardBoxPrefab;
        public GameObject EmoticonPrefab => settings.emoticonPrefab;
        public GameObject DefaultMouseClickDetectorPrefab => settings.defaultMouseClickDetector;

        public YisoGameAssetModule(GameManager manager, Settings settings) : base(manager) {
            this.settings = settings;
        }

        [Serializable]
        public class Settings {
            [Title("Quest")] public GameObject questRewardBoxPrefab;

            [Title("Emoticon")] public GameObject emoticonPrefab;
            
            [Title("Detector")] public GameObject defaultMouseClickDetector;
        }
    }
}