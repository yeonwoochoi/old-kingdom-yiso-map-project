using System;
using Core.Behaviour;
using Core.Service.UI.Event;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.HUD.Interact {
    public class YisoPlayerInteractButtonsUI : RunIBehaviour {
        [SerializeField, Title("Buttons")] private YisoPlayerInteractionButtonUI grabButton;
        [SerializeField] private YisoPlayerInteractionButtonUI speechButton;
        [SerializeField] private YisoPlayerInteractionButtonUI enterButton;
        [SerializeField] private YisoPlayerInteractionButtonUI spawnButton;

        public void Show(YisoHudUIInteractTypes type, UnityAction onClick) {
            GetButtonByType(type).Visible(true, onClick);
        }

        public void Hide(YisoHudUIInteractTypes type) {
            GetButtonByType(type).Visible(false);
        }

        private YisoPlayerInteractionButtonUI GetButtonByType(YisoHudUIInteractTypes type) => type switch {
            YisoHudUIInteractTypes.SPEECH => speechButton,
            YisoHudUIInteractTypes.ENTER => enterButton,
            YisoHudUIInteractTypes.GRAB => grabButton,
            YisoHudUIInteractTypes.SPAWN => spawnButton,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}