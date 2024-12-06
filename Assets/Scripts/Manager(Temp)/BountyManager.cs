using System.Collections;
using Camera;
using Character.Ability;
using Character.Core;
using Controller.Map;
using Core.Domain.Bounty;
using Core.Domain.Locale;
using Core.Domain.Map;
using Core.Domain.Types;
using Core.Logger;
using Core.Service;
using Core.Service.Bounty;
using Core.Service.Character;
using Core.Service.Game;
using Core.Service.Log;
using Core.Service.Map;
using Core.Service.UI.Popup;
using DG.Tweening;
using Sirenix.OdinInspector;
using Tools.Event;
using Tools.Singleton;
using UnityEngine;
using Utils.Beagle;

namespace Manager_Temp_ {
    public struct YisoBountyChangeEvent {
        public YisoBounty currentBounty;

        public YisoBountyChangeEvent(YisoBounty bounty) {
            currentBounty = bounty;
        }

        static YisoBountyChangeEvent e;

        public static void Trigger(YisoBounty bounty) {
            e.currentBounty = bounty;
            YisoEventManager.TriggerEvent(e);
        }
    }

    public class BountyManager : YisoTempSingleton<BountyManager>, IYisoEventListener<YisoInGameEvent> {
        [Title("Character")] public YisoCharacter playerPrefab;
        public YisoCharacter characterInScene;
        public bool addEnemySpawnerInPlayer = true;
        [ShowIf("addEnemySpawnerInPlayer")] public Transform enemyAttachment;

        [Title("Life")] public int maxDeathCount = 3;

        [Title("Attachment")] public Transform playerAttachment;
        public Transform mapAttachment;

        [Title("Bounty")]
        [ReadOnly] public int currentBountyID = 1;
        [ReadOnly] public GameObject boss;
        
        [Title("Npc")]
        public GameObject npcStore;
        public GameObject portal;

        [Title("Debug")]
#if UNITY_EDITOR
        public bool testMode = true;

        [ShowIf("testMode")] public int startStage = 1;
#endif

        private YisoCharacter Player => GameManager.Instance.Player;
        public IYisoBountyService BountyService => YisoServiceProvider.Instance.Get<IYisoBountyService>();
        public IYisoMapService MapService => YisoServiceProvider.Instance.Get<IYisoMapService>();
        public IYisoPopupUIService PopupUIService => YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
        public YisoMapController CurrentMapController => currentMapController;
        public YisoLocale.Locale CurrentLocale => YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<BountyManager>();

        public YisoBounty CurrentBounty => currentBounty;
        public int DeathCount => deathCount;

        protected YisoBounty currentBounty;
        protected YisoMap currentMap;
        protected YisoMapController currentMapController;
        protected GameObject currentMapObj;
        protected int deathCount = 0;
        protected bool isClear = false;
        protected YisoCharacterSpawner enemySpawner;

        private readonly float spawnDelay = 0f; // 캐릭터 스폰 전에 약간의 Delay
        private readonly float delayBeforeDeathScreen = 1f; // 죽고나서 Death Screen 뜨기 전 약간의 Delay
        private readonly float delayBeforeRespawn = 2f; // Fade out 후 character respawn 되기 전 delay
        private readonly float delayBeforeNextStage = 2f; // Next Stage로 이동할 때 Fade out 유지되는 시간

        private readonly YisoLocale deathPopupUITitle = new YisoLocale {
            [YisoLocale.Locale.KR] = "캐릭터 사망",
            [YisoLocale.Locale.EN] = "Player Dead"
        };

        private readonly YisoLocale deathPopupUIContent = new YisoLocale {
            [YisoLocale.Locale.KR] = "캐릭터가 사망했습니다.\n다시 도전하시겠습니까?",
            [YisoLocale.Locale.EN] = "Your character has died. Would you like to try again?"
        };

        private readonly YisoLocale missionFailedPopupUITitle = new YisoLocale {
            [YisoLocale.Locale.KR] = "임무 실패",
            [YisoLocale.Locale.EN] = "Mission Failed"
        };

        private readonly YisoLocale missionFailedPopupUIContent = new YisoLocale {
            [YisoLocale.Locale.KR] = "임무 실패하셨습니다. \n베이스 캠프로 이동하겠습니다.",
            [YisoLocale.Locale.EN] = "Mission Failed. \nReturning to base camp."
        };

        private readonly YisoLocale baseCampPopupUITitle = new YisoLocale {
            [YisoLocale.Locale.KR] = "베이스 캠프 이동",
            [YisoLocale.Locale.EN] = "Base Camp"
        };

        private readonly YisoLocale baseCampPopupUIContent = new YisoLocale {
            [YisoLocale.Locale.KR] = "베이스 캠프로 이동하시겠습니까?",
            [YisoLocale.Locale.EN] = "Would you like to return to the base camp?"
        };
        
