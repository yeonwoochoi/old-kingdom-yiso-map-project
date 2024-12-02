using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Core.Behaviour;
using MEC;
using UnityEngine;
using UnityEngine.Events;
using Utils.ObjectId;

namespace Core.Domain.CoolDown {
    public sealed class YisoPlayerCoolDownHolder {
        public int SourceId { get; }
        public double CoolDown { get; }
        public bool IsCooling { get; private set; } = false;
        public float Progress { get; set; } = 0f;

        private readonly string tag;
        
        public YisoObjectID Id { get; }

        private readonly List<UnityAction> onCoolDownStarts = new();
        private readonly List<UnityAction<float>> onCoolDownProgresses = new();
        private readonly List<UnityAction> onCoolDownCompletes = new();

        public YisoPlayerCoolDownHolder(YisoObjectID id, int sourceId, double coolDown) {
            SourceId = sourceId;
            CoolDown = coolDown;
            Id = id;
            tag = id.ToString();
        }

        public void Clear(Index index) {
            if (index.Start != -1) RemoveCoolDownStart(index.Start); 
            if (index.Progress != -1) RemoveCoolDownProgress(index.Progress);
            if (index.Complete != -1) RemoveCoolDownComplete(index.Complete);
            index.Clear();
        }

        public void StartCoolDown(RunIBehaviour handler, Action onComplete = null) {
            handler.StartCoroutine(DoCool(onComplete), tag);
        }

        public bool StopCoolDown() {
            if (!IsCooling) return false;
            Timing.KillCoroutines(tag);
            return true;
        }

        public int AddCoolDownStart([NotNull] UnityAction onCoolDownStart) {
            onCoolDownStarts.Add(onCoolDownStart);
            return onCoolDownStarts.Count - 1;
        }

        private void RemoveCoolDownStart(int index) {
            onCoolDownStarts.RemoveAt(index);
        }

        public int AddCoolDownProgress([NotNull] UnityAction<float> onCoolDownProgress) {
            onCoolDownProgresses.Add(onCoolDownProgress);
            return onCoolDownProgresses.Count - 1;
        }

        private void RemoveCoolDownProgress(int index) {
            onCoolDownProgresses.RemoveAt(index);
        }

        public int AddCoolDownComplete([NotNull] UnityAction onCoolDownComplete) {
            onCoolDownCompletes.Add(onCoolDownComplete);
            return onCoolDownCompletes.Count - 1;
        }

        private void RemoveCoolDownComplete(int index) {
            onCoolDownCompletes.RemoveAt(index);
        }

        private void OnStartCoolDown() {
            for (var i = onCoolDownStarts.Count - 1; i >= 0; i--) {
                onCoolDownStarts[i].Invoke();
            }
        }

        private void OnProgressCoolDown() {
            for (var i = onCoolDownProgresses.Count - 1; i >= 0; i--) {
                onCoolDownProgresses[i].Invoke(Progress);
            }
        }

        private void OnCompleteCoolDown() {
            for (var i = onCoolDownCompletes.Count - 1; i >= 0; i--) {
                onCoolDownCompletes[i].Invoke();
            }
        }

        private IEnumerator<float> DoCool(Action onComplete = null) {
            IsCooling = true;
            var current = (float) CoolDown;
            OnStartCoolDown();
            while (current >= 0) {
                Progress = current / (float) CoolDown;
                OnProgressCoolDown();
                current -= Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }
            OnCompleteCoolDown();
            onComplete?.Invoke();
            IsCooling = false;
        }
        
        public class Index {
            public int Start { get; set; } = -1;
            public int Progress { get; set; } = -1;
            public int Complete { get; set; } = -1;
            public bool IsCool { get; set; }

            public void Clear() {
                Start = -1;
                Progress = -1;
                Complete = -1;
            }
        }
    }
}