using System;
using Core.Behaviour;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Utils.Extensions;

namespace Controller.Effect {
    [AddComponentMenu("Yiso/Controller/Effect/Damage Effect Controller")]
    public class YisoDamageEffectController : RunISortingBehaviour {
        [SerializeField, Title("Colors")] private Color normalColor = Color.white;
        [SerializeField] private Color criticalColor = Color.red;
        [SerializeField, Title("Font Size")] private float normalFontSize = 4f;
        [SerializeField] private float criticalFontSize = 6f;

        [SerializeField, Title("Settings")] private float disappearSpeed = 3f;
        [SerializeField] private float scaleAmount = 1f;
        [SerializeField] private float moveYSpeed = 2f;
        [SerializeField] private Vector3 movementVector = new(1f, 1f);
        protected override string GetSortingId() => "DAMAGE_EFFECT";

        private const float DISAPPEAR_TIMER_MAX = 0.3f;

        private TextMeshPro textMesh;
        private float disappearTimer;
        private Color textColor;
        private Vector3 moveVector;

        private bool active = false;

        protected override void Awake() {
            base.Awake();
            textMesh = GetComponent<TextMeshPro>();
        }

        private Action onComplete = null;

        public void Setup(double damage, bool isCritical, Action onComplete = null) {
            this.onComplete = () => {
                active = false;
                onComplete?.Invoke();
            };

            textMesh.fontSize = isCritical ? criticalFontSize : normalFontSize;
            textMesh.color = isCritical ? criticalColor : normalColor;
            textMesh.sortingOrder = GetNextSortingOrder();
            textMesh.SetText(damage.ToCommaString());
            transform.localScale = Vector3.one;
            disappearTimer = DISAPPEAR_TIMER_MAX;

            textColor = textMesh.color;
            moveVector = movementVector * moveYSpeed;
            active = true;
        }

        public override void OnUpdate() {
            if (!active) return;
            transform.position += movementVector * Time.deltaTime;
            moveVector -= moveVector * 8f * Time.deltaTime;

            if (disappearTimer > DISAPPEAR_TIMER_MAX * .5f) {
                transform.localScale += Vector3.one * Time.deltaTime;
            }
            else {
                transform.localScale -= Vector3.one * Time.deltaTime;
            }

            disappearTimer -= Time.deltaTime;
            if (disappearTimer < 0) {
                textColor.a -= disappearSpeed * Time.deltaTime;
                textMesh.color = textColor;
                if (textColor.a <= 0.1) {
                    onComplete?.Invoke();
                }
            }
        }
    }
}