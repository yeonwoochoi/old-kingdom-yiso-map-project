using Core.Behaviour;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Interact.WorldMap.Spot {
    public class YisoInteractWorldMapSpotUI : RunIBehaviour {
        [SerializeField, Title("Node")] private YisoMapTypes nextNode;
        [SerializeField] private YisoMapTypes prevNode;
        
        private Button button;
        
        public event UnityAction<YisoMapTypes, YisoMapTypes> OnClickNodeEvent;

        protected override void Start() {
            base.Start();
            button = GetComponent<Button>();
            
            button.onClick.AddListener(() => {
                Debug.Log($"OnClickSpot => next: {nextNode}");
                OnClickNodeEvent?.Invoke(prevNode, nextNode);
            });
        }
    }
}