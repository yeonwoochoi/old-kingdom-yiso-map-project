using System;
using System.Collections;
using System.Collections.Generic;
using Camera;
using Character.Core;
using Controller.Map;
using Core.Domain.Actor.Player.Modules.Quest;
using Core.Domain.Locale;
using Core.Domain.Map;
using Core.Domain.Stage;
using Core.Logger;
using Core.Service;
using Core.Service.Character;
using Core.Service.Effect;
using Core.Service.Game;
using Core.Service.Log;
using Core.Service.Map;
using Core.Service.Stage;
using Core.Service.Temp;
using Core.Service.UI.Game;
using Core.Service.UI.Popup;
using DG.Tweening;
using Sirenix.OdinInspector;
using Spawn;
using Tools.Event;
using Tools.Singleton;
using UnityEngine;

namespace Manager_Temp_ {
    public class StageManager : YisoTempSingleton<StageManager>, IYisoEventListener<YisoInGameEvent> {
        [Title("Character")] public YisoCharacter playerPrefab;
        public YisoCharacter characterInScene;

        [Title("Attachment")] public Transform playerAttachment;
        public Transform mapAttachment;

        [Title("Stage")] public int stageLimit = 5;
        [ReadOnly] public int currentStageID = 1;

        [Title("Debug")]
#if UNITY_EDITOR
        public bool testMode = true;

        [ShowIf("testMode")] public int startStage = 1;
#endif

        private YisoCharacter Player => GameManager.Instance.Player;
        public IYisoStageService StageService => YisoServiceProvider.Instance.Get<IYisoStageService>();
        public IYisoMapService MapService => YisoServiceProvider.Instance.Get<IYisoMapService>();
        public IYisoGameUIService GameUIService => YisoServiceProvider.Instance.Get<IYisoGameUIService>();
        public IYisoCharacterService CharacterService => YisoServiceProvider.Instance.Get<IYisoCharacterService>();
        public IYisoTempService TempService => YisoServiceProvider.Instance.Get<IYisoTempService>();
        public YisoPlayerQuestModule QuestModule => YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().QuestModule;
        public YisoLocale.Locale CurrentLocale => YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<StageManager>();
        public YisoMapController CurrentMapController => currentMapController;
        public YisoStage CurrentStage => currentStage;
        public int DeathCount => deathCount;

        protected YisoStage prevStage;
        protected YisoStage currentStage;

        protected YisoMap prevMap;
        protected YisoMap currentMap;

        protected bool isInitialMapLoad = true;
        protected GameObject currentMapObj;
        protected YisoMapController currentMapController;

        protected int deathCount = 0;

        private readonly float spawnDelay = 0f; // 캐릭터 스폰 전에 약간의 Delay
        private readonly float delayBeforeDeathScreen = 1f; // 죽고나서 Death Screen 뜨기 전 약간의 Delay
        private readonly float delayBeforeRespawn = 2f; // Fade out 후 character respawn 되기 전 delay
        private readonly float delayBeforeNextStage = 2f; // Next Stage로 이동할 때 Fade out 유지되는 시간

        private readonly YisoLocale deathPopupUITitle = new YisoLocale {
            [YisoLocale.Locale.KR] = "미션 실패",
            [YisoLocale.Locale.EN] = "Mission Failed:"
        };

        private readonly YisoLocale deathPopupUIContent = new YisoLocale {
            [YisoLocale.Locale.KR] = "당신의 캐릭터가 죽었습니다. 부활하시겠습니까?",
            [YisoLocale.Locale.EN] = "Your character has died. Would you like to try again?"
        };

        // TODO 나중에 연동하고 지우기

        #region Test Data

        [Title("For Test")] public YisoMapSO sampleSaveMapSo;
        public Vector2 sampleSavedPlayerPosition;
        public int sampleSavedCheckPointId;

        private YisoMap savedMap;
        private Vector2 savedPlayerPosition;
        private int savedCheckPointId;

        #endregion

        #region Initialization

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        protected static void InitializeStatics() {
            instance = null;
        }

        protected override void Awake() {
            base.Awake();
            StartCoroutine(InitializationCo());
        }

