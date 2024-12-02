using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Core.Domain.Locale;
using Core.Service;
using Core.Service.UI.HUD;
using MEC;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;
using Utils.ObjectId;

namespace UI.HUD.Timer {
    public class YisoPlayerHUDTimerUI : YisoUIController {
        [SerializeField] private Image progressBar;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI titleText;

        private CanvasGroup canvasGroup;
        private bool pause = false;

        public bool Visible {
            get => canvasGroup.IsVisible();
            set => canvasGroup.Visible(value);
        }

        private readonly List<UnityAction<float>> onProgressActions = new();
        private readonly List<UnityAction> onStartActions = new();
        private readonly List<UnityAction> onCompleteActions = new();

        private string timerTag = string.Empty;
        
        private bool hideWhenDone;

        protected override void OnEnable() {
            base.OnEnable();
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>().RegisterOnTimer(OnTimer);
        }

        protected override void OnDisable() {
            base.OnDisable();
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>().UnregisterOnTimer(OnTimer);
        }

        protected override void Start() {
            base.Start();
            timerTag = YisoObjectID.GenerateString();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnTimer(Actions action, object data = null) {
            switch (action) {
                case Actions.START:
                    if (data == null) throw new NullReferenceException("Timer args must not be null when start");
                    var args = (YisoPlayerHUDTimerEventArgs) data!;
                    hideWhenDone = args.HideWhenDone;
                    DisplayTime(args.Time);
                    onStartActions.AddRange(args.StartActions);
                    onProgressActions.AddRange(args.ProgressActions);
                    onCompleteActions.AddRange(args.CompleteActions);
                    Visible = true;
                    StartCoroutine(DOTimer(args.Time), timerTag);
                    break;
                case Actions.PAUSE:
                    pause = true;
                    break;
                case Actions.RESUME:
                    pause = false;
                    break;
                case Actions.STOP:
                    ClearTimer();
                    Visible = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private void InvokeOnProgress(float progress) {
            foreach (var action in onProgressActions) action(progress);
            progressBar.fillAmount = progress;
        }

        private IEnumerator<float> DOTimer(float time) {
            foreach (var action in onStartActions) action();
            var current = time;

            while (current >= 0) {
                while (pause) yield return Timing.WaitForOneFrame;
                current -= Time.deltaTime;
                var progress = current / time;
                InvokeOnProgress(progress);
                if (current >= 0) DisplayTime(current);
                yield return Timing.WaitForOneFrame;
            }
            
            DisplayTime(0);

            foreach (var action in onCompleteActions) action();
            if (hideWhenDone) {
                Visible = false;
                ClearTimer();
            }
        }

        private void ClearTimer() {
            DisplayTime(0);
            progressBar.fillAmount = 1f;
            onStartActions.Clear();
            onProgressActions.Clear();
            onCompleteActions.Clear();
            pause = false;
        }

        private void DisplayTime(float time) {
            var minutes = Mathf.FloorToInt(time / 60);
            var seconds = Mathf.FloorToInt(time % 60);
            timerText.SetText(minutes >= 100 ? $"{minutes:D3}:{seconds:D2}" : $"{minutes:D2}:{seconds:D2}");
        }

        public enum Actions {
            START, PAUSE, RESUME, STOP
        }
    }
}