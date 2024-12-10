using System.Collections;
using System.Linq;
using Character.Core;
using Controller.Map;
using Core.Behaviour;
using Core.Domain.Types;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Core.Service.Scene;
using Core.Service.Temp;
using Core.Service.UI;
using Core.Service.UI.Global;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Manager.Modules;
using Sirenix.OdinInspector;
using Tools.Event;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Manager {
    public class GameManager : RunISingleton<GameManager>, IYisoEventListener<YisoGameEvent>,
        IYisoEventListener<YisoInGameEvent> {
        public enum GameMode {
            None,
            Bounty,
            Story
        }

        [SerializeField] private YisoGameModules gameModules;

        protected SceneInstance currentSceneInstance;

        public YisoGameModules GameModules => gameModules;
        public IYisoSceneService SceneService => YisoServiceProvider.Instance.Get<IYisoSceneService>();
        public IYisoUIService UIService => YisoServiceProvider.Instance.Get<IYisoUIService>();
        public IYisoGlobalUIService GlobalUIService => YisoServiceProvider.Instance.Get<IYisoGlobalUIService>();
        private YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<GameManager>();
        public YisoCharacter Player { get; set; }
        public bool Paused { get; set; }
        public GameMode CurrentGameMode { get; set; } = GameMode.None;

        public YisoMapController CurrentMapController {
            get {
                return CurrentGameMode switch {
                    GameMode.None => null,
                    GameMode.Bounty => !BountyManager.HasInstance ? null : BountyManager.Instance.CurrentMapController,
                    GameMode.Story => !StageManager.HasInstance ? null : StageManager.Instance.CurrentMapController,
                    _ => null
                };
            }
        }

        public int CurrentStageId {
            get {
                switch (CurrentGameMode) {
                    case GameMode.None:
                        return -1;
                    case GameMode.Bounty:
                        if (!BountyManager.HasInstance) return -1;
                        if (BountyManager.Instance.CurrentBounty == null) return -1;
                        return BountyManager.Instance.CurrentBounty.Id;
                    case GameMode.Story:
                        if (!StageManager.HasInstance) return -1;
                        if (StageManager.Instance.CurrentStage == null) return -1;
                        return StageManager.Instance.CurrentStage.Id;
                    default:
                        return -1;
                }
            }
        }

        public int DeathCount {
            get {
                switch (CurrentGameMode) {
                    case GameMode.None:
                        return 0;
                    case GameMode.Bounty:
                        if (!BountyManager.HasInstance) return 0;
                        return BountyManager.Instance.DeathCount;
                    case GameMode.Story:
                        if (!StageManager.HasInstance) return 0;
                        return StageManager.Instance.DeathCount;
                    default:
                        return 0;
                }
            }
        }

        public bool IsUIShown {
            get => isUIShown;
            set {
                if (isUIShown == value) return;
                isUIShown = value;
                if (isUIShown) {
                    FreezeCharacter(0f, YisoCharacterStates.FreezePriority.UIPopup);
                    if (TimeManager.HasInstance) TimeManager.Instance.PauseTimer();
                }
                else {
                    UnFreezeCharacter(0f, YisoCharacterStates.FreezePriority.UIPopup);
                    if (TimeManager.HasInstance) TimeManager.Instance.ResumeTimer();
                }
            }
        }

        private bool isUIShown = false;

        #region Cycle

        protected override void Awake() {
            base.Awake();
            if (instance != this) return;
            gameModules.InitializeModules(this);
            YisoServiceProvider.Instance.Get<IYisoTempService>().SetGameManager(this);
        }

        protected override void OnEnable() {
            if (instance != this) return;
            base.OnEnable();
            foreach (var module in gameModules.Modules) {
                module.OnEnabled();
            }
        }

        public override void OnUpdate() {
            if (instance != this) return;
            base.OnUpdate();
            foreach (var module in gameModules.Modules) {
                module.OnUpdate();
            }
            CheckUIShowed();
        }

        protected override void OnDisable() {
            if (instance != this) return;
            base.OnDisable();
            foreach (var module in gameModules.Modules) {
                module.OnDisabled();
            }
        }

        protected virtual void OnDestroy() {
            if (instance != this) return;
            foreach (var module in gameModules.Modules) {
                module.OnDestroy();
            }
        }

        #endregion

        #region Player Utils

        protected virtual void CheckUIShowed() {
            if (!UIService.IsReady()) return;
            IsUIShown = UIService.IsUIShowed();
        }

        public virtual void FreezeCharacter(float delay = 0f, YisoCharacterStates.FreezePriority priority = YisoCharacterStates.FreezePriority.Default, bool includePets = true) {
            if (Player == null) return;
            if (delay > 0) {
                StartCoroutine(FreezeCharacterCo(delay, true, priority));
            }
            else {
                Player.Freeze(priority);
            }

            if (includePets) {
                var pets = GameModules.PetModule.GetPets();
                if (pets != null && pets.Count > 0) {
                    foreach (var pet in pets.Where(pet =>
                        pet.conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Dead)) {
                        pet.Freeze(priority);
                    }
                }
            }
        }

        public virtual void UnFreezeCharacter(float delay, YisoCharacterStates.FreezePriority priority, bool includePets = true) {
            if (Player == null) return;
            if (delay > 0) {
                StartCoroutine(FreezeCharacterCo(delay, false, priority));
            }
            else {
                Player.UnFreeze(priority);
            }

            if (includePets) {
                var pets = GameModules.PetModule.GetPets();
                if (pets != null && pets.Count > 0) {
                    foreach (var pet in pets.Where(pet =>
                        pet.conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Dead)) {
                        pet.UnFreeze(priority);
                    }
                }
            }
            
            // Reset All Movement
            if(InputManager.HasInstance) InputManager.Instance.ResetAllMovement();
        }

        protected virtual IEnumerator FreezeCharacterCo(float delay, bool freeze, YisoCharacterStates.FreezePriority priority, bool includePets = true) {
            yield return new WaitForSeconds(delay);
            if (freeze) {
                Player.Freeze(priority);
            }
            else {
                Player.UnFreeze(priority);
            }

            if (includePets) {
                var pets = GameModules.PetModule.GetPets();
                if (pets != null && pets.Count > 0) {
                    foreach (var pet in pets.Where(pet =>
                        pet.conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Dead)) {
                        if (freeze) {
                            pet.Freeze(priority);
                        }
                        else {
                            pet.UnFreeze(priority);
                        }
                    }
                }
            }
        }

        public virtual void ShowPlayer(bool includePets = true) {
            if (Player == null) return;
            Player.SetCharacterVisible(true);
            if (includePets) {
                var pets = GameModules.PetModule.GetPets();
                if (pets != null && pets.Count > 0) {
                    foreach (var pet in pets.Where(pet =>
                        pet.conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Dead)) {
                        pet.SetCharacterVisible(true);
                    }
                }
            }
        }

        public virtual void HidePlayer(bool includePets = true) {
            if (Player == null) return;
            Player.SetCharacterVisible(false);
            if (includePets) {
                var pets = GameModules.PetModule.GetPets();
                if (pets != null && pets.Count > 0) {
                    foreach (var pet in pets.Where(pet =>
                        pet.conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Dead)) {
                        pet.SetCharacterVisible(false);
                    }
                }
            }
        }

        public virtual void SetPlayerPosition(Vector2 newPosition) {
            if (Player == null) return;
            Player.transform.position = newPosition;
        }

        public virtual void SetPlayerPosition(Vector2 newPosition, YisoCharacter.FacingDirections direction) {
            if (Player == null) return;
            Player.transform.position = newPosition;
            Player.Orientation2D.Face(direction);
        }

        #endregion

        #region Time

        [Button]
        public virtual void Pause() {
            YisoTimeScaleEvent.Trigger(TimeScaleMethods.Change, 0f, 0f, false, 0f, true);
        }

        [Button]
        public virtual void UnPause() {
            YisoTimeScaleEvent.Trigger(TimeScaleMethods.Reset, 0f, 0f, false, 0f, false);
        }

        [Button]
        public virtual void ChangeTimeScale(float newTimeScale) {
            YisoTimeScaleEvent.Trigger(TimeScaleMethods.Change, newTimeScale, 3f, true, 1f, false);
        }

        #endregion

        #region Load Scene

        public virtual void LoadInitScene() {
            SceneService.LoadScene(YisoSceneTypes.INIT);
        }

        public virtual void LoadBaseCampScene() {
            SceneService.LoadScene(YisoSceneTypes.BASE_CAMP);
        }

        public virtual IEnumerator LoadCutsceneAdditive(string sceneAddress, UnityAction<Scene> onSceneLoaded = null,
            UnityAction onSceneLoadFailed = null) {
            var handle = Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Additive);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                currentSceneInstance = handle.Result;
                onSceneLoaded?.Invoke(currentSceneInstance.Scene);
            }
            else {
                onSceneLoadFailed?.Invoke();
            }
        }

        public virtual IEnumerator UnloadCutsceneAdditive(UnityAction onSceneUnloaded = null,
            UnityAction onSceneUnloadFailed = null) {
            if (!currentSceneInstance.Scene.isLoaded) {
                LogService.Warn($"[GameManager] {currentSceneInstance.Scene.name} is not loaded.");
                yield break;
            }

            var handle = Addressables.UnloadSceneAsync(currentSceneInstance);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                onSceneUnloaded?.Invoke();
            }
            else {
                LogService.Fatal($"[GameManager] {currentSceneInstance.Scene.name} unload failed.");
                onSceneUnloadFailed?.Invoke();
            }
        }

        #endregion

        #region Fade

        public TweenerCore<float, float, FloatOptions> Fade(bool flag, float duration) {
            return GlobalUIService.DOFade(flag, duration);
        }

        #endregion

        public void OnEvent(YisoGameEvent e) {
        }

        public void OnEvent(YisoInGameEvent e) {
        }
    }
}