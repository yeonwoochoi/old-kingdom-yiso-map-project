using System.Collections.Generic;
using Core.Behaviour;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using UI.Interact.WorldMap.Spot;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Interact.WorldMap.Component {
    public class YisoInteractWorldMapComponentUI : RunIBehaviour {
        [SerializeField, Title("Settings")] private YisoMapTypes type;
        [SerializeField, PreviewField] private Sprite mapSprite;
        [SerializeField, Title("Spots")] private List<YisoInteractWorldMapSpotUI> spotUIs;
        [SerializeField, Title("Nodes")] private YisoInteractWorldMapComponentUI nextMap;
        [SerializeField] private YisoInteractWorldMapSpotUI prevMap;
        
        public YisoMapTypes MapType => type;
        public Sprite MapSprite => mapSprite;

        public event UnityAction<YisoMapTypes, YisoMapTypes, YisoMapTypes> OnClickSpotEvent;

        public void RegisterEvents() {
            foreach (var spot in spotUIs) {
                spot.OnClickNodeEvent += OnClickSpot;
            }
        }

        public void UnregisterEvents() {
            foreach (var spot in spotUIs) {
                spot.OnClickNodeEvent -= OnClickSpot;
            }
        }
        
        private void OnClickSpot(YisoMapTypes prevNode, YisoMapTypes nextNode) {
            OnClickSpotEvent?.Invoke(prevNode, type, nextNode);
        }
    }
}