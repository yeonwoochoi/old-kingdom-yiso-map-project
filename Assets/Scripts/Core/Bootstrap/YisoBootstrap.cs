using System;
using System.Collections.Generic;
using System.Linq;
using Core.Behaviour;
using Core.Domain.Bounty;
using Core.Domain.Data;
using Core.Domain.Types;
using Core.Logger;
using Core.ObjectPool;
using Core.Service;
using Core.Service.Bounty;
using Core.Service.Character;
using Core.Service.CoolDown;
using Core.Service.Data;
using Core.Service.Data.Item;
using Core.Service.Domain;
using Core.Service.Effect;
using Core.Service.Factor.Combat;
using Core.Service.Factor.HonorRating;
using Core.Service.Factor.Item;
using Core.Service.Game;
using Core.Service.Log;
using Core.Service.Map;
using Core.Service.ObjectPool;
using Core.Service.Scene;
using Core.Service.Server;
using Core.Service.Stage;
using Core.Service.Temp;
using Core.Service.UI;
using Core.Service.UI.Game;
using Core.Service.UI.Global;
using Core.Service.UI.HUD;
using Core.Service.UI.Menu;
using Core.Service.UI.Popup;
using Core.Service.UI.Popup2;

using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Security;

namespace Core.Bootstrap {
    public class YisoBootstrap : RunIBehaviour {
        [SerializeField, Title("Global")] private GameObject globalUICanvas;
        [SerializeField] private YisoGameService.Settings gameSettings;
        [SerializeField] private YisoDomainService.Settings domainSettings;
        [SerializeField] private YisoDataService.Settings dataSettings;
        [SerializeField] private YisoItemService.Settings itemSettings;
        [SerializeField] private YisoObjectPoolService.Settings poolSettings;
        [SerializeField] private YisoMapService.Settings mapSettings;
        [SerializeField] private YisoStageService.Settings stageSettings;
        [SerializeField] private YisoCharacterService.Settings characterSettings;
        [SerializeField] private YisoBountyService.Settings bountySettings;
        [SerializeField] private YisoLogService.Settings logSettings;

        [SerializeField, Title("Init Settings")]
        private YisoSceneTypes currentScene = YisoSceneTypes.INIT;


        [SerializeField] private bool attachUI = true;
        

        [Title("Logger Settings")]
        [SerializeField] private GameObject logHelperPrefab;

        [SerializeField, Title("Build Info")] public BuildVersionInfo versionInfo;

        [Button]
        public void ResetPlayerData() {
            PlayerPrefs.DeleteKey("player_data");
        }
        
        protected override void Awake() {
            base.Awake();

            if (GameObject.FindWithTag("Bootstrap") == null) {
                DontDestroyOnLoad(gameObject);
                gameObject.tag = "Bootstrap";
            }
            else {
                Destroy(gameObject);
                return;
            }

            RegisterServices();
            Instantiate(globalUICanvas, transform.position, Quaternion.identity);
        }

        private void RegisterServices() {
            var serviceProvider = YisoServiceProvider.Instance;

            if (serviceProvider.Initialized) return;
            var loggerBehaviour = Instantiate(logHelperPrefab, transform.position, Quaternion.identity)
                .GetComponent<YisoLoggerBehaviour>();
            
            serviceProvider.Register(typeof(IYisoLogService), YisoLogService.CreateService(logSettings, loggerBehaviour));

            serviceProvider.Register(typeof(IYisoServerService), YisoServerService.CreateService());
            RegisterFactorServices(serviceProvider);
            
            
            serviceProvider.Register(typeof(IYisoMapService), YisoMapService.CreateService(mapSettings));
            serviceProvider.Register(typeof(IYisoStageService),
                YisoStageService.CreateService(stageSettings));
            serviceProvider.Register(typeof(IYisoDomainService), YisoDomainService.CreateService(domainSettings));
            serviceProvider.Register(typeof(IYisoDataService),
                YisoDataService.CreateService(dataSettings, versionInfo));
            serviceProvider.Register(typeof(IYisoItemService), YisoItemService.CreateService(itemSettings));
            
            serviceProvider.Register(typeof(IYisoGameService), YisoGameService.CreateService(gameSettings));
            serviceProvider.Register(typeof(IYisoGlobalUIService), YisoGlobalUIService.CreateService());
            serviceProvider.Register(typeof(IYisoSceneService), YisoSceneService.CreateService(serviceProvider));

            serviceProvider.Register(typeof(IYisoCoolDownService), YisoCoolDownService.CreateService(this));
            serviceProvider.Register(typeof(IYisoEffectService), YisoEffectService.CreateService(this));
            serviceProvider.Register(typeof(IYisoCharacterService),
                YisoCharacterService.CreateService(characterSettings));
            serviceProvider.Register(typeof(IYisoObjectPoolService),
                YisoObjectPoolService.CreateService(
                    CreateObject<YisoObjectPoolController>(poolSettings.objectPoolPrefab, null)));
            
            serviceProvider.Register(typeof(IYisoBountyService), YisoBountyService.CreateService(bountySettings));

            // UI Services
            serviceProvider.Register(typeof(IYisoUIService), YisoUIService.CreateService());
            serviceProvider.Register(typeof(IYisoHUDUIService), YisoHUDUIService.CreateService());
            serviceProvider.Register(typeof(IYisoGameUIService), YisoGameUIService.CreateService());
            serviceProvider.Register(typeof(IYisoPopupUIService), YisoPopupUIService.CreateService());
            serviceProvider.Register(typeof(IYisoPopup2UIService), YisoPopup2UIService.CreateService());
            serviceProvider.Register(typeof(IYisoMenuUIService), YisoMenuUIService.CreateService());
            
            serviceProvider.Register(typeof(IYisoTempService), YisoTempService.CreateService());

            serviceProvider.Get<IYisoSceneService>().SetCurrentScene(currentScene);
            if (attachUI)
                serviceProvider.Get<IYisoSceneService>().LoadUIScene();
            serviceProvider.Initialized = true;
            
            serviceProvider.Get<IYisoCharacterService>().LoadPlayerData();
        }

        private void RegisterFactorServices(YisoServiceProvider serviceProvider) {
            serviceProvider.Register(typeof(IYisoHonorRatingFactorService), YisoHonorRatingFactorService.CreateService(serviceProvider));
            serviceProvider.Register(typeof(IYIsoCombatFactorService), YisoCombatFactorService.CreateService());
            serviceProvider.Register(typeof(IYisoItemFactorService), YisoItemFactorService.CreateService());
        }

        [Serializable]
        public class BuildVersionInfo {
            public int major;
            public int minor;
            public int patch;
        }
    }
}