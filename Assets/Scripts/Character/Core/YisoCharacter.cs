using System;
using System.Collections;
using System.Collections.Generic;
using Character.Ability;
using Character.Health;
using Core.Behaviour;
using Manager_Temp_;
using Sirenix.OdinInspector;
using Tools.AI;
using Tools.Event;
using Tools.StateMachine;
using UnityEngine;
using Utils.Beagle;

namespace Character.Core {
    [SelectionBase]
    [AddComponentMenu("Yiso/Character/Core/Character")]
    public class YisoCharacter : RunIBehaviour {
        public enum FacingDirections {
            West,
            East,
            North,
            South
        }

        public enum CharacterTypes {
            Player,
            AI
        }

        #region Inspector

        public CharacterTypes characterType = CharacterTypes.AI;
        public string playerID = "";
        public Animator characterAnimator;
        public GameObject characterModel;
        public YisoHealth characterHealth;
        public YisoAIBrain characterBrain;

        public List<GameObject>
            additionalAbilityNodes; // character ability를 character object에 몽땅 몰아넣지 않고 하위 옵젝에 분리하고 싶으면 여기에 등록해줘야함

        public bool activateOnInstantiate = false; // 바로 Activate시킬건지
        public bool playSpawnAnimation = true; // Spawn될때 Animation 재생할건지
        public bool runAnimatorSanityChecks = false; // 애니메이터 Parameter 있는지 체크할건지 말건지

#if UNITY_EDITOR
        [Title("Debug")] [ReadOnly] public YisoCharacterStates.MovementStates currentMovementState;
        [ReadOnly] public YisoCharacterStates.CharacterConditions currentConditionState;
#endif

        #endregion

        public YisoStateMachine<YisoCharacterStates.MovementStates> movementState;
        public YisoStateMachine<YisoCharacterStates.CharacterConditions> conditionState;

        public InputManager LinkedInputManager { get; protected set; }
        public Animator Animator { get; protected set; }
        public HashSet<int> AnimatorParameters { get; protected set; }
        public YisoCharacterOrientation2D Orientation2D { get; protected set; }
        public YisoCharacterMovement CharacterMovement { get; protected set; }
        public YisoCharacterStat CharacterStat { get; protected set; }
        public GameObject CameraTarget { get; set; }
        public Vector3 CameraDirection { get; protected set; }

        protected YisoCharacterAbility[] characterAbilities;
        protected bool abilitiesCachedOnce = false;

        protected TopDownController controller;

        protected bool animatorInitialized = false;

        protected Coroutine conditionChangeCoroutine;
        protected YisoCharacterStates.CharacterConditions lastState;

        protected float freezeDurationOnSpawn = 3f;
        protected FreezePriorityHandler freezePriorityHandler;

        protected const string SpawnAnimationParameterName = "IsSpawn";
        protected const string DeathAnimationParameterName = "IsDeath";

        protected int spawnAnimationParameter;
        protected int deathAnimationParameter;

        protected override void Awake() {
            Initialization();
        }

        public override void OnUpdate() {
            EveryFrame();
        }

        public virtual void Disable() {
            enabled = false;
            controller.enabled = false;
        }

        protected virtual void Initialization() {
            // State Machine 설정
            movementState = new YisoStateMachine<YisoCharacterStates.MovementStates>(gameObject, true);
            conditionState = new YisoStateMachine<YisoCharacterStates.CharacterConditions>(gameObject, true);
            freezePriorityHandler = new FreezePriorityHandler();

#if UNITY_EDITOR
            currentMovementState = movementState.CurrentState;
            currentConditionState = conditionState.CurrentState;

            movementState.OnStateChange += () => { currentMovementState = movementState.CurrentState; };
            conditionState.OnStateChange += () => { currentConditionState = conditionState.CurrentState; };
#endif

            // Animator 설정
            AssignAnimator();

            // Top Down Controller 설정
            controller = gameObject.GetComponent<TopDownController>();

            // Play Spawn Animation
            SpawnCharacter(false);

            // Input Manager 설정
            SetInputManager();

            // Health 설정
            if (characterHealth == null) characterHealth = gameObject.GetComponent<YisoHealth>();

            // Character Ability Init
            CacheAbilitiesAtInit();

            // Character Brain
            if (characterBrain == null) characterBrain = gameObject.GetComponentInChildren<YisoAIBrain>();
            if (characterBrain != null) characterBrain.owner = this;

            // Character Ability 캐싱
            StoreCharacterAbilities();

            // Camera Target Instantiate
            if (CameraTarget == null) CameraTarget = new GameObject($"[{playerID}] Camera Target");
            CameraTarget.transform.SetParent(transform);
            CameraTarget.transform.localPosition = Vector3.zero;
            CameraTarget.name = "CameraTarget";

            // TODO (ConeOfVision2D)
        }

