using Core.Behaviour;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Interact.WorldMap {
    [RequireComponent(typeof(Image))]
    public class YisoInteractWorldMapControlUI : RunIBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler, IEndDragHandler, IPointerUpHandler {
        [SerializeField, Title("Canvas")] private Canvas interactCanvas;

        [SerializeField, Title("RectTransforms")]
        private RectTransform viewportRectTransform;

        [SerializeField, Title("Follow")] private RectTransform followRectTransform;
        
        private RectTransform mapRectTransform;

        private Vector2 dragStartPosition;
        private Vector2 mapStartPosition;
        private Vector2 initPosition;
        private Vector2 initScale;
        
        public States State { get; private set; } = States.NONE;
        
        private float initPinchDistance;
        private Vector3 initPinchScale;

        private Image mapImage;

        protected override void Start() {
            base.Start();

            mapRectTransform = (RectTransform)transform;
            mapImage = GetComponent<Image>();

            initPosition = mapRectTransform.anchoredPosition;
            initScale = mapRectTransform.localScale;
        }

        public override void OnUpdate() {
            if (Input.touchCount == 1) {
                var touch = Input.GetTouch(0);
                switch (touch.phase) {
                    case TouchPhase.Began:
                        OnBeginDrag(touch.position);
                        break;
                    case TouchPhase.Moved:
                        OnDrag(touch.position);
                        break;
                    case TouchPhase.Ended or TouchPhase.Canceled:
                        State = States.NONE;
                        break;
                }
            }
            else if (Input.touchCount == 2) {
                var touch1 = Input.GetTouch(0);
                var touch2 = Input.GetTouch(1);

                if (touch1.phase is TouchPhase.Began or TouchPhase.Began) {
                    initPinchDistance = Vector2.Distance(touch1.position, touch2.position);
                    initPinchScale = mapRectTransform.localScale;
                } else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved) {
                    State = States.SCROLL;
                    var currentPinchDistance = Vector2.Distance(touch1.position, touch2.position);
                    var scaleMultiplier = currentPinchDistance / initPinchDistance;
                    var newScale = initPinchScale * scaleMultiplier;

                    newScale.x = Mathf.Clamp(newScale.x, 0.5f, 2f);
                    newScale.y = Mathf.Clamp(newScale.y, 0.5f, 2f);
                    newScale.z = Mathf.Clamp(newScale.z, 0.5f, 2f);

                    mapRectTransform.localScale = newScale;
                    followRectTransform.localPosition = newScale;

                    var currentMapPos = mapRectTransform.anchoredPosition;
                    var viewportSize = viewportRectTransform.rect.size;
                    var mapSize = GetMapSize();

                    var clampedX = Mathf.Clamp(currentMapPos.x, viewportSize.x / 2 - mapSize.x / 2,
                        mapSize.x / 2 - viewportSize.x / 2);
                    var clampedY = Mathf.Clamp(currentMapPos.y, viewportSize.y / 2 - mapSize.y / 2,
                        mapSize.y / 2 - viewportSize.y / 2);

                    currentMapPos = new Vector2(clampedX, clampedY);

                    mapRectTransform.anchoredPosition = currentMapPos;
                    followRectTransform.anchoredPosition = currentMapPos;
                } else if (touch1.phase == TouchPhase.Ended && touch2.phase == TouchPhase.Ended) {
                    State = States.NONE;
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData) {
            OnBeginDrag(eventData.position);
        }

        private void OnBeginDrag(Vector2 position) {
            dragStartPosition = position;
            mapStartPosition = mapRectTransform.anchoredPosition;
            State = States.BEGIN_DRAG;
        }

        public void OnDrag(PointerEventData eventData) {
            OnDrag(eventData.position);
        }

        private void OnDrag(Vector2 position) {
            State = States.DRAG;
            var mouseDelta = (position - dragStartPosition) / interactCanvas.scaleFactor;
            var newMapPos = mapStartPosition + mouseDelta;

            var viewportSize = viewportRectTransform.rect.size;
            var mapSize = GetMapSize();

            var clampedX = Mathf.Clamp(newMapPos.x, viewportSize.x / 2 - mapSize.x / 2,
                mapSize.x / 2 - viewportSize.x / 2);
            var clampedY = Mathf.Clamp(newMapPos.y, viewportSize.y / 2 - mapSize.y / 2,
                mapSize.y / 2 - viewportSize.y / 2);

            
            newMapPos = new Vector2(clampedX, clampedY);

            mapRectTransform.anchoredPosition = newMapPos;
            followRectTransform.anchoredPosition = newMapPos;
        }

        public void OnScroll(PointerEventData eventData) {
            State = States.SCROLL;
            var scrollDelta = eventData.scrollDelta.y;
            var newScale = Mathf.Clamp(mapRectTransform.localScale.x + scrollDelta * 0.1f, 0.5f, 2f);

            var viewportSize = viewportRectTransform.rect.size;
            var mapSize = mapRectTransform.rect.size * newScale;

            if (mapImage.preserveAspect) {
                var aspectRatio = mapImage.sprite.bounds.size.x / mapImage.sprite.bounds.size.y;

                if (aspectRatio > 1) {
                    if (mapSize.x < viewportSize.x) {
                        newScale = viewportSize.x / mapRectTransform.rect.size.x;
                    }
                }
                else {
                    if (mapSize.y < viewportSize.y) {
                        newScale = viewportSize.y / mapRectTransform.rect.size.y;
                    }
                }
            }

            mapRectTransform.localScale = new Vector3(newScale, newScale, newScale);
            followRectTransform.localPosition = new Vector3(newScale, newScale, newScale);

            var currentMapPos = mapRectTransform.anchoredPosition;
            mapSize = GetMapSize();

            var clampedX = Mathf.Clamp(currentMapPos.x, viewportSize.x / 2 - mapSize.x / 2,
                mapSize.x / 2 - viewportSize.x / 2);
            var clampedY = Mathf.Clamp(currentMapPos.y, viewportSize.y / 2 - mapSize.y / 2,
                mapSize.y / 2 - viewportSize.y / 2);

            currentMapPos = new Vector2(clampedX, clampedY);

            mapRectTransform.anchoredPosition = currentMapPos;
            followRectTransform.anchoredPosition = currentMapPos;
        }

        private Vector2 GetMapSize() {
            var aspectRatio = mapImage.sprite.bounds.size.x / mapImage.sprite.bounds.size.y;
            var width = mapRectTransform.rect.width * mapRectTransform.localScale.x;
            var height = mapRectTransform.rect.height * mapRectTransform.localScale.y;

            if (!mapImage.preserveAspect) return new Vector2(width, height);
            if (aspectRatio > 1) {
                height = width / aspectRatio;
            }
            else {
                width = height * aspectRatio;
            }

            return new Vector2(width, height);
        }

        public void RestToInitialState() {
            mapRectTransform.anchoredPosition = initPosition;
            mapRectTransform.localScale = initScale;
            followRectTransform.anchoredPosition = initPosition;
            followRectTransform.localScale = initScale;
        }

        public enum States {
            NONE,
            BEGIN_DRAG,
            DRAG,
            END_DRAG,
            SCROLL
        }

        public void OnEndDrag(PointerEventData eventData) {
            State = States.END_DRAG;
        }

        public void OnPointerUp(PointerEventData eventData) {
            State = States.NONE;
        }
    }
}