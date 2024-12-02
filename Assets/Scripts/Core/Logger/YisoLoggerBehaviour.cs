using System;
using System.Collections.Generic;
using Core.Behaviour;
using UnityEngine.Events;

namespace Core.Logger {
    public class YisoLoggerBehaviour : RunIBehaviour {
        public UnityAction OnApplicationQuickAction { get; internal set; }
        protected override void Start() {
            base.Start();
            
        }

        private void OnApplicationQuit() {
            
        }
    }
}