        protected virtual void EveryFrame() {
            PreProcessAbilities();
            ProcessAbilities();
            PostProcessAbilities();
            UpdateAnimators();
        }

        #region InputManager

        public virtual void SetInputManager() {
            if (characterType == CharacterTypes.AI) {
                LinkedInputManager = null;
                UpdateInputManagersInAbilities();
                return;
            }

            if (!string.IsNullOrEmpty(playerID)) {
                LinkedInputManager = null;
                var foundInputManagers = FindObjectsOfType(typeof(InputManager)) as InputManager[];
                foreach (var foundInputManager in foundInputManagers) {
                    if (foundInputManager.playerID == playerID) {
                        LinkedInputManager = foundInputManager;
                    }
                }
            }

            UpdateInputManagersInAbilities();
        }

        /// <summary>
        /// Input Manager를 Character Ability에 모두 등록시켜줌
        /// </summary>
        public virtual void UpdateInputManagersInAbilities() {
            if (characterAbilities == null) return;
            foreach (var ability in characterAbilities) {
                ability.SetInputManager(LinkedInputManager);
            }
        }

        public virtual void ResetInput() {
            if (characterAbilities == null) return;
            foreach (var ability in characterAbilities) {
                ability.ResetInput();
            }
        }

        #endregion

        #region Ability

        protected virtual void StoreCharacterAbilities() {
            Orientation2D = FindAbility<YisoCharacterOrientation2D>();
            CharacterStat = FindAbility<YisoCharacterStat>();
            CharacterMovement = FindAbility<YisoCharacterMovement>();
        }

        protected virtual void CacheAbilitiesAtInit() {
            if (abilitiesCachedOnce) return;
            CacheAbilities();
        }

        public virtual void CacheAbilities() {
            characterAbilities = gameObject.GetComponents<YisoCharacterAbility>();
            if (additionalAbilityNodes != null && additionalAbilityNodes.Count > 0) {
                var tempAbilityList = new List<YisoCharacterAbility>();
                foreach (var ability in characterAbilities) {
                    tempAbilityList.Add(ability);
                }

                foreach (var additionalAbilityNode in additionalAbilityNodes) {
                    var tempArray = additionalAbilityNode.GetComponentsInChildren<YisoCharacterAbility>();
                    foreach (var ability in tempArray) {
                        tempAbilityList.Add(ability);
                    }
                }

                characterAbilities = tempAbilityList.ToArray();
            }

            abilitiesCachedOnce = true;
        }

        public T FindAbility<T>() where T : YisoCharacterAbility {
            CacheAbilitiesAtInit();
            foreach (var ability in characterAbilities) {
                if (ability is T characterAbility) {
                    return characterAbility;
                }
            }

            return null;
        }

        public List<T> FindAbilities<T>() where T : YisoCharacterAbility {
            CacheAbilitiesAtInit();

            var resultList = new List<T>();
            foreach (var ability in characterAbilities) {
                if (ability is T characterAbility) {
                    resultList.Add(characterAbility);
                }
            }

            return resultList;
        }

        protected virtual void PreProcessAbilities() {
            foreach (var ability in characterAbilities) {
                if (ability.enabled && ability.AbilityInitialized) {
                    ability.PreProcessAbility();
                }
            }
        }

        protected virtual void ProcessAbilities() {
            foreach (var ability in characterAbilities) {
                if (ability.enabled && ability.AbilityInitialized) {
                    ability.ProcessAbility();
                }
            }
        }

        protected virtual void PostProcessAbilities() {
            foreach (var ability in characterAbilities) {
                if (ability.enabled && ability.AbilityInitialized) {
                    ability.PostProcessAbility();
                }
            }
        }

        /// <summary>
        /// Character 죽었을 때 call
        /// </summary>
        public virtual void ResetAbilities() {
            if (characterAbilities == null) return;
            if (characterAbilities.Length == 0) return;
            foreach (var ability in characterAbilities) {
                if (ability.enabled) ability.ResetAbility();
            }
        }

        #endregion

        #region Animator

        public virtual void AssignAnimator() {
            if (animatorInitialized) return;

            AnimatorParameters = new HashSet<int>();
            Animator = characterAnimator != null ? characterAnimator : gameObject.GetComponent<Animator>();

            if (Animator != null) InitializeAnimatorParameters();
            animatorInitialized = true;
        }

