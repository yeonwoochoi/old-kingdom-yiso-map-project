using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Core.Behaviour;
using Core.Domain.Actor;
using Core.Domain.Entity;
using MEC;
using UnityEngine;
using Utils.ObjectId;

namespace Core.Domain.Effect {
    public class YisoEffect {
        private readonly string tag;
        private readonly IYisoEffect effect;
        
        public YisoObjectID Id { get; }

        public int SourceId => effect.SourceId;
        public int Duration => effect.Duration;
        public bool IsBuff => effect is IYisoBuff;
        public bool IsEffecting { get; private set; } = false;
        public bool IsRestartEffect { get; private set; } = false;

        private readonly List<Action> onStartEffects = new();
        private readonly List<Action<float>> onProgressEffects = new();
        private readonly List<Action> onCompleteEffects = new();

        private RunIBehaviour handler;

        public YisoEffect(YisoObjectID id, IYisoEffect effect, RunIBehaviour handler) {
            Id = id;
            tag = id.ToString();
            this.effect = effect;
            this.handler = handler;
        }
        
        
        public void Start(IYisoEntity entity, bool restart) {
            var coroutine = effect.IsOverTime
                ? DOOverTimeEffect(entity, restart)
                : DOEffect(entity, restart);
            handler.StartCoroutine(coroutine, tag);
        }

        public void Restart(IYisoEntity entity) {
            handler.KillCoroutine(tag);
            if (!IsRestartEffect) {
                onStartEffects.RemoveAt(0);
                IsRestartEffect = true;
            }
            
            Start(entity, true);
        }

        public void Stop(IYisoEntity entity) {
            if (!IsEffecting) return;
            handler.KillCoroutine(tag);
        }

        public int AddOnStartEffect([NotNull] Action onStartEffect) {
            onStartEffects.Add(onStartEffect);
            return onStartEffects.Count - 1;
        }

        public int AddOnProgressEffect([NotNull] Action<float> onProgressEffect) {
            onProgressEffects.Add(onProgressEffect);
            return onProgressEffects.Count - 1;
        }

        public int AddOnCompleteEffect([NotNull] Action onCompleteEffect) {
            onCompleteEffects.Add(onCompleteEffect);
            return onCompleteEffects.Count - 1;
        }

        private void OnStartEffects() {
            for (var i = onStartEffects.Count - 1; i >= 0; i--) {
                onStartEffects[i].Invoke();
            }
        }

        private void OnProgressEffects(float progress) {
            for (var i = onProgressEffects.Count - 1; i >= 0; i--) {
                onProgressEffects[i].Invoke(progress);
            }
        }

        private void OnCompleteEffects() {
            for (var i = onCompleteEffects.Count - 1; i >= 0; i--) {
                onCompleteEffects[i].Invoke();
            }
        }

        private IEnumerator<float> DOEffect(IYisoEntity entity, bool restart = false) {
            IsEffecting = true;
            var time = 1f;
            if (!restart) effect?.OnStart(entity);
            OnStartEffects();
            while (time >= 0f) {
                time -= (Time.deltaTime / Duration);
                OnProgressEffects(time);
                effect?.OnProgress(entity, time);
                yield return Timing.WaitForOneFrame;
            }
            effect?.OnComplete(entity);
            OnCompleteEffects();
            IsEffecting = false;
        }

        private IEnumerator<float> DOOverTimeEffect(IYisoEntity entity, bool restart) {
            IsEffecting = true;
            var valuePerSeconds = GetValuePerSeconds();
            var current = Duration;
            if (!restart) effect?.OnStart(entity);
            OnStartEffects();
            while (current >= 0) {
                effect?.OnOverTimeProgress(entity, valuePerSeconds);
                yield return Timing.WaitForSeconds(1f);
                current--;
            }
            effect?.OnComplete(entity);
            OnCompleteEffects();
            IsEffecting = false;
        }

        private double GetValuePerSeconds() {
            var totalValue = effect.TotalValue;
            return totalValue / Duration;
        }
    }
}