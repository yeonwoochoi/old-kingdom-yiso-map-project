using System.Collections.Generic;
using Core.Domain.Types;
using UI.Interact.Base;
using UI.Interact.WorldMap.Component;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Interact.WorldMap {
    public class YisoInteractWorldMapContentUI : YisoInteractBasePanelUI {
        [SerializeField] private Button backButton;
        [SerializeField] private Image mapImage;
        [SerializeField] private List<YisoInteractWorldMapComponentUI> componentUIs;

        private Dictionary<YisoMapTypes, YisoInteractWorldMapComponentUI> components;

        private CanvasGroup backButtonCanvas;
        private CanvasGroup mapImageCanvas;

        protected override void Start() {
            base.Start();

            backButtonCanvas = backButton.GetComponent<CanvasGroup>();
            mapImageCanvas = mapImage.GetComponent<CanvasGroup>();

            foreach (var component in componentUIs) {
                components[component.MapType] = component;
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            
            foreach (var component in componentUIs) component.RegisterEvents();
        }

        protected override void OnDisable() {
            base.OnDisable();
            
            foreach (var component in componentUIs) component.UnregisterEvents();
        }


        public override void ClearPanel() {
            
        }

        public override YisoInteractTypes GetType() => YisoInteractTypes.WORLD_MAP;
    }
}