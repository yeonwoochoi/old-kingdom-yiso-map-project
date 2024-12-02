using System.Collections.Generic;
using System.Linq;
using Character.Core;
using Core.Behaviour;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tools.AI {
    [AddComponentMenu("Yiso/Character/AI/AIBrain")]
    public class YisoAIBrain : RunIBehaviour {
        public enum AITargetType {
            Main,
            Sub,
            SpawnPosition
        }

        [Title("Basic")] public List<YisoAIState> states;

        [ReadOnly] public YisoCharacter owner;
        [ReadOnly] public string currentState;
        [ReadOnly] public float timeInThisState;
        [ReadOnly] public Transform mainTarget;
        [ReadOnly] public Transform subTarget;
        [ReadOnly] public Transform spawnPositionTarget;
        [ReadOnly] public Vector3 lastKnownTargetPosition = Vector3.zero;

        [Title("State")] public bool brainActive = true;
        public bool resetBrainOnStart = true;
        public bool resetBrainOnEnable = false;

        [Title("Frequency")] public float actionDelay = 0f; // action 쿨타임이라고 생각하면 됨 (다음 action이 일어나는데까지 시간)
        public float decisionDelay = 0f; // decision 쿨타임이라고 생각하면 됨 (다음 decision 일어나는데까지 시간)
        public bool randomizeFrequencies = false;
        [ShowIf("randomizeFrequencies")] public Vector2 randomActionFrequency = new(0.5f, 1f);
        [ShowIf("randomizeFrequencies")] public Vector2 randomDecisionFrequency = new(0.5f, 1f);

        public YisoAIState CurrentState { get; protected set; }

        protected YisoAIDecision[] decisions;
        protected YisoAIAction[] actions;
        protected float lastActionsUpdate = 0f;
        protected float lastDecisionsUpdate = 0f;

        #region Getter

        public virtual YisoAIAction[] GetAttachedActions() {
            var attachedActions = this.gameObject.GetComponentsInChildren<YisoAIAction>();
            return attachedActions;
        }

        public virtual YisoAIDecision[] GetAttachedDecisions() {
            var attachedDecisions = this.gameObject.GetComponentsInChildren<YisoAIDecision>();
            return attachedDecisions;
        }

        #endregion

        #region Initialization

        protected override void Awake() {
            foreach (var state in states) {
                state.SetBrain(this);
            }

            decisions = GetAttachedDecisions();
            actions = GetAttachedActions();
            if (randomizeFrequencies) {
                actionDelay = Random.Range(randomActionFrequency.x, randomActionFrequency.y);
                decisionDelay = Random.Range(randomDecisionFrequency.x, randomDecisionFrequency.y);
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            if (resetBrainOnEnable) {
                ResetBrain();
            }
        }

        protected override void Start() {
            if (resetBrainOnStart) {
                ResetBrain();
            }
        }

        public virtual void ResetBrain() {
            InitializeDecisions();
            InitializeActions();
            InitializeSpawnPosition();
            brainActive = true;
            enabled = true;
            mainTarget = null;

            if (CurrentState != null) {
                CurrentState.ExitState();
                OnExitState();
            }

            if (states.Count > 0) {
                CurrentState = states[0];
                CurrentState?.EnterState();
            }
        }

        protected virtual void InitializeActions() {
            actions ??= GetAttachedActions();
            foreach (var action in actions) {
                action.Initialization();
            }
        }

        protected virtual void InitializeDecisions() {
            decisions ??= GetAttachedDecisions();
            foreach (var decision in decisions) {
                decision.Initialization();
            }
        }

        protected virtual void InitializeSpawnPosition() {
            if (spawnPositionTarget == null) {
                spawnPositionTarget = new GameObject($"[SpawnPosition] {owner.playerID}").transform;
            }
            spawnPositionTarget.position = owner.transform.position;
        }

        #endregion

        #region Update

        public override void OnUpdate() {
            // 현재 State의 Action 재생
            if (!brainActive || CurrentState == null || Time.deltaTime == 0f) return;
            if (owner.conditionState.CurrentState is YisoCharacterStates.CharacterConditions.Dead or YisoCharacterStates
                .CharacterConditions
                .Frozen or YisoCharacterStates.CharacterConditions.Stunned)
                return; // TODO 일일히 제어해줘야 하는 경우에 이 코드 지우고 세부 코드에 일일히 써주면 됨
            if (Time.time - lastActionsUpdate > actionDelay) {
                CurrentState.PerformActions();
                lastActionsUpdate = Time.time;
            }

            // Transition에서 Decision 체크해서 다음 State으로 넘어가
            if (!brainActive) return;
            if (Time.time - lastDecisionsUpdate > decisionDelay) {
                CurrentState.EvaluateTransitions();
                lastDecisionsUpdate = Time.time;
            }

            timeInThisState += Time.deltaTime;

            StoreLastKnownPosition();
            currentState = CurrentState.stateName;
        }

        #endregion

        #region Core

        public virtual void TransitionToState(string newStateName) {
            if (CurrentState == null) {
                CurrentState = FindState(newStateName);
                CurrentState?.EnterState();
                return;
            }

            if (newStateName != CurrentState.stateName) {
                CurrentState.ExitState();
                OnExitState();

                CurrentState = FindState(newStateName);
                CurrentState?.EnterState();
            }
        }

        protected virtual void OnExitState() {
            timeInThisState = 0f;
        }

        #endregion

        #region Utils

        public YisoAIState FindState(string stateName) {
            foreach (var state in states.Where(state => state.stateName == stateName)) {
                return state;
            }

            if (stateName != "") {
                Debug.LogError("You're trying to transition to state '" + stateName + "' in " + this.gameObject.name +
                               "'s AI Brain, but no state of this name exists. Make sure your states are named properly, and that your transitions states match existing states.");
            }

            return null;
        }

        protected virtual void StoreLastKnownPosition() {
            if (mainTarget != null) {
                lastKnownTargetPosition = mainTarget.transform.position;
            }
        }

        #endregion

        protected override void OnDestroy() {
            base.OnDestroy();
            if (spawnPositionTarget != null) {
                Destroy(spawnPositionTarget.gameObject);
            }
        }
    }
}