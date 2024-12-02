using System.Collections.Generic;
using Core.Behaviour;
using UnityEngine;
using UnityEngine.Events;

namespace BaseCamp.Player {
    public class YisoBaseCampPlayerInputController : RunIBehaviour {
        public event UnityAction<YisoBaseCampPlayerInputEventArgs> OnInputEvent; 

        private readonly List<YisoBaseCampPlayerInputEventArgs> eventArgs = new();
        
        public override void OnUpdate() {
            var inputX = Input.GetAxisRaw("Horizontal");
            var inputY = Input.GetAxisRaw("Vertical");
            var moveArgs =
                YisoBaseCampPlayerInputEventArgs.GetEventArgs<YisoBaseCampPlayerMovementInputEventArgs>(
                    YisoBaseCampPlayerInputEventTypes.MOVEMENT);
            
            
            moveArgs.Movement = new Vector2(inputX, inputY);
            eventArgs.Add(moveArgs);

            if (Input.GetKeyDown(KeyCode.E)) eventArgs.Add(YisoBaseCampPlayerInputEventArgs.ARGS[YisoBaseCampPlayerInputEventTypes.INTERACT]);
            if (Input.GetKeyDown(KeyCode.Z)) eventArgs.Add(YisoBaseCampPlayerInputEventArgs.ARGS[YisoBaseCampPlayerInputEventTypes.DASH]);
            
            foreach (var args in eventArgs) OnInputEvent?.Invoke(args);
            eventArgs.Clear();
        }
    }
    
    public enum YisoBaseCampPlayerInputEventTypes {
        MOVEMENT,
        ATTACK,
        DASH,
        INTERACT
    }

    public abstract class YisoBaseCampPlayerInputEventArgs {
        public static readonly Dictionary<YisoBaseCampPlayerInputEventTypes, YisoBaseCampPlayerInputEventArgs> ARGS = new() {
            { YisoBaseCampPlayerInputEventTypes.MOVEMENT, new YisoBaseCampPlayerMovementInputEventArgs() },
            { YisoBaseCampPlayerInputEventTypes.ATTACK, new YisoBaseCampPlayerAttackInputEventArgs() },
            { YisoBaseCampPlayerInputEventTypes.DASH, new YisoBaseCampPlayerDashInputEventArgs() },
            { YisoBaseCampPlayerInputEventTypes.INTERACT , new YisoBaseCampPlayerInteractInputEventArgs() }
        };

        public static T GetEventArgs<T>(YisoBaseCampPlayerInputEventTypes type) where T : YisoBaseCampPlayerInputEventArgs => (T)ARGS[type];
    }

    public class YisoBaseCampPlayerMovementInputEventArgs : YisoBaseCampPlayerInputEventArgs {
        public Vector2 Movement { get; set; }
    }

    public class YisoBaseCampPlayerAttackInputEventArgs : YisoBaseCampPlayerInputEventArgs { }
    
    public class YisoBaseCampPlayerDashInputEventArgs : YisoBaseCampPlayerInputEventArgs { }

    public class YisoBaseCampPlayerInteractInputEventArgs : YisoBaseCampPlayerInputEventArgs { }
}