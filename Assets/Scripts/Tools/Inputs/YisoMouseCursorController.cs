using Controller.UI;
using Core.Behaviour;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Beagle;

namespace Tools.Inputs {
    [AddComponentMenu("Yiso/Tools/Inputs/MouseCursorController")]
    public class YisoMouseCursorController : RunIBehaviour {
        [Title("Settings")] public YisoMouseClickMarkerSettings mouseClickMarkerSettings;

        [Title("Cursor")] public LayerMask hoverCheckLayerMask;

        public Texture2D defaultCursorTexture;
        public Texture2D enemiesCursorTexture;
        public Texture2D npcCursorTexture;
        public Texture2D interactableObjectCursorTexture;

        private UnityEngine.Camera mainCamera;

        private Vector2 hotSpot = Vector2.zero;

        private readonly float checkInterval = 0.1f;
        private float nextCheckTime = 0f;

        protected override void Start() {
            mouseClickMarkerSettings.Initialization();
            mainCamera = UnityEngine.Camera.main;
        }

        public override void OnUpdate() {
            if (Input.GetMouseButton(1) && !YisoInputUtils.IsPointerOverUI()) {
                ShowClickMarker();
            }
            
            if (Time.time >= nextCheckTime) {
                nextCheckTime = Time.time + checkInterval;
                ChangeCursorOnHover();
            }
        }

        private void ShowClickMarker() {
            if (!InputManager.HasInstance || mainCamera == null) return;
            if (InputManager.Instance.movementControl != InputManager.MovementControls.Mouse) return;
            mouseClickMarkerSettings.ShowMouseClickCursor(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        }

        private void ChangeCursorOnHover() {
            var mousePosition = Input.mousePosition;
            mousePosition.z = 10.0f;
            var worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            
            var hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, hoverCheckLayerMask);
            if (hit.collider != null && !YisoInputUtils.IsPointerOverUI()) {
                var hoverLayer = GetHoverLayer(hit.collider.gameObject);

                switch (hoverLayer) {
                    case HoverLayer.Enemies:
                        Cursor.SetCursor(enemiesCursorTexture, hotSpot, CursorMode.Auto);
                        break;
                    case HoverLayer.Npc:
                        Cursor.SetCursor(npcCursorTexture, hotSpot, CursorMode.Auto);
                        break;
                    case HoverLayer.InteractableObject:
                        Cursor.SetCursor(interactableObjectCursorTexture, hotSpot, CursorMode.Auto);
                        break;
                    case HoverLayer.Default:
                        Cursor.SetCursor(defaultCursorTexture, hotSpot, CursorMode.Auto);
                        break;
                }
            }
            else {
                Cursor.SetCursor(defaultCursorTexture, hotSpot, CursorMode.Auto);
            }
        }

        private HoverLayer GetHoverLayer(GameObject target) {
            if (LayerManager.CheckIfInLayer(target, LayerManager.EnemiesLayerMask)) return HoverLayer.Enemies;
            if (LayerManager.CheckIfInLayer(target, LayerManager.NpcLayerMask)) return HoverLayer.Npc;
            if (LayerManager.CheckIfInLayer(target, LayerManager.InteractableObjectLayerMask)) return HoverLayer.InteractableObject;
            return HoverLayer.Default;
        }

        private enum HoverLayer {
            Default,
            Enemies,
            Npc,
            InteractableObject
        }
    }
}