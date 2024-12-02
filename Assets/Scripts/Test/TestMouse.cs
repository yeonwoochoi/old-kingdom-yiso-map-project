using System;
using Core.Behaviour;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Test {
    public class TestMouse : RunIBehaviour {
        public LayerMask layerMask;
        
        public GameObject signObj;
        public GameObject obj;
        public Sprite normalSprite;
        public Sprite speechSprite;
        public Sprite attackSprite;
        
        private Texture2D normalTexture;
        private Texture2D speechTexture;
        private Texture2D attackTexture;
        
        private Vector2 hotSpot = Vector2.zero;

        private float checkInterval = 0.1f;
        private float nextCheckTime = 0f;

        protected override void Start() {
            CreateTexture(normalSprite, out normalTexture);
            CreateTexture(speechSprite, out speechTexture);
            CreateTexture(attackSprite, out attackTexture);
            SetCursor(normalTexture);
        }

        public override void OnUpdate() {
            if (Input.GetMouseButtonDown(0)) {
                OnClickLeftButton();
            }

            if (Input.GetMouseButtonUp(0)) { }

            if (Input.GetMouseButtonDown(1)) { }

            if (Input.GetMouseButtonUp(1)) { }

            if (Time.time >= nextCheckTime) {
                nextCheckTime = Time.time + checkInterval;
                CheckForObjectAtMousePosition();
            }
        }

        private void SetCursor(Texture2D texture) {
            // hotSpot = new Vector2(texture.width / 2, -texture.height / 2);
            Cursor.SetCursor(texture, hotSpot, CursorMode.Auto);
        }

        private void OnClickLeftButton() {
            var mousePosition = Input.mousePosition;
            mousePosition.z = 10.0f;

            var worldPosition = UnityEngine.Camera.main!.ScreenToWorldPoint(mousePosition);
            // obj.transform.position = worldPosition;
            // Debug.Log($"Object move to: {worldPosition}");
            var sign = CreateSign().GetComponent<SpriteRenderer>();
            sign.gameObject.transform.position = worldPosition;
            sign.DOFade(0f, 0.5f).OnComplete(() => {
                Destroy(sign.gameObject);
            });
        }

        private void CreateTexture(Sprite sprite, out Texture2D texture) {
            texture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
            var pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height
            );
            
            texture.SetPixels(pixels);
            texture.Apply();
        }

        private void CheckForObjectAtMousePosition() {
            var mousePosition = Input.mousePosition;
            mousePosition.z = 10.0f;

            var worldPosition = UnityEngine.Camera.main!.ScreenToWorldPoint(mousePosition);

            var hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, layerMask);

            if (hit.collider != null) {
                var hitLayer = hit.collider.gameObject.layer;
                var layerName = LayerMask.LayerToName(hitLayer);
                if (layerName == "Npc") {
                    SetCursor(speechTexture);
                } else if (layerName == "Enemies") {
                    SetCursor(attackTexture);
                }
            } else {
                SetCursor(normalTexture);
            }
        }

        private GameObject CreateSign() {
            var obj = Instantiate(signObj, signObj.transform.position, Quaternion.identity);
            return obj;
        }
    }
}