using System;
using UnityEngine;

namespace Manager {
    public static class LayerManager {
        private static int MapLayer = 6;
        private static int ObstaclesLayer = 8;
        private static int GroundLayer = 9;
        private static int PlayerLayer = 10;
        private static int EnemiesLayer = 11;
        private static int NpcLayer = 14;
        private static int InteractableObjectLayer = 15;
        private static int PortalLayer = 16;
        private static int AlliesLayer = 17;
        private static int PetLayer = 18;

        public static int MapLayerMask = 1 << MapLayer;
        public static int ObstaclesLayerMask = 1 << ObstaclesLayer;
        public static int GroundLayerMask = 1 << GroundLayer;
        public static int PlayerLayerMask = 1 << PlayerLayer;
        public static int EnemiesLayerMask = 1 << EnemiesLayer;
        public static int NpcLayerMask = 1 << NpcLayer;
        public static int InteractableObjectLayerMask = 1 << InteractableObjectLayer;
        public static int PortalLayerMask = 1 << PortalLayer;
        public static int AlliesLayerMask = 1 << AlliesLayer;
        public static int PetLayerMask = 1 << PetLayer;

        private static string DefaultSortingLayerName = "Default";
        private static string Layer1SortingLayerName = "Layer1";
        private static string Layer2SortingLayerName = "Layer2";
        private static string Layer3SortingLayerName = "Layer3";
        private static string UISortingLayerName = "UI";

        public static int DefaultSortingLayer => SortingLayer.NameToID(DefaultSortingLayerName);
        public static int Layer1SortingLayer => SortingLayer.NameToID(Layer1SortingLayerName);
        public static int Layer2SortingLayer => SortingLayer.NameToID(Layer2SortingLayerName);
        public static int Layer3SortingLayer => SortingLayer.NameToID(Layer3SortingLayerName);
        public static int UISortingLayer => SortingLayer.NameToID(UISortingLayerName);

        public static bool CheckIfInLayer(GameObject targetObject, int targetLayer) {
            return ((1 << targetObject.layer) & targetLayer) != 0;
        }

        public static string SortingLayerTypeToName(SortingLayerType type) {
            return type switch {
                SortingLayerType.Default => DefaultSortingLayerName,
                SortingLayerType.Layer1 => Layer1SortingLayerName,
                SortingLayerType.Layer2 => Layer2SortingLayerName,
                SortingLayerType.Layer3 => Layer3SortingLayerName,
                SortingLayerType.UI => UISortingLayerName,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        public enum SortingLayerType {
            Default,
            Layer1,
            Layer2,
            Layer3,
            UI
        }
    }
}