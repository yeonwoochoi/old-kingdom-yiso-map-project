using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Manager_Temp_.Modules {
    [Serializable]
    public class YisoGameModules {
        #region Settings

        [Title("Module Settings")] [SerializeField]
        private YisoGameEffectModule.Settings effectSettings;

        [SerializeField] private YisoGameDropItemModule.Settings dropItemSettings;
        [SerializeField] private YisoGameAssetModule.Settings assetSettings;
        [SerializeField] private YisoGamePetModule.Settings petSettings;
        [SerializeField] private YisoGameQuestModule.Settings questSettings;

        #endregion

        #region Modules

        public YisoGameEffectModule EffectModule { get; private set; }
        public YisoGameDropItemModule DropItemModule { get; private set; }
        public YisoGameAssetModule AssetModule { get; private set; }
        public YisoGamePetModule PetModule { get; private set; }
        public YisoGameQuestModule QuestModule { get; private set; }

        #endregion

        public IEnumerable<YisoGameBaseModule> Modules {
            get {
                yield return EffectModule;
                yield return DropItemModule;
                yield return AssetModule;
                yield return PetModule;
                yield return QuestModule;
            }
        }

        public void InitializeModules(GameManager manager) {
            EffectModule = new YisoGameEffectModule(manager, effectSettings);
            DropItemModule = new YisoGameDropItemModule(manager, dropItemSettings);
            AssetModule = new YisoGameAssetModule(manager, assetSettings);
            PetModule = new YisoGamePetModule(manager, petSettings);
            QuestModule = new YisoGameQuestModule(manager, questSettings);
        }
    }
}