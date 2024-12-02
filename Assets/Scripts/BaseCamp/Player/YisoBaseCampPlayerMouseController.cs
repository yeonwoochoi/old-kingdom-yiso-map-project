using System;
using Core.Behaviour;
using Core.Service;
using Core.Service.ObjectPool;
using Core.Service.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace BaseCamp.Player {
    public class YisoBaseCampPlayerMouseController : RunIBehaviour {
        public event UnityAction<ButtonTypes, Modes, GameObject> OnMouseClickEvent;
        
        [SerializeField, Title("Settings")]
        private LayerMask layerMask;
        [SerializeField] private GameObject signPrefab;
        [SerializeField] private GameObject signRoot;
        [SerializeField] private UnityEngine.Camera mainCamera;

        [SerializeField, Title("Sprites")] 
        private Texture2D normalTexture2D;
        [SerializeField] private Texture2D speechTexture2D;
        [SerializeField] private Texture2D interactTexture2D;
        [SerializeField] private Texture2D portalTexture2D;
        [SerializeField] private Texture2D attackTexture2D;
        
        private Texture2D normalTexture;
        private Texture2D speechTexture;
        private Texture2D attackTexture;
        private Texture2D interactTexture;
        private Texture2D portalTexture;
        
        private Vector2 hotSpot = Vector2.zero;
        private IYisoObjectPoolService poolService;
        private IYisoUIService uiService;

        private readonly float checkInterval = 0.1f;
        private float nextCheckTime = 0f;

        private Modes currentMode = Modes.NORMAL;
        private GameObject cursorObject = null;

        private const string LAYER_NPC = "Npc";
        private const string LAYER_PORTAL = "Portal";
        private const string LAYER_ENEMIES = "Enemies";
        private const string LAYER_INTERACT_OBJECT = "InteractObject";

        protected override void Start() {
            SetCursor();
            
            poolService = YisoServiceProvider.Instance.Get<IYisoObjectPoolService>();
            poolService.WarmPool(signPrefab, 30, signRoot);
            
            uiService = YisoServiceProvider.Instance.Get<IYisoUIService>();
        }

        public override void OnUpdate() {
            if (uiService.IsReady() && uiService.IsUIShowed()) {
                currentMode = Modes.NORMAL;
                SetCursor(true);
                return;
            }
            if (Input.GetMouseButtonDown(0)) {
                OnClickMouse(ButtonTypes.LEFT);
            }

            if (Input.GetMouseButtonDown(1)) {
                OnClickMouse(ButtonTypes.RIGHT);
            }
            
            if (Time.time >= nextCheckTime) {
                nextCheckTime = Time.time + checkInterval;
                CheckObjectAtMousePosition();
            }
        }

        private void OnClickMouse(ButtonTypes type) {
            var mousePosition = Input.mousePosition;
            mousePosition.z = 10.0f;
            var worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

            GameObject target = null;
            
            if (type == ButtonTypes.RIGHT) {
                var sign = poolService.SpawnObject<SpriteRenderer>(signPrefab, signRoot);
                sign.gameObject.transform.position = worldPosition;
                if (currentMode == Modes.NORMAL)
                    sign.DOFade(1f, 0.01f);
                sign.DOFade(0f, 0.5f).OnComplete(() => {
                    poolService.ReleaseObject(sign.gameObject);
                });

                target = sign.gameObject;
            } else {
                target = cursorObject;
            }
            
            OnMouseClickEvent?.Invoke(type, currentMode, target);
        }

        private void SetCursor(bool zeroSpot = false) {
            return;
            var texture = GetTextureByMode(currentMode);
            var spot = Vector2.zero;
            if (!zeroSpot)
                spot = GetHotSpotByMode(currentMode);
            Cursor.SetCursor(texture, spot, CursorMode.Auto);
        }

        private Texture2D GetTextureByMode(Modes mode) => mode switch {
            Modes.NORMAL => normalTexture2D,
            Modes.SPEECH => speechTexture2D,
            Modes.ATTACK => attackTexture2D,
            Modes.OBJECT => interactTexture2D,
            Modes.PORTAL => portalTexture2D,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        private Vector2 GetHotSpotByMode(Modes mode) => mode switch {
            Modes.NORMAL => new Vector2(normalTexture2D.width / 2, -normalTexture2D.height / 2),
            _ => Vector2.zero
        };

        private void CheckObjectAtMousePosition() {
            var mousePosition = Input.mousePosition;
            mousePosition.z = 10.0f;
            var worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            
            var hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, layerMask);
            if (hit.collider != null) {
                var hitLayer = hit.collider.gameObject.layer;
                var layerName = LayerMask.LayerToName(hitLayer);
                currentMode = layerName switch {
                    LAYER_NPC => Modes.SPEECH,
                    LAYER_ENEMIES => Modes.ATTACK,
                    LAYER_PORTAL => Modes.PORTAL,
                    LAYER_INTERACT_OBJECT => Modes.OBJECT,
                    _ => Modes.NORMAL
                };
                cursorObject = hit.collider.gameObject;
                SetCursor();
                return;
            }
            if (currentMode == Modes.NORMAL) return;
            currentMode = Modes.NORMAL;
            cursorObject = null;
            SetCursor();
        }

        public enum Modes {
            NORMAL,
            SPEECH,
            OBJECT,
            PORTAL,
            ATTACK
        }

        public enum ButtonTypes {
            LEFT, RIGHT
        }
    }
}