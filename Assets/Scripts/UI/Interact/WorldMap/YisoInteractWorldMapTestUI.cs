using Core.Behaviour;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Interact.WorldMap {
    public class YisoInteractWorldMapTestUI : RunIBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler {
        public Canvas canvas;

        public RectTransform viewportRectTransform;

        private RectTransform mapRectTransform;
        private Vector2 dragStartPos;
        private Vector2 mapStartPos;

        private Image mapImage;

        private Vector2 initialPosition;
        private Vector2 initialScale;

        private bool isDragging;
        private float initialPinchDistance;
        private Vector3 initialPinchScale;

        protected override void Start() {
            base.Start();

            mapRectTransform = GetComponent<RectTransform>();
            mapImage = GetComponent<Image>();

            initialPosition = mapRectTransform.anchoredPosition;
            initialScale = mapRectTransform.localScale;
        }

        public override void OnUpdate() {
            base.OnUpdate();
            if (Input.touchCount == 1) {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) {
                    OnBeginDrag(touch.position);
                }
                else if (touch.phase == TouchPhase.Moved) {
                    OnDrag(touch.position);
                }
                else if (touch.phase is TouchPhase.Ended or TouchPhase.Canceled) {
                    isDragging = false;
                }
            }
            else if (Input.touchCount == 2) {
                var touch1 = Input.GetTouch(0);
                var touch2 = Input.GetTouch(1);

                if (touch1.phase is TouchPhase.Began or TouchPhase.Began) {
                    initialPinchDistance = Vector2.Distance(touch1.position, touch2.position);
                    initialPinchScale = mapRectTransform.localScale;
                }
                else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved) {
                    float currentPinchDistance = Vector2.Distance(touch1.position, touch2.position);
                    float scaleMultiplier = currentPinchDistance / initialPinchDistance;
                    Vector3 newScale = initialPinchScale * scaleMultiplier;

                    // 최소 및 최대 스케일 제한
                    newScale.x = Mathf.Clamp(newScale.x, 0.5f, 2f);
                    newScale.y = Mathf.Clamp(newScale.y, 0.5f, 2f);
                    newScale.z = Mathf.Clamp(newScale.z, 0.5f, 2f);

                    mapRectTransform.localScale = newScale;

                    // 확대/축소 후 경계 체크
                    Vector2 currentMapPos = mapRectTransform.anchoredPosition;
                    Vector2 viewportSize = viewportRectTransform.rect.size;
                    Vector2 mapSize = GetMapSize();

                    float clampedX = Mathf.Clamp(currentMapPos.x, viewportSize.x / 2 - mapSize.x / 2,
                        mapSize.x / 2 - viewportSize.x / 2);
                    float clampedY = Mathf.Clamp(currentMapPos.y, viewportSize.y / 2 - mapSize.y / 2,
                        mapSize.y / 2 - viewportSize.y / 2);

                    currentMapPos = new Vector2(clampedX, clampedY);

                    mapRectTransform.anchoredPosition = currentMapPos;
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData) {
            dragStartPos = eventData.position;
            mapStartPos = mapRectTransform.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData) {
            Vector2 currentMousePos = eventData.position;
            Vector2 mouseDelta = (currentMousePos - dragStartPos) / canvas.scaleFactor;
            Vector2 newMapPos = mapStartPos + mouseDelta;

            // 경계 제한 계산
            Vector2 viewportSize = viewportRectTransform.rect.size;
            Vector2 mapSize = GetMapSize();

            // 경계 계산
            float clampedX = Mathf.Clamp(newMapPos.x, viewportSize.x / 2 - mapSize.x / 2,
                mapSize.x / 2 - viewportSize.x / 2);
            float clampedY = Mathf.Clamp(newMapPos.y, viewportSize.y / 2 - mapSize.y / 2,
                mapSize.y / 2 - viewportSize.y / 2);

            newMapPos = new Vector2(clampedX, clampedY);

            mapRectTransform.anchoredPosition = newMapPos;
        }

        public void OnScroll(PointerEventData eventData) {
            float scrollDelta = eventData.scrollDelta.y;
            float newScale = Mathf.Clamp(mapRectTransform.localScale.x + scrollDelta * 0.1f, 0.5f, 2f);

            // 축소 시 맵이 뷰포트보다 작아지지 않도록 제한
            Vector2 viewportSize = viewportRectTransform.rect.size;
            Vector2 mapSize = mapRectTransform.rect.size * newScale;

            if (mapImage.preserveAspect) {
                float aspectRatio = mapImage.sprite.bounds.size.x / mapImage.sprite.bounds.size.y;

                if (aspectRatio > 1) // 가로가 더 긴 경우
                {
                    if (mapSize.x < viewportSize.x) {
                        newScale = viewportSize.x / mapRectTransform.rect.size.x;
                    }
                }
                else // 세로가 더 긴 경우
                {
                    if (mapSize.y < viewportSize.y) {
                        newScale = viewportSize.y / mapRectTransform.rect.size.y;
                    }
                }
            }

            mapRectTransform.localScale = new Vector3(newScale, newScale, newScale);

            // 확대/축소 후 경계 체크
            Vector2 currentMapPos = mapRectTransform.anchoredPosition;
            mapSize = GetMapSize();

            float clampedX = Mathf.Clamp(currentMapPos.x, viewportSize.x / 2 - mapSize.x / 2,
                mapSize.x / 2 - viewportSize.x / 2);
            float clampedY = Mathf.Clamp(currentMapPos.y, viewportSize.y / 2 - mapSize.y / 2,
                mapSize.y / 2 - viewportSize.y / 2);

            currentMapPos = new Vector2(clampedX, clampedY);

            mapRectTransform.anchoredPosition = currentMapPos;
        }

        private Vector2 GetMapSize() {
            float aspectRatio = mapImage.sprite.bounds.size.x / mapImage.sprite.bounds.size.y;
            float width = mapRectTransform.rect.width * mapRectTransform.localScale.x;
            float height = mapRectTransform.rect.height * mapRectTransform.localScale.y;

            if (mapImage.preserveAspect) {
                if (aspectRatio > 1) // 가로가 더 긴 경우
                {
                    height = width / aspectRatio;
                }
                else // 세로가 더 긴 경우
                {
                    width = height * aspectRatio;
                }
            }

            return new Vector2(width, height);
        }

        // 모바일 환경에서 터치 입력으로 드래그 시작
        private void OnBeginDrag(Vector2 position) {
            dragStartPos = position;
            mapStartPos = mapRectTransform.anchoredPosition;
            isDragging = true;
        }

        // 모바일 환경에서 터치 입력으로 드래그 중
        private void OnDrag(Vector2 position) {
            if (!isDragging) return;

            Vector2 mouseDelta = (position - dragStartPos) / canvas.scaleFactor;
            Vector2 newMapPos = mapStartPos + mouseDelta;

            // 경계 제한 계산
            Vector2 viewportSize = viewportRectTransform.rect.size;
            Vector2 mapSize = GetMapSize();

            // 경계 계산
            float clampedX = Mathf.Clamp(newMapPos.x, viewportSize.x / 2 - mapSize.x / 2,
                mapSize.x / 2 - viewportSize.x / 2);
            float clampedY = Mathf.Clamp(newMapPos.y, viewportSize.y / 2 - mapSize.y / 2,
                mapSize.y / 2 - viewportSize.y / 2);

            newMapPos = new Vector2(clampedX, clampedY);

            mapRectTransform.anchoredPosition = newMapPos;
        }

        public void RestToInitialState() {
            mapRectTransform.anchoredPosition = initialPosition;
            mapRectTransform.localScale = initialScale;
        }
    }
}