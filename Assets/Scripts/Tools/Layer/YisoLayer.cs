using System;
using UnityEngine;

namespace Tools.Layer {
    [Serializable]
    public class YisoLayer {
        public enum SortingLayer {
            Default = 0,
            Layer1 = 1,
            Layer2 = 2,
            Layer3 = 3
        }

        [SerializeField] private SortingLayer layerType = SortingLayer.Default;
        [SerializeField] private int orderInLayerOffset = 0;

        public YisoLayer(SortingLayer layer, int offset) {
            layerType = layer;
            orderInLayerOffset = offset;
        }

        public SortingLayer LayerType => layerType;
        public int SortingLayerIndex => (int) layerType;
        public int OrderInLayerOffset => orderInLayerOffset;
    }
}