        public virtual void ChangeAnimatorController(RuntimeAnimatorController animatorController) {
            if (Animator == null) return;
            Animator.runtimeAnimatorController = animatorController;
        }

        protected virtual void InitializeAnimatorParameters() {
            if (Animator == null) return;
            YisoAnimatorUtils.AddAnimatorParameterIfExists(Animator, SpawnAnimationParameterName,
                out spawnAnimationParameter, AnimatorControllerParameterType.Trigger, AnimatorParameters);
            YisoAnimatorUtils.AddAnimatorParameterIfExists(Animator, DeathAnimationParameterName,
                out deathAnimationParameter, AnimatorControllerParameterType.Bool, AnimatorParameters);
        }

        /// <summary>
        /// 매 프레임마다 Character에 있는 Animator Parameter랑 Character Ability에 있는 Animator Parameter를 일괄적으로 Update함
        /// </summary>
        protected virtual void UpdateAnimators() {
            if (Animator == null) return;

            // 여기다 Animator param 업데이트 시키면 됨

            foreach (var ability in characterAbilities) {
                if (ability.enabled && ability.AbilityInitialized) {
                    ability.UpdateAnimator();
                }
            }
        }

        protected virtual void SpawnCharacter(bool respawn) {
            var nextState = GetNextConditionAfterRespawn(respawn);
            if (playSpawnAnimation) {
                if (!animatorInitialized) AssignAnimator();
                YisoAnimatorUtils.UpdateAnimatorTrigger(Animator, spawnAnimationParameter, AnimatorParameters,
                    runAnimatorSanityChecks);

                ChangeCharacterConditionTemporarily(YisoCharacterStates.CharacterConditions.Frozen, nextState,
                    freezeDurationOnSpawn, true, YisoCharacterStates.FreezePriority.Respawn);
            }
            else {
                if (nextState == YisoCharacterStates.CharacterConditions.Frozen) {
                    Freeze(YisoCharacterStates.FreezePriority.Respawn);
                }

                if (nextState == YisoCharacterStates.CharacterConditions.Normal) {
                    UnFreeze(YisoCharacterStates.FreezePriority.Respawn);
                }
            }
        }

        protected virtual YisoCharacterStates.CharacterConditions GetNextConditionAfterRespawn(bool respawn) {
            YisoCharacterStates.CharacterConditions nextState;
            if (respawn) {
                nextState = YisoCharacterStates.CharacterConditions.Normal;
            }
            else {
                nextState = activateOnInstantiate
                    ? YisoCharacterStates.CharacterConditions.Normal
                    : YisoCharacterStates.CharacterConditions.Frozen;
            }

            return nextState;
        }

        #endregion

        #region Respawn

        public virtual void RespawnAt(FacingDirections facingDirection, bool isRespawn) {
            RespawnAt(transform.position, facingDirection, isRespawn);
        }

        public virtual void RespawnAt(Transform spawnPoint, FacingDirections facingDirection, bool isRespawn) {
            RespawnAt(spawnPoint.position, facingDirection, isRespawn);
        }

        public virtual void RespawnAt(Vector3 spawnPosition, FacingDirections facingDirections, bool isRespawn) {
            transform.position = spawnPosition;
            if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

            if (gameObject.YisoGetComponentNoAlloc<Collider2D>() != null)
                gameObject.YisoGetComponentNoAlloc<Collider2D>().enabled = true;

            controller.enabled = true;
            controller.Reset();

            ResetInput();
            ResetAbilities();

            SpawnCharacter(isRespawn);

            if (characterBrain != null) {
                characterBrain.enabled = true;
                characterBrain.ResetBrain();
            }

            if (characterHealth != null) {
                characterHealth.StoreInitialPosition();
                characterHealth.ResetHealthToMaxHealth();
                characterHealth.Revive();
            }

            // Set Animator
            if (Animator != null) {
                YisoAnimatorUtils.UpdateAnimatorBool(Animator, deathAnimationParameter, false, AnimatorParameters,
                    runAnimatorSanityChecks);
            }

            if (Orientation2D == null) StoreCharacterAbilities();
            if (Orientation2D != null) {
                Orientation2D.initialFacingDirection = facingDirections;
                Orientation2D.Face(facingDirections);
            }
        }

        #endregion

        #region Setter

        public virtual void SetPlayerID(string newPlayerID) {
            playerID = newPlayerID;
            SetInputManager();
        }

        public virtual void SetCameraDirection(Vector3 direction) {
            CameraDirection = direction;
        }

        #endregion

