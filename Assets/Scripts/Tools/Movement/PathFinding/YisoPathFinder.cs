using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Movement.PathFinding {
    public class YisoPathFinder : MonoBehaviour {
        [SerializeField, BoxGroup("Anchors")] private GameObject lowerLeftAnchor;
        [SerializeField, BoxGroup("Anchors")] private GameObject upperRightAnchor;

        private Vector3 centerPosition = Vector3.zero;

        public void Scan() {
            var lowerLeftPosition = lowerLeftAnchor.transform.position;
            var upperRightPosition = upperRightAnchor.transform.position;

            var activeGridPath = AstarPath.active.data.gridGraph;
            var width = (upperRightPosition.x - lowerLeftPosition.x);
            var depth = (upperRightPosition.y - lowerLeftPosition.y);

            if (centerPosition == Vector3.zero) {
                var centerWidth = width / 2;
                var centerDepth = depth / 2;
                centerPosition = new Vector3(lowerLeftPosition.x + centerWidth, lowerLeftPosition.y + centerDepth);
            }

            activeGridPath.center = centerPosition;
            activeGridPath.SetDimensions((int) width, (int) depth, 1f);

            AstarPath.active.Scan();
        }
    }
}