        private readonly YisoLocale bossAppearanceUI = new YisoLocale() {
            kr = "보스가 등장했습니다!",
            en = "Boss has appeared!"
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
            GameManager.Instance.CurrentGameMode = GameManager.GameMode.Bounty;

            // Initialize Current Stage
            InitializeCurrentBounty(out var success);

            // Check Condition (Stage)
            if (!success) yield break;

            // Instantiate Map
            yield return StartCoroutine(InstantiateMap());

            // Check Condition (Map)
            if (currentMap == null) yield break;

            // Spawn Delay
            if (spawnDelay > 0) yield return new WaitForSeconds(spawnDelay);

            // Initialize Current Map
            currentMapController.Initialization(currentMap, currentBounty.Id);

            // Instantiate Character
            InstantiatePlayer();
            if (Player == null) yield break;
            
            // Add Enemy Spawner in Player
            if (addEnemySpawnerInPlayer) InitializeEnemySpawner();

            // Initialize Pet
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().PetModule.UnregisterAll();

            // Spawn Character
            SpawnPlayer();

            // Checkpoint Assignment (Erry, Enemies)
            currentMapController.CheckpointAssignment();

            YisoBountyChangeEvent.Trigger(currentBounty);

            // Initialize Death Count
            deathCount = 0;

            // Camera Event
            YisoCameraEvent.Trigger(YisoCameraEventTypes.SetTargetCharacter, Player);
            YisoCameraEvent.Trigger(YisoCameraEventTypes.StartFollowing);
            
            // Inactivate Npc
            npcStore.SetActive(false);
            portal.SetActive(false);

            // Fade In (밝아지는거)
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(Fade(false, 0.5f));

            // Bounty Quest Popup (+ Start Timer)
            PopupUIService.ShowBountyPopup(() => {
                Player.UnFreeze(YisoCharacterStates.FreezePriority.Default);
                StartTimer(currentBounty.TimeLimit);

                // In Game Event (Game Start)
                YisoInGameEvent.Trigger(YisoInGameEventTypes.StageStart, Player, currentBounty);
            });

            currentBountyID = currentBounty.Id;
            
            boss = null;
        }

        protected virtual void InitializeCurrentBounty(out bool success) {
#if UNITY_EDITOR
            if (testMode) {
                BountyService.StartBounty(startStage);
                currentBounty = BountyService.GetCurrentBounty();
                success = currentBounty != null;
            }
            else {
                currentBounty = BountyService.GetCurrentBounty();
                success = currentBounty != null;
                if (success) BountyService.StartBounty(currentBounty.Id);
                isClear = false;
            }
#else
            currentBounty = BountyService.GetCurrentBounty();
            success = currentBounty != null;
            if (success) BountyService.StartBounty(currentBounty.Id);
#endif
        }

        protected virtual IEnumerator InstantiateMap() {
            if (MapService.TryGetMap(currentBounty.MapId, out var newCurrentMap)) {
                // 기존에 있는 Map 제거
                if (currentMap != null) {
                    if (currentMap.Id != newCurrentMap.Id) {
                        Destroy(currentMapObj);
                        currentMapController = null;
                    }
                }

                yield return null;

                // 새로 Instantiate할 Map이 이미 존재하면 제거
                var mapControllers = FindObjectsOfType<YisoMapController>(true);
                foreach (var controller in mapControllers) {
                    if (controller.CurrentMap == null) {
                        Destroy(controller.gameObject);
                    }
                    else if (controller.CurrentMap.Id == newCurrentMap.Id) {
                        Destroy(controller.gameObject);
                    }
                }

                yield return null;

                currentMap = newCurrentMap;
                currentMapObj = Instantiate(currentMap.Prefab, mapAttachment);
                currentMapController = currentMapObj.GetComponentInChildren<YisoMapController>();
                currentMapController.transform.localPosition = Vector3.zero;
            }
        }

        protected virtual void InstantiatePlayer() {
            // 이미 Player가 Scene 내에 있는 경우
            if (characterInScene != null) {
                GameManager.Instance.Player = characterInScene;
                return;
            }

            if (playerPrefab != null) {
                var newPlayer = Instantiate(playerPrefab, playerAttachment);
                newPlayer.name = playerPrefab.name;
                GameManager.Instance.Player = newPlayer;
                characterInScene = Player;

                if (playerPrefab.characterType != YisoCharacter.CharacterTypes.Player) {
                    LogService.Fatal("[BountyManager] The Character you've set in the BountyManager isn't a Player, which means it's probably not going to move. You can change that in the Character component of your prefab.");
                }
            }
        }

        protected virtual void SpawnPlayer() {
            YisoInGameEvent.Trigger(YisoInGameEventTypes.SpawnCharacterStarts, Player, currentBounty);
            currentMapController.SpawnPlayer(Player, false);
            Player.Freeze(YisoCharacterStates.FreezePriority.Default);
            YisoInGameEvent.Trigger(YisoInGameEventTypes.SpawnComplete, Player, currentBounty);
        }

        protected virtual void InitializeEnemySpawner() {
            if (currentBounty == null || currentBounty.EnemySpawnerPrefab == null) return;
            var enemySpawnerObj = Instantiate(currentBounty.EnemySpawnerPrefab, Vector3.zero, Quaternion.identity);
            enemySpawner = enemySpawnerObj.GetComponent<YisoCharacterSpawner>();
            enemySpawner.parent = enemyAttachment;
            enemySpawnerObj.transform.SetParent(Player.transform);
            enemySpawnerObj.transform.localPosition = Vector3.zero;
            enemySpawner.Initialization();
        }