        #region Freeze

        public virtual void Freeze(YisoCharacterStates.FreezePriority priority) {
            freezePriorityHandler.SetFreezeState(priority, true);
            if (!freezePriorityHandler.IsFrozen) return;

            // Reset Movement   
            LinkedInputManager?.ResetAllMovement();
            if (CharacterMovement == null) StoreCharacterAbilities();
            CharacterMovement?.SetMovement(Vector2.zero);

            movementState.ChangeState(YisoCharacterStates.MovementStates.Idle);
            conditionState.ChangeState(YisoCharacterStates.CharacterConditions.Frozen);
        }

        public virtual void UnFreeze(YisoCharacterStates.FreezePriority priority) {
            freezePriorityHandler.SetFreezeState(priority, false);
            if (freezePriorityHandler.IsFrozen) return;
            
            // Reset Movement
            LinkedInputManager?.ResetAllMovement();
            if (CharacterMovement == null) StoreCharacterAbilities();
            CharacterMovement?.SetMovement(Vector2.zero);

            movementState.ChangeState(YisoCharacterStates.MovementStates.Idle);
            conditionState.ChangeState(YisoCharacterStates.CharacterConditions.Normal);
        }

        #endregion

        #region Condition

        protected virtual void ChangeCharacterCondition(YisoCharacterStates.CharacterConditions newCondition,
            YisoCharacterStates.FreezePriority? freezePriority = null) {
            // ? => Frozen
            if (newCondition == YisoCharacterStates.CharacterConditions.Frozen && freezePriority.HasValue) {
                Freeze(freezePriority.Value);
                return;
            }

            // Frozen => Normal
            if (conditionState.CurrentState == YisoCharacterStates.CharacterConditions.Frozen &&
                newCondition == YisoCharacterStates.CharacterConditions.Normal && freezePriority.HasValue) {
                UnFreeze(freezePriority.Value);
                return;
            }

            // 그 외
            if (newCondition == YisoCharacterStates.CharacterConditions.Dead) {
                conditionState.ChangeState(newCondition);
                freezePriorityHandler.Reset();
            }
        }

        public virtual void ChangeCharacterConditionTemporarily(YisoCharacterStates.CharacterConditions newCondition,
            float duration, bool resetControllerForces,
            YisoCharacterStates.FreezePriority freezePriority = YisoCharacterStates.FreezePriority.Default) {
            if (conditionChangeCoroutine != null) StopCoroutine(conditionChangeCoroutine);
            conditionChangeCoroutine =
                StartCoroutine(ChangeCharacterConditionTemporarilyCo(newCondition, duration, resetControllerForces,
                    freezePriority));
        }

        public virtual void ChangeCharacterConditionTemporarily(YisoCharacterStates.CharacterConditions temporaryState,
            YisoCharacterStates.CharacterConditions nextState, float duration, bool resetControllerForces,
            YisoCharacterStates.FreezePriority freezePriority = YisoCharacterStates.FreezePriority.Default) {
            if (conditionChangeCoroutine != null) StopCoroutine(conditionChangeCoroutine);
            conditionChangeCoroutine =
                StartCoroutine(ChangeCharacterConditionTemporarilyCo(temporaryState, nextState, duration,
                    resetControllerForces, freezePriority));
        }

        protected virtual IEnumerator ChangeCharacterConditionTemporarilyCo(
            YisoCharacterStates.CharacterConditions newCondition, float duration, bool resetMovementForces,
            YisoCharacterStates.FreezePriority freezePriority = YisoCharacterStates.FreezePriority.Default) {
            if (lastState != newCondition || conditionState.CurrentState != newCondition) {
                lastState = conditionState.CurrentState;
            }

            ChangeCharacterCondition(newCondition, freezePriority);
            if (resetMovementForces) controller.SetMovement(Vector2.zero);
            yield return new WaitForSeconds(duration);
            ChangeCharacterCondition(lastState, freezePriority);
        }

        protected virtual IEnumerator ChangeCharacterConditionTemporarilyCo(
            YisoCharacterStates.CharacterConditions temporaryState, YisoCharacterStates.CharacterConditions nextState,
            float duration, bool resetMovementForces,
            YisoCharacterStates.FreezePriority freezePriority = YisoCharacterStates.FreezePriority.Default) {
            ChangeCharacterCondition(temporaryState, freezePriority);
            if (resetMovementForces) controller.SetMovement(Vector2.zero);
            yield return new WaitForSeconds(duration);
            ChangeCharacterCondition(nextState, freezePriority);
        }

        #endregion

        #region Health

