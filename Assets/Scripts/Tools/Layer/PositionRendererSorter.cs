using Core.Behaviour;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tools.Layer {
    public class PositionRendererSorter : RunIBehaviour {
        [SerializeField] private int sortingOrderBase = 5000;
        [SerializeField] private int offset = 0;
        [SerializeField] private bool runOnlyOnce = false;

        public int Offset {
            get => offset;
            set => offset = value;
        }

        private float timer;
        private float timerMax = .1f;
        private Renderer myRenderer;
        private SortingGroup sortingGroup;

        protected override void Awake() {
            sortingGroup = gameObject.GetComponent<SortingGroup>();
            myRenderer = gameObject.GetComponent<Renderer>();
        }

        public override void OnLateUpdate() {
            timer -= Time.deltaTime;
            if (timer <= 0f) {
                timer = timerMax;
                if (sortingGroup != null) {
                    sortingGroup.sortingOrder = (int) (sortingOrderBase - transform.position.y - offset);
                }
                else if (myRenderer != null) {
                    myRenderer.sortingOrder = (int) (sortingOrderBase - transform.position.y - offset);
                }

                if (runOnlyOnce) {
                    Destroy(this);
                }
            }
        }
    }
}