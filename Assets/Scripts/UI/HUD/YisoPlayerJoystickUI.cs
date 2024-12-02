using System;
using Core.Behaviour;
using Core.Service;
using Core.Service.UI;
using Core.Service.UI.HUD;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI.HUD {
    public class YisoPlayerJoystickUI : RunIBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        [SerializeField, Title("Settings")] private float movementRange = 50f;
        [SerializeField] private bool baseCamp;

        public event UnityAction<Vector2> OnJoystickMovementEvent;
        public event UnityAction<bool> OnJoystickInputEvent; 

        private Vector3 startPosition;
        private Vector2 pointerDownPosition;
        private Vector2 movePosition;

        private RectTransform parentRect = null;
        private RectTransform rect = null;

        private IYisoHUDUIService huduiService;

        protected override void Start() {
            rect = (RectTransform) transform;
            startPosition = rect.anchoredPosition;
            parentRect = transform.parent.GetComponent<RectTransform>();
            if (!baseCamp)
                huduiService = YisoServiceProvider.Instance.Get<IYisoHUDUIService>();
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (eventData == null) throw new ArgumentException(nameof(eventData));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect,
                eventData.position,
                eventData.pressEventCamera, out pointerDownPosition);
            if (baseCamp) {
                RaiseJoystickInput(true);
                return;
            }
            huduiService.RaiseJoystickInput(true);
        }

        public void OnPointerUp(PointerEventData eventData) {
            rect.anchoredPosition = startPosition;
            movePosition = Vector2.zero;
            if (baseCamp) {
                RaiseJoystickMovement(movePosition);
                RaiseJoystickInput(false);
                return;
            }
            huduiService.RaiseJoystickMovement(movePosition);
            huduiService.RaiseJoystickInput(false);
        }

        public void OnDrag(PointerEventData eventData) {
            if (eventData == null) throw new ArgumentException(nameof(eventData));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position,
                eventData.pressEventCamera, out var position);
            var delta = position - pointerDownPosition;
            
            delta = Vector2.ClampMagnitude(delta, movementRange);
            rect.anchoredPosition = startPosition + (Vector3) delta;

            movePosition = new Vector2(delta.x / movementRange, delta.y / movementRange);
            if (!baseCamp)
                huduiService.RaiseJoystickMovement(movePosition);
            else 
                RaiseJoystickMovement(movePosition);
        }

        private void RaiseJoystickInput(bool flag) {
            OnJoystickInputEvent?.Invoke(flag);
        }

        private void RaiseJoystickMovement(Vector2 movement) {
            OnJoystickMovementEvent?.Invoke(movement);
        }
    }
}