        protected virtual void OnDeath() {
            ResetAbilities();

            if (characterType == CharacterTypes.Player) {
                YisoInGameEvent.Trigger(YisoInGameEventTypes.PlayerDeath, this, GameManager.Instance.CurrentStageId);
            }

            if (characterBrain != null) {
                characterBrain.TransitionToState("");
                characterBrain.enabled = false;
            }

            // Set Animator
            if (Animator != null) {
                YisoAnimatorUtils.UpdateAnimatorBool(Animator, deathAnimationParameter, true, AnimatorParameters,
                    runAnimatorSanityChecks);
            }

            // Disable controller
            controller.Reset();
            controller.enabled = false;

            movementState.ChangeState(YisoCharacterStates.MovementStates.Idle);
            ChangeCharacterCondition(YisoCharacterStates.CharacterConditions.Dead);
        }

        protected virtual void OnHit(GameObject attacker) {
        }

        protected override void OnEnable() {
            base.OnEnable();
            if (characterHealth != null) {
                characterHealth.onDeath += OnDeath;
                characterHealth.onHit += OnHit;
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (characterHealth != null) {
                characterHealth.onDeath -= OnDeath;
                characterHealth.onHit -= OnHit;
            }
        }

        #endregion

        #region Visible

        public virtual void SetCharacterVisible(bool isVisible) {
            if (characterBrain != null) {
                characterBrain.enabled = isVisible;
            }

            if (characterModel.TryGetComponent(out SpriteRenderer renderer)) {
                renderer.enabled = isVisible;
            }

            var handleWeapon = FindAbility<YisoCharacterHandleWeapon>();
            if (handleWeapon != null) {
                if (handleWeapon.currentWeapon != null) {
                    handleWeapon.CurrentWeaponModel?.SetActive(isVisible);
                }
            }

            if (isVisible) {
                controller.CollisionsOn();
            }
            else {
                controller.CollisionsOff();
            }
        }

        #endregion

        #region Freeze Priority Handler (Nested Class)

        protected sealed class FreezePriorityHandler {
            private int currentFreezePriorityMask;

            public FreezePriorityHandler() {
                currentFreezePriorityMask = 0;
            }

            /// <summary>
            /// 캐릭터가 Freeze 상태인지 여부를 확인
            /// </summary>
            public bool IsFrozen => currentFreezePriorityMask != 0;

            /// <summary>
            /// 현재 활성화된 가장 낮은 우선순위의 Freeze Priority 반환
            /// </summary>
            public YisoCharacterStates.FreezePriority CurrentFreezePriority {
                get {
                    // 비트 마스크에서 활성화된 가장 낮은 우선순위 찾기
                    for (var i = 0; i <= (int) YisoCharacterStates.FreezePriority.Default; i++) {
                        if ((currentFreezePriorityMask & (1 << i)) != 0) {
                            return (YisoCharacterStates.FreezePriority) i;
                        }
                    }

                    return YisoCharacterStates.FreezePriority.Default; // 모두 false일 경우
                }
            }

            /// <summary>
            /// 특정 우선순위보다 낮거나 같은 우선순위의 freeze 상태가 있는지 확인
            /// </summary>
            /// <param name="priority"></param>
            /// <returns></returns>
            public bool CheckLowerPriorityIsFrozen(YisoCharacterStates.FreezePriority priority) {
                var maskToCheck = (1 << (int) priority) - 1; // 우선순위 이하의 비트 마스크
                return (currentFreezePriorityMask & maskToCheck) != 0; // 해당 비트가 설정되어 있으면 true
            }

            /// <summary>
            /// 특정 우선순위의 freeze 상태를 설정하는 메서드
            /// </summary>
            /// <param name="priority"></param>
            /// <param name="isFrozen"></param>
            public void SetFreezeState(YisoCharacterStates.FreezePriority priority, bool isFrozen) {
                if (isFrozen) {
                    currentFreezePriorityMask |= (int) priority; // 해당 비트 설정
                }
                else {
                    currentFreezePriorityMask &= ~(int) priority; // 해당 비트 해제
                }
            }

            public void Reset() {
                currentFreezePriorityMask = 0;
            }

            public override string ToString() {
                var result = "";

                foreach (YisoCharacterStates.FreezePriority value in Enum.GetValues(
                    typeof(YisoCharacterStates.FreezePriority))) {
                    var isBitSet = (currentFreezePriorityMask & (int) value) != 0;
                    result += $"{value}: {(isBitSet ? "true" : "false")}\n";
                }

                return result.TrimEnd('\n');
            }
        }

        #endregion
    }
}