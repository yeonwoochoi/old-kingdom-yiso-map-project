using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Cool {
    [Serializable]
    public class YisoCooldown {
        public enum CooldownStates {
            Idle,
            Start,
            InCool,
            Stop
        }

        public bool unlimited = false;
        public bool canInterrupt = true;
        public float duration = 2f;
        [ReadOnly] public CooldownStates cooldownState = CooldownStates.Idle;

        public bool Ready {
            get {
                if (!initialized) return false;
                if (unlimited) return true;
                return cooldownState == CooldownStates.Idle;
            }
        }

        public bool IsInCoolTime => cooldownState != CooldownStates.Idle;

        public float CoolTimeLeft {
            get {
                if (currentDurationLeft <= 0f) return 0;
                return currentDurationLeft;
            }
        }

        public int CoolTimeLeftInteger => (int) CoolTimeLeft;

        public delegate void OnStateChangeDelegate(CooldownStates newState);

        public OnStateChangeDelegate onStateChange;

        protected bool initialized = false;
        protected float currentDurationLeft = 0f;

        public void Initialization() {
            ChangeState(CooldownStates.Idle);
            if (duration < 0f) duration = 0f;
            currentDurationLeft = duration;
            initialized = true;
        }

        #region Public API

        /// <summary>
        /// 쿨타임 시작
        /// </summary>
        public void Start() {
            if (!Ready) return;
            currentDurationLeft = duration;
            ChangeState(CooldownStates.Start);
        }

        public void Update() {
            if (unlimited) return;
            switch (cooldownState) {
                case CooldownStates.Idle:
                    break;
                case CooldownStates.Start:
                    if (duration <= 0f || currentDurationLeft <= 0f) {
                        ChangeState(CooldownStates.Idle);
                    }
                    else {
                        ChangeState(CooldownStates.InCool);
                    }

                    break;
                case CooldownStates.InCool:
                    currentDurationLeft -= Time.deltaTime;
                    if (currentDurationLeft <= 0f) {
                        ChangeState(CooldownStates.Stop);
                    }

                    break;
                case CooldownStates.Stop:
                    currentDurationLeft = 0f;
                    ChangeState(CooldownStates.Idle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 쿨타임 초기화
        /// </summary>
        public void Stop() {
            if (!canInterrupt) return;
            ChangeState(CooldownStates.Idle);
            currentDurationLeft = 0f;
        }

        #endregion

        #region State

        protected void ChangeState(CooldownStates newState) {
            cooldownState = newState;
            onStateChange?.Invoke(newState);
        }

        #endregion
    }
}