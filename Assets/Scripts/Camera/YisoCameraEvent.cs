using Character.Core;
using Tools.Event;
using UnityEngine;

namespace Camera {
    public enum YisoCameraEventTypes {
        SetTargetCharacter,
        SetConfiner,
        StartFollowing,
        StopFollowing,
        RefreshPosition,
        ResetPriorities
    }

    public struct YisoCameraEvent {
        public YisoCameraEventTypes eventType;
        public YisoCharacter targetCharacter;
        public Collider2D bounds;
        public float orthoSize;

        public YisoCameraEvent(YisoCameraEventTypes eventType, YisoCharacter targetCharacter = null,
            Collider2D bounds = null, float orthoSize = 5f) {
            this.eventType = eventType;
            this.targetCharacter = targetCharacter;
            this.bounds = bounds;
            this.orthoSize = orthoSize;
        }

        private static YisoCameraEvent e;

        public static void Trigger(YisoCameraEventTypes eventType) {
            e.eventType = eventType;
            YisoEventManager.TriggerEvent(e);
        }

        public static void Trigger(YisoCameraEventTypes eventType, Collider2D bounds) {
            e.eventType = eventType;
            e.bounds = bounds;
            YisoEventManager.TriggerEvent(e);
        }

        public static void Trigger(YisoCameraEventTypes eventType, YisoCharacter targetCharacter) {
            e.eventType = eventType;
            e.targetCharacter = targetCharacter;
            YisoEventManager.TriggerEvent(e);
        }
    }
}