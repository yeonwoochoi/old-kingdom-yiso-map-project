using System;
using Core.Behaviour;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.UI {
    public class GridLayoutFitter : RunIBehaviour {
        [SerializeField] private bool applyAspectRatio;
        [SerializeField] private int cellCount = 2;
        [SerializeField] private GridCalculateBase baseType = GridCalculateBase.WIDTH;

        [ShowIf("baseType", GridCalculateBase.HEIGHT), SerializeField, Title("Calculate Height")]
        private int childCount;

        private RectTransform gridRect;
        private Vector2 oldCellSize;
        private float aspectRatio;
        private GridLayoutGroup grid;

        protected override void Start() {
            grid = GetComponent<GridLayoutGroup>();
            gridRect = grid.GetComponent<RectTransform>();
            oldCellSize = grid.cellSize;
            aspectRatio = oldCellSize.x / oldCellSize.y;

            gridRect.ObserveEveryValueChanged(r => r.rect.width)
                .Where(width => width > 100f)
                .CombineLatest(gridRect.ObserveEveryValueChanged(r => r.rect.height).Where(height => height >= 100f),
                    (w, h) => new Vector2(w, h))
                .Subscribe(size => {
                    if (baseType == GridCalculateBase.WIDTH) SetWidthBase(size);
                    else SetHeightBase(size);
                }).AddTo(this);
        }
        
        private void SetWidthBase(Vector2 size) {
            var padding = grid.padding.left + grid.padding.right;
            var spacing = grid.spacing.x * (cellCount - 1);
            var xSize = (size.x - (padding + spacing)) / cellCount;
            var ySize = applyAspectRatio ? CalculateHeight(xSize) : grid.cellSize.y;
            grid.cellSize = new Vector2(xSize, ySize);
        }

        private void SetHeightBase(Vector2 size) {
            var rowCount = childCount / cellCount;
            var paddingY = grid.padding.top + grid.padding.bottom;
            var spacingY = grid.spacing.y * (rowCount - 1);
            var ySize = (size.y - (paddingY + spacingY)) / rowCount;
            var xSize = applyAspectRatio ? CalculateWidth(ySize) : grid.cellSize.x;
            grid.cellSize = new Vector2(xSize, ySize);
        }

        private float CalculateHeight(float width) => width / aspectRatio;
        private float CalculateWidth(float height) => height * aspectRatio;

        public enum GridCalculateBase {
            WIDTH, HEIGHT
        }
    }
}