        #endregion

        #region Dead

        protected virtual void PlayerDead() {
            if (Player != null) {
                StartCoroutine(PlayerDeadCo());
            }
        }

        protected virtual IEnumerator PlayerDeadCo() {
            yield return new WaitForSeconds(delayBeforeDeathScreen);
            if (isClear) {
                PopupUIService.AlertS(deathPopupUITitle[CurrentLocale],
                    deathPopupUIContent[CurrentLocale]
                    , Restart
                    , DrawBounty);
            }
            else if (deathCount >= maxDeathCount) {
                PopupUIService.AlertS(missionFailedPopupUITitle[CurrentLocale],
                    missionFailedPopupUIContent[CurrentLocale]
                    , DrawBounty
                    , DrawBounty);
            }
            else {
                var lastChanceSuffixMessage = CurrentLocale == YisoLocale.Locale.KR
                    ? $"(남은 기회: {maxDeathCount - deathCount}/{maxDeathCount})"
                    : $"(Last Chance: {maxDeathCount - deathCount}/{maxDeathCount})";
                PopupUIService.AlertS(deathPopupUITitle[CurrentLocale],
                    $"{deathPopupUIContent[CurrentLocale]} {lastChanceSuffixMessage}"
                    , Restart
                    , DrawBounty);
            }
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
            YisoInGameEvent.Trigger(YisoInGameEventTypes.RespawnComplete, Player, currentBounty);
        }

        #endregion

        #region Clear

        public void CompleteBounty() {
            StopTimer();
            
            var success = BountyService.CompleteBounty(out var reason);
            
            PopupUIService.ShowBountyClearPopup();

            var obstacleLayerMask = LayerManager.EnemiesLayerMask | LayerManager.MapLayerMask | LayerManager.PlayerLayerMask | LayerManager.PortalLayerMask | LayerManager.NpcLayerMask;
            
            var npcStorePosition = YisoPhysicsUtils.FindValidPositionInCircle(Player.transform.position, 4f, obstacleLayerMask);
            npcStore.transform.position = npcStorePosition;
            npcStore.SetActive(true);
            
            var portalPosition = YisoPhysicsUtils.FindValidPositionInCircle(Player.transform.position, 4f, obstacleLayerMask);
            portal.transform.position = portalPosition;
            portal.SetActive(true);
        }

        public virtual void DrawBounty() {
            StopTimer();
            BountyService.DrawBounty();
            GameManager.Instance.LoadBaseCampScene();
        }

        public void ShowBountyClearPopup() {
            PopupUIService.AlertS(baseCampPopupUITitle[CurrentLocale], baseCampPopupUIContent[CurrentLocale]
                , DrawBounty
                , () => { });
        }

        protected virtual void CheckBountyClear(YisoBountyEventArgs eventArgs) {
            if (!GameManager.HasInstance || GameManager.Instance.CurrentGameMode != GameManager.GameMode.Bounty) return;
            switch (eventArgs) {
                case YisoBountyStatusChangeEventArgs statusChangeEventArgs:
                    if (statusChangeEventArgs.To == YisoBountyStatus.COMPLETE) {
                        isClear = true;
                    }
                    break;
            }
        }

        #endregion

        #region Timer

        private bool CanUseTimer() {
            return TimeManager.HasInstance && currentBounty != null && currentBounty.UseTimeLimit;
        }

        private void StartTimer(float time) {
            if (!CanUseTimer()) return;
            TimeManager.Instance.StartTimer(time, true);
        }

        public void StopTimer() {
            if (!CanUseTimer()) return;
            TimeManager.Instance.StopTimer();
        }

        #endregion

        #region Boss

        public virtual void RegisterBoss(GameObject bossObj) {
            boss = bossObj;
            NavigateToBoss();
        }

        protected virtual void NavigateToBoss() {
            var questNavigator = Player.FindAbility<YisoCharacterNavigator>();
            if (questNavigator != null) {
                questNavigator.StartNavigation(boss, bossAppearanceUI);
            }
        }

        #endregion

        protected virtual IEnumerator Fade(bool flag, float duration) {
            yield return GameManager.Instance.Fade(flag, duration).WaitForCompletion();
        }

        public void OnEvent(YisoInGameEvent inGameEvent) {
            if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.Bounty) return;
            switch (inGameEvent.eventType) {
                case YisoInGameEventTypes.PlayerDeath:
                    deathCount++;
                    PlayerDead();
                    break;
                case YisoInGameEventTypes.RespawnStarted:
                    Restart();
                    break;
            }
        }

        protected virtual void OnEnable() {
            this.YisoEventStartListening();
            BountyService.RegisterOnBountyEvent(CheckBountyClear);
        }

        protected virtual void OnDisable() {
            this.YisoEventStopListening();
            BountyService.UnregisterOnBountyEvent(CheckBountyClear);
        }
    }
}