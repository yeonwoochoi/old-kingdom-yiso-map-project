using Character.Core;
using Core.Behaviour;
using Manager_Temp_;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tools.Layer {
    // when object exit the trigger, put it to the assigned sorting layers
    // used in the stair objects for player to travel between layers
    [AddComponentMenu("Yiso/Tools/Layer/SortingLayerChangerOnTrigger")]
    public class YisoSortingLayerChangerOnTrigger : RunIBehaviour {
        public LayerMask targetLayer = LayerManager.PlayerLayerMask | LayerManager.EnemiesLayerMask | LayerManager.AlliesLayerMask;

        public LayerManager.SortingLayerType sortingLayer;

        private void OnTriggerExit2D(Collider2D other) {
            if (!LayerManager.CheckIfInLayer(other.gameObject, targetLayer)) return;
            if (!other.gameObject.TryGetComponent<YisoCharacter>(out var character)) return;

            // 부모 Renderer
            var sortingGroup = character.GetComponent<SortingGroup>();
            if (sortingGroup != null) {
                sortingGroup.sortingLayerName = LayerManager.SortingLayerTypeToName(sortingLayer);
            }

            // 자식 Renderer
            var srs = character.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in srs) {
                sr.sortingLayerName = LayerManager.SortingLayerTypeToName(sortingLayer);
            }
        }
    }
}