        protected virtual IEnumerator InitializationCo() {
            // Set Current Mode
            if (!GameManager.HasInstance) {
                LogService.Error($"[StageManager.InitializationCo] There is no game manager instance.");
                yield break;
            }

            // Set Game Mode (Story)
            SetGameMode();

            // Get Save Data
            LoadSaveData();

            // Get Stage
            LoadStage(out var stageLoadSuccess);
            if (!stageLoadSuccess) {
                LogService.Error($"[StageManager.InitializationCo] There is no current stage to load.");
                yield break;
            }

            yield return null;

            // Get Map
            SetInitialMap(out var mapLoadSuccess);
            if (!mapLoadSuccess) {
                LogService.Error($"[StageManager.InitializationCo] There is no current map.");
                yield break;
            }
            
            // Instantiate & Initialize Map
            InstantiateMap();
            yield return null;
            InitializeMap();
            
            // Instantiate Player
            InstantiatePlayer();
            
            // Instantiate Pets
            InstantiatePets();

            // Spawn Player (Set Position)
            SpawnPlayer(false);

            // Spawn Pets (Set Position)
            SpawnPets();
            
            // CheckPoint Assignment (체크 포인트에 IRespawnable 모두 등록)
            currentMapController.CheckpointAssignment();
            LogService.Debug($"[StageManager.InitializationCo] Assign IRespawnable objects to check points.");
            
            // Trigger Stage ChangeEvent
            YisoInGameEvent.Trigger(YisoInGameEventTypes.StageStart, Player, currentStage);
            LogService.Debug($"[StageManager.InitializationCo] Trigger Stage Change Event.");
            
            // Change Stage Quest Status to Ready
            InitializeQuests();

            // Initialize Death Count
            deathCount = 0;
            LogService.Debug($"[StageManager.InitializationCo] Set Death Count to 0.");
            
            // Trigger Camera Events
            YisoCameraEvent.Trigger(YisoCameraEventTypes.SetTargetCharacter, Player);
            YisoCameraEvent.Trigger(YisoCameraEventTypes.StartFollowing);
            LogService.Debug($"[StageManager.InitializationCo] Trigger Camera Events.");

            // Fade In (밝아지는거)
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(Fade(false, 0.5f));
            LogService.Debug($"[StageManager.InitializationCo] Fade In.");
            
            // Portal로 이동시 freeze된 character를 unfreeze
            Player?.UnFreeze(YisoCharacterStates.FreezePriority.Portal);
            LogService.Debug($"[StageManager.InitializationCo] Unfreeze Player.");

            // For Debug
            currentStageID = currentStage.Id;
        }

        protected virtual void SetGameMode() {
            GameManager.Instance.CurrentGameMode = GameManager.GameMode.Story;
            LogService.Debug($"[StageManager.SetGameMode] Set game mode to 'Stage'.");
        }

        #endregion

        #region Stage
        
        /// <summary>
        /// Current Stage Setter
        /// </summary>
        /// <param name="success"></param>
        protected virtual void LoadStage(out bool success) {
            prevStage = currentStage;
#if UNITY_EDITOR
            if (testMode) {
                success = StageService.TryGetStage(startStage, out currentStage);
            }
#endif
            success = StageService.TryGetStage(StageService.GetCurrentStageId(), out currentStage);

            LogService.Debug($"[StageManager.LoadStage] Get current stage.");
        }
        
        /// <summary>
        /// 다음 Stage만 넘어갈때 사용하는 Public API
        /// </summary>
        public virtual void MoveToNextStage() {
            prevStage = currentStage;
            UpdateNextStageID();

            // Stage Change Event
            YisoInGameEvent.Trigger(YisoInGameEventTypes.MoveNextStage, Player, currentStage);

            // Change Stage Quest Status to Ready
            InitializeQuests();
        }
        
        protected virtual void UpdateNextStageID() {
            var stageId = currentStage?.Id ?? StageService.GetCurrentStageId();
            int nextStageId = Mathf.Clamp(stageId + 1, 1, stageLimit);
            StageService.SetCurrentStageId(nextStageId);
            StageService.TryGetStage(nextStageId, out currentStage);
            currentStageID = nextStageId;
        }

        protected virtual void Restart() {
            if (Player != null) {
                StartCoroutine(RestartGameCo());
            }
        }

        protected virtual IEnumerator RestartGameCo() {
            if (playerPrefab == null && characterInScene == null) {
                LogService.Error($"[StageManager.RestartGameCo] Both 'playerPrefab' and 'characterInScene' are null. Unable to restart the game. Please ensure that a player prefab is assigned or a character exists in the scene.");
                yield break;
            }
            
            // Camera Event (Stop Following)
            YisoCameraEvent.Trigger(YisoCameraEventTypes.StopFollowing);
            LogService.Debug($"[StageManager.RestartGameCo] Stop camera following.");

            // Fade out (어둡게)
            yield return StartCoroutine(Fade(true, 0.25f));
            LogService.Debug($"[StageManager.RestartGameCo] Fade out.");

            // Respawn Delay
            yield return new WaitForSeconds(delayBeforeRespawn);

            // Check point
            currentMapController.RestartGame();
            
            if (Player == null) {
                InstantiatePlayer();
                LogService.Debug($"[StageManager.RestartGameCo] Player is null. Reinstantiating the Player.");
            }
            SpawnPlayer(true);
            
            // Camera Event (Start Following)
            YisoCameraEvent.Trigger(YisoCameraEventTypes.StartFollowing);
            LogService.Debug($"[StageManager.RestartGameCo] Start camera following.");
            
            // Fade In (밝게)
            yield return StartCoroutine(Fade(false, 0.25f));
            LogService.Debug($"[StageManager.RestartGameCo] Fade in.");
        }

        #endregion

        #region Map
        
        /// <summary>
        /// 초기 Current Map 세팅 (Story Scene 처음 넘어갈 때만 호출됨)
        /// </summary>
        protected virtual void SetInitialMap(out bool success) {
            if (isInitialMapLoad) {
                prevMap = null;
                currentMap = savedMap;
            }
            isInitialMapLoad = false;
            
            // isInitialMapLoad 가 아닌 경우 (ex. NPC 통한 이동) => currentMap을 미리 설정해놨을 것임

            success = currentMap != null;
            LogService.Debug($"[StageManager.SetInitialMap] Set current map from save data.");
        }

        /// <summary>
        /// 다른 Map으로 바꿀 때 사용하는 Public API
        /// </summary>
        /// <param name="newMap"></param>
        public virtual void SetNewMap(YisoMap newMap) {
            prevMap = currentMap;
            currentMap = newMap;
            
            InstantiateMap();
            InitializeMap();
            
            LogService.Debug($"[StageManager.SetNewMap] Change current map ({prevMap?.GetName(CurrentLocale)} => {currentMap?.GetName(CurrentLocale)}).");
        }

        protected virtual void InstantiateMap() {
            // Clean Previous Map
            CleanupPreviousMap();

            // Instantiate Current Map
            currentMapObj = Instantiate(currentMap.Prefab, mapAttachment);
            currentMapObj.transform.localPosition = Vector3.zero;
            currentMapController = currentMapObj.GetComponentInChildren<YisoMapController>();
            LogService.Debug($"[StageManager.InstantiateMap] Instantiate current map.");
        }

        protected virtual void InitializeMap() {
            currentMapController.Initialization(currentMap, savedPlayerPosition, savedCheckPointId, isInitialMapLoad,
                currentStage.Id, currentStage.RelevantStageIds);
            LogService.Debug($"[StageManager.InitializeMap] Initialize current map.");
            
            if (IsMapChanged()) {
                YisoMapChangeEvent.Trigger(prevMap, currentMap, isInitialMapLoad);
                LogService.Debug($"[StageManager.InitializeMap] Trigger map change event.");
            }
        }

        private bool IsMapChanged() {
            return prevMap == null 
                ? currentMap != null 
                : currentMap != null && prevMap.Id != currentMap.Id;
        }

        private void CleanupPreviousMap() {
            if (prevMap != null && currentMap != null && prevMap.Id != currentMap.Id) {
                Destroy(currentMapObj);
                currentMapController = null;
                LogService.Debug($"[StageManager.CleanupPreviousMap] Clean previous map ({currentMapObj.name}).");
            }

            var mapControllers = FindObjectsOfType<YisoMapController>(true);
            foreach (var controller in mapControllers) {
                if (controller.CurrentMap == null || controller.CurrentMap.Id == currentMap.Id) {
                    Destroy(controller.gameObject);
                    LogService.Debug($"[StageManager.CleanupPreviousMap] Clean previous map ({controller.gameObject.name}).");
                }
            }
        }

        #endregion

        #region Player
        
        protected virtual void InstantiatePlayer() {
            // 이미 Player가 Scene 내에 있는 경우
            if (characterInScene != null) {
                GameManager.Instance.Player = characterInScene;
                LogService.Debug($"[StageManager.InstantiatePlayer] Player already exists in the scene. Skipping spawn.");
                return;
            }

            if (playerPrefab != null) {
                var newPlayer = Instantiate(playerPrefab, playerAttachment);
                newPlayer.name = playerPrefab.name;
                GameManager.Instance.Player = newPlayer;
                characterInScene = Player;
                LogService.Debug($"[StageManager.InstantiatePlayer] Instantiate new Player game object.");

                if (playerPrefab.characterType != YisoCharacter.CharacterTypes.Player) {
                    LogService.Fatal("[StageManager.InstantiatePlayer] The Character you've set in the StageManager isn't a Player, which means it's probably not going to move. You can change that in the Character component of your prefab.");
                }
            }
        }

        protected virtual void InstantiatePets() {
            // TODO
        }

        /// <summary>
        /// 정해진 위치에 스폰
        /// </summary>
        protected virtual void SpawnPlayer(bool isRespawn) {
            YisoInGameEvent.Trigger(isRespawn ? YisoInGameEventTypes.RespawnStarted : YisoInGameEventTypes.SpawnCharacterStarts, Player, currentStage);
            LogService.Debug($"[StageManager.SpawnPlayer] Start {(isRespawn ? "Respawn" : "Spawn")} Player.");
            currentMapController.SpawnPlayer(Player, isRespawn);
            YisoInGameEvent.Trigger(isRespawn ? YisoInGameEventTypes.RespawnComplete : YisoInGameEventTypes.SpawnComplete, Player, currentStage);
            LogService.Debug($"[StageManager.SpawnPlayer] Complete {(isRespawn ? "Respawn" : "Spawn")} Player.");
        }

        protected virtual void SpawnPets() {
            // TODO
        }
        
        protected virtual void HandlePlayerDeath() {
            deathCount++;
            LogService.Debug($"[StageManager.HandlePlayerDeath] Increase death count.");
            if (Player != null) {
                StartCoroutine(HandlePlayerDeathCo());
            }
            else {
                LogService.Warn("[StageManager.HandlePlayerDeath] Cannot handle player death. Player instance is null.");
            }
        }

        protected virtual IEnumerator HandlePlayerDeathCo() {
            yield return new WaitForSeconds(delayBeforeDeathScreen);
            YisoServiceProvider.Instance.Get<IYisoPopupUIService>().AlertS(
                deathPopupUITitle[CurrentLocale], 
                deathPopupUIContent[CurrentLocale], 
                Restart, 
                GameManager.Instance.LoadBaseCampScene);
            LogService.Debug($"[StageManager.HandlePlayerDeathCo] Show Alert Popup");
        }

        #endregion

        #region Save Data

        /// <summary>
        /// Save Data 받아옴
        /// </summary>
        protected virtual void LoadSaveData() {
            // TODO: Core System 연동
            savedMap = sampleSaveMapSo.CreateMap();
            savedPlayerPosition = sampleSavedPlayerPosition;
            savedCheckPointId = sampleSavedCheckPointId;

            LogService.Debug(
                $"[StageManager.LoadSaveData] Get save data (mapId: {savedMap.Id}, playerPosition: {savedPlayerPosition}, checkPointId: {savedCheckPointId}).");
        }

        #endregion

        #region Quest

        protected virtual void InitializeQuests() {
            var questNames = new List<string>();
            foreach (var mainQuest in currentStage.MainQuests) {
                CharacterService.GetPlayer().QuestModule.ReadyQuest(mainQuest.Id);
                if (!string.IsNullOrEmpty(mainQuest.GetName(CurrentLocale))) {
                    questNames.Add(mainQuest.GetName(CurrentLocale));
                }
            }
            if (questNames.Count > 0) {
                LogService.Debug($"[StageManager.InitializeQuests] Initialized Main Quests: {string.Join(", ", questNames)}");
            } else {
                LogService.Warn($"[StageManager.InitializeQuests] No Main Quests to Initialize in stage {currentStage.Id}.");
            }
        }

        protected virtual void CheckStageMainQuestsComplete(QuestEventArgs args) {
            switch (args) {
                case QuestStatusChangeEventArgs statusChangeEventArgs:
                    if (statusChangeEventArgs.IsMainQuestsInStageAllComplete) {
                        YisoInGameEvent.Trigger(YisoInGameEventTypes.StageClear, Player, currentStage);
                        LogService.Debug($"[YisoQuestStatusChangeChecker] All main quests for the stage {currentStage.Id} are complete.");
                    }
                    break;
            }
        }

        #endregion
        
        #region Fade

        /// <summary>
        /// true: fade out (어두워지는거)
        /// false: fade in (밝아지는거)
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        protected virtual IEnumerator Fade(bool flag, float duration) {
            yield return GameManager.Instance.Fade(flag, duration).WaitForCompletion();
        }

        #endregion

        public void OnEvent(YisoInGameEvent e) {
            switch (e.eventType) {
                case YisoInGameEventTypes.PlayerDeath:
                    HandlePlayerDeath();
                    break;
                case YisoInGameEventTypes.RespawnStarted:
                    Restart();
                    break;
            }
        }
        
        protected virtual void OnEnable() {
            QuestModule.OnQuestEvent += CheckStageMainQuestsComplete;
            this.YisoEventStartListening();
        }

        protected virtual void OnDisable() {
            QuestModule.OnQuestEvent -= CheckStageMainQuestsComplete;
            this.YisoEventStopListening();
        }
    }
}