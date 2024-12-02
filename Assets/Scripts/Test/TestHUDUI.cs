using System;
using Core.Behaviour;
using Core.Service;
using Core.Service.UI.HUD;
using UnityEngine;
using Utils.Extensions;

namespace Test {
    public class TestHUDUI : RunIBehaviour {
        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>()
                .SwitchToRevive(OnRevive);
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;
            YisoServiceProvider.Instance.Get<IYisoHUDUIService>()
                .SwitchToAttack();
        }

        private void OnRevive(float progress, bool revived, bool stopped) {
            Debug.Log($"Progress: {progress.ToPercentage()}, Revived: {revived}");
        }
    }
} 