using System.Collections;
using Camera;
using Character.Core;
using Controller.Map;
using Core.Domain.Locale;
using Core.Domain.Map;
using Core.Domain.Stage;
using Core.Logger;
using Core.Service;
using Core.Service.Character;
using Core.Service.Game;
using Core.Service.Log;
using Core.Service.Map;
using Core.Service.Stage;
using Core.Service.Temp;
using Core.Service.UI.Game;
using Core.Service.UI.Popup;
using DG.Tweening;
using Sirenix.OdinInspector;
using Tools.Event;
using Tools.Singleton;
using UnityEngine;

namespace Manager_Temp_ {
    public struct YisoStageChangeEvent {
        public YisoStage prevStage;
        public YisoStage currentStage;
        public bool isMapChanged;

        public YisoStageChangeEvent(YisoStage prevStage, YisoStage currentStage, bool isMapChanged = true) {
            this.prevStage = prevStage;
            this.currentStage = currentStage;
            this.isMapChanged = isMapChanged;
        }

        static YisoStageChangeEvent e;

        public static void Trigger(YisoStage prevStage, YisoStage currentStage, bool isMapChanged = true) {
            e.prevStage = prevStage;
            e.currentStage = currentStage;
            e.isMapChanged = isMapChanged;
            YisoEventManager.TriggerEvent(e);
        }
    }

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
        public YisoLocale.Locale CurrentLocale => YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<StageManager>();
        public YisoMapController CurrentMapController => currentMapController;
        public YisoStage CurrentStage => currentStage;
        public int DeathCount => deathCount;

        protected YisoStage currentStage;
        protected YisoMap currentMap;
        protected YisoMapController currentMapController;
        protected GameObject currentMapObj;
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
            if (!GameManager.HasInstance) yield break;

            // Set Current Mode
            GameManager.Instance.CurrentGameMode = GameManager.GameMode.Story;

            // Initialize Current Stage
            if (!InitializeCurrentStage()) yield break;

            // Instantiate Map
            yield return StartCoroutine(InstantiateMap());

            // Check Condition (Map)
            if (currentMap == null) yield break;

            // Spawn Delay
            if (spawnDelay > 0) yield return new WaitForSeconds(spawnDelay);

            // Initialize Current Map
            currentMapController.Initialization(currentMap, currentStage.Id, currentStage.RelevantStageIds);

            // Instantiate Character
            InstantiatePlayer();
            if (Player == null) yield break;

            // Initialize Pet
            CharacterService.GetPlayer().PetModule.UnregisterAll();

            // Spawn Character
            SpawnPlayer();

            // Checkpoint Assignment (Erry, Enemies)
            currentMapController.CheckpointAssignment();

            // Stage Change Event
            TriggerStageChangeEvent(currentStage, currentStage);

            // Change Stage Quest Status to Ready
            InitializeQuests();

            // Initialize Death Count
            deathCount = 0;

            // In Game Event (Game Start)
            TriggerGameStartEvents();

            // Fade In (밝아지는거)
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(Fade(false, 0.5f));
            
            // Portal로 이동시 freeze된 character를 unfreeze
            Player?.UnFreeze(YisoCharacterStates.FreezePriority.Portal);

            // Loading Comment
            GameUIService.ShowStageLoadingComment(false, currentStage.Id);

            currentStageID = currentStage.Id;
        }

        protected virtual bool InitializeCurrentStage() {
#if UNITY_EDITOR
            if (testMode) {
                return StageService.TryGetStage(startStage, out currentStage);
            }
#endif
            return StageService.TryGetStage(StageService.GetCurrentStageId(), out currentStage);
        }

        protected virtual IEnumerator InstantiateMap() {
            if (MapService.TryGetMap(currentStage.Id, out var newCurrentMap)) {
                // 기존에 있는 Map 제거
                CleanupPreviousMap(newCurrentMap);

                yield return null;

                currentMap = newCurrentMap;
                currentMapObj = Instantiate(currentMap.Prefab, mapAttachment);
                currentMapController = currentMapObj.GetComponentInChildren<YisoMapController>();
                currentMapController.transform.localPosition = Vector3.zero;
            }
        }
        
        private void CleanupPreviousMap(YisoMap newCurrentMap) {
            if (currentMap != null && currentMap.Id != newCurrentMap.Id) {
                Destroy(currentMapObj);
                currentMapController = null;
            }

            var mapControllers = FindObjectsOfType<YisoMapController>(true);
            foreach (var controller in mapControllers) {
                if (controller.CurrenMap == null || controller.CurrenMap.Id == newCurrentMap.Id) {
                    Destroy(controller.gameObject);
                }
            }
        }

        // TODO : Spawn Pets
        protected virtual void InstantiatePlayer() {
            // 이미 Player가 Scene 내에 있는 경우
            if (characterInScene != null) {
                GameManager.Instance.Player = characterInScene;
                return;
            }

            if (playerPrefab != null) {
                var newPlayer = Instantiate(playerPrefab, playerAttachment);
                newPlayer.transform.position = currentMapController.InitialSpawnPointPosition;
                newPlayer.name = playerPrefab.name;
                GameManager.Instance.Player = newPlayer;
                characterInScene = Player;

                if (playerPrefab.characterType != YisoCharacter.CharacterTypes.Player) {
                    LogService.Fatal("[StageManager] The Character you've set in the StageManager isn't a Player, which means it's probably not going to move. You can change that in the Character component of your prefab.");
                }
            }
        }

        protected virtual void SpawnPlayer() {
            YisoInGameEvent.Trigger(YisoInGameEventTypes.SpawnCharacterStarts, Player, currentStage.Id);
            currentMapController.SpawnPlayer(Player, false);
            // TODO : Spawn Pets
            YisoInGameEvent.Trigger(YisoInGameEventTypes.SpawnComplete, Player, currentStage.Id);
        }
        
        protected virtual void InitializeQuests() {
            foreach (var mainQuest in currentStage.MainQuests) {
                CharacterService.GetPlayer().QuestModule.ReadyQuest(mainQuest.Id);
            }
        }

        private void TriggerStageChangeEvent(YisoStage prevStage, YisoStage currentStage) {
            YisoStageChangeEvent.Trigger(prevStage, currentStage);
        }

        private void TriggerGameStartEvents() {
            YisoInGameEvent.Trigger(YisoInGameEventTypes.StageStart, Player, currentStage.Id);
            YisoCameraEvent.Trigger(YisoCameraEventTypes.SetTargetCharacter, Player);
            YisoCameraEvent.Trigger(YisoCameraEventTypes.StartFollowing);
        }

        #endregion

        #region Player Death

        public void OnEvent(YisoInGameEvent inGameEvent) {
            if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.Story) return;
            switch (inGameEvent.eventType) {
                case YisoInGameEventTypes.PlayerDeath:
                    HandlePlayerDeath();
                    break;
                case YisoInGameEventTypes.RespawnStarted:
                    Restart();
                    break;
            }
        }
        
        protected virtual void HandlePlayerDeath() {
            deathCount++;
            if (Player != null) {
                StartCoroutine(PlayerDeathCo());
            }
        }
        
        protected virtual IEnumerator PlayerDeathCo() {
            yield return new WaitForSeconds(delayBeforeDeathScreen);
            YisoServiceProvider.Instance.Get<IYisoPopupUIService>().AlertS(
                deathPopupUITitle[CurrentLocale], 
                deathPopupUIContent[CurrentLocale], 
                Restart, 
                GameManager.Instance.LoadBaseCampScene);
        }
        
        #endregion

        #region Restart

        public virtual void Restart() {
            if (Player != null) {
                StartCoroutine(RestartGame());
            }
        }

        protected virtual IEnumerator RestartGame() {
            if (playerPrefab == null && characterInScene == null) yield break;

            // Camera Event (Stop Following)
            YisoCameraEvent.Trigger(YisoCameraEventTypes.StopFollowing);

            // Fade out (어둡게)
            yield return StartCoroutine(Fade(true, 0.25f));

            // Respawn Delay
            yield return new WaitForSeconds(delayBeforeRespawn);

            // Respawn Player
            currentMapController.RestartGame();
            if (Player == null) InstantiatePlayer();
            currentMapController.SpawnPlayer(Player, true);

            // Camera Event (Start Following)
            YisoCameraEvent.Trigger(YisoCameraEventTypes.StartFollowing);

            // Fade In (밝게)
            yield return StartCoroutine(Fade(false, 0.25f));

            // In Game Event (Respawn Complete)
            YisoInGameEvent.Trigger(YisoInGameEventTypes.RespawnComplete, Player, currentStage.Id);
        }

        #endregion

        #region Stage Clear

        public void ShowStageClearPopup() {
            GameUIService.ShowStoryClearPopup(
                () => {
                    GameManager.Instance.LoadBaseCampScene();
                    UpdateNextStageID();
                },
                () => { StartCoroutine(MoveToNextStageCo()); },
                () => { Player?.UnFreeze(YisoCharacterStates.FreezePriority.Portal); },
                currentStage.Id < stageLimit);
        }

        /// <summary>
        /// 현재 Map을 유지하고 Stage만 넘어갈때 사용하는 Public API
        /// </summary>
        public virtual void MoveToNextStage() {
            var prevStage = currentStage;
            UpdateNextStageID();

            // Stage Change Event
            YisoStageChangeEvent.Trigger(prevStage, currentStage, false);

            // Change Stage Quest Status to Ready
            InitializeQuests();
        }

        /// <summary>
        /// Stage가 넘어감과 동시에 Map도 바뀔때 사용하는 API
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator MoveToNextStageCo() {
            // Set Next Stage ID
            UpdateNextStageID();

            // Fade Out
            yield return StartCoroutine(Fade(true, 0.2f));
            yield return new WaitForSeconds(delayBeforeNextStage);

            // Trigger In Game Event
            YisoInGameEvent.Trigger(YisoInGameEventTypes.MoveNextStageMap, Player, currentStage.Id);

            // Initialize Next Stage
            StartCoroutine(InitializationCo());
        }

        protected virtual void UpdateNextStageID() {
            var stageId = currentStage?.Id ?? StageService.GetCurrentStageId();
            int nextStageId = Mathf.Clamp(stageId + 1, 1, stageLimit);
            StageService.SetCurrentStageId(nextStageId);
            YisoInGameEvent.Trigger(YisoInGameEventTypes.MoveNextStage, Player, nextStageId);
            StageService.TryGetStage(nextStageId, out currentStage);
            currentStageID = nextStageId;
        }

#if UNITY_EDITOR
        [Button, ShowIf("testMode")]
        public void SetStageForTesting(int nextStageId) {
            if (!testMode) return;
            StageService.SetCurrentStageId(nextStageId);
            YisoInGameEvent.Trigger(YisoInGameEventTypes.MoveNextStage, Player, nextStageId);
            StageService.TryGetStage(nextStageId, out currentStage);
            currentStageID = nextStageId;
        }

        [Button]
        public void ShowQuestStatus(int questId) {
            Debug.Log($"{questId}:{YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().QuestModule.GetStatusByQuestId(questId)}");
        }
#endif

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

        protected virtual void OnEnable() {
            this.YisoEventStartListening();
        }

        protected virtual void OnDisable() {
            this.YisoEventStopListening();
        }
    }
}