using System.Linq;
using Sirenix.OdinInspector;
using Tools.Event;
using UnityEngine;

namespace Tools.Cutscene.Conditions {
    public struct YisoFieldEnterEvent {
        public int fieldId;

        public YisoFieldEnterEvent(int fieldId) {
            this.fieldId = fieldId;
        }

        static YisoFieldEnterEvent e;

        public static void Trigger(int fieldId) {
            e.fieldId = fieldId;
            YisoEventManager.TriggerEvent(e);
        }
    }

    [AddComponentMenu("Yiso/Tools/Cutscene/Condition/CutsceneConditionDetectCharacter")]
    public class YisoCutsceneConditionDetectCharacter : YisoCutsceneCondition {
        public enum DetectColliderType {
            Circle,
            Box,
            Polygon
        }

        public bool detectPlayer = true;
        [ShowIf("@!detectPlayer")] public GameObject[] detectTargets;
        public bool eventTrigger = true; // Quest Requirement에는 없지만 Cutscene 발동 조건인 경우 (ex. 처음 맵 들어오자마자 Cutscene 시작할때)
        [ShowIf("eventTrigger")] public int fieldId;
        public DetectColliderType detectColliderType = DetectColliderType.Circle;

        [ShowIf("detectColliderType", DetectColliderType.Circle)]
        public float radius = 6f;

        [ShowIf("detectColliderType", DetectColliderType.Box)]
        public Vector2 boxSize = Vector2.one * 6f;

        protected GameObject targetCharacter;
        protected BoxCollider2D boxCollider;
        protected CircleCollider2D circleCollider;
        protected PolygonCollider2D polygonCollider;
        protected bool isEventTriggered = false;

        public override void Initialization() {
            base.Initialization();

            if (GetComponent<Collider2D>() != null) return;
            switch (detectColliderType) {
                case DetectColliderType.Circle:
                    circleCollider = GetOrAddComponent<CircleCollider2D>();
                    circleCollider.isTrigger = true;
                    circleCollider.radius = radius;
                    break;
                case DetectColliderType.Box:
                    boxCollider = GetOrAddComponent<BoxCollider2D>();
                    boxCollider.isTrigger = true;
                    boxCollider.size = boxSize;
                    break;
                case DetectColliderType.Polygon:
                    polygonCollider = GetNonNullComponent<PolygonCollider2D>();
                    polygonCollider.isTrigger = true;
                    break;
            }
        }

        public override bool CanPlay() {
            return targetCharacter != null;
        }

        protected virtual void RaiseEvent() {
            if (!eventTrigger) return;
            if (isEventTriggered) return;
            YisoFieldEnterEvent.Trigger(fieldId);
            isEventTriggered = true;
        }
        
        private void HandleTrigger(Collider2D other, bool isEntering) {
            if (detectPlayer) {
                if (!other.gameObject.CompareTag("Player")) return;
            } else {
                if (detectTargets == null || !detectTargets.Contains(other.gameObject)) return;
            }

            if (isEntering) {
                targetCharacter = other.gameObject;
                RaiseEvent();
            } else {
                targetCharacter = null;
                isEventTriggered = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            HandleTrigger(other, true);
        }

        private void OnTriggerStay2D(Collider2D other) {
            HandleTrigger(other, true);
        }

        private void OnTriggerExit2D(Collider2D other) {
            HandleTrigger(other, false);
        }
    }
}