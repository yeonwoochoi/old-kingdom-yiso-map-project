using System.Collections;
using System.Linq;
using Core.Behaviour;
using Manager_Temp_;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Controller.Object {
    [AddComponentMenu("Yiso/Controller/Object/RendererFader")]
    public class YisoRendererFader : RunIBehaviour {
        public LayerMask targetLayerMask = LayerManager.PlayerLayerMask;
        public AnimationCurve curve = new(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        public float time = 0.2f;
        public float finalAlpha = 0.3f;

        public bool hideChildRenderers = true;

        public Tilemap tilemap;

        private SpriteRenderer rendererToHide;
        private SpriteRenderer[] childRenderers;
        private Color initialColor;
        private Color[] childInitialColors;
        private Color currentColor;
        private Color[] childCurrentColors;

        protected override void Start() {
            curve.preWrapMode = WrapMode.Once;
            curve.postWrapMode = WrapMode.ClampForever;
            rendererToHide = gameObject.GetComponent<SpriteRenderer>();
            if (hideChildRenderers) {
                childRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>().Where(c => c.gameObject != gameObject).ToArray();
                if (childRenderers != null && childRenderers.Length > 0) {
                    childInitialColors = new Color[childRenderers.Length];
                    childCurrentColors = new Color[childRenderers.Length];
                    for (var i = 0; i < childRenderers.Length; i++) {
                        childInitialColors[i] = childRenderers[i].color;
                        childCurrentColors[i] = childRenderers[i].color;
                    }
                }
            }

            if (rendererToHide != null) {
                initialColor = rendererToHide.color;
            }

            if (tilemap != null) {
                initialColor = tilemap.color;
            }

            currentColor = initialColor;
        }


        private void OnTriggerEnter2D(Collider2D other) {
            if (!LayerManager.CheckIfInLayer(other.gameObject, targetLayerMask)) return;
            if (!gameObject.activeInHierarchy) return;
            if (rendererToHide != null || tilemap != null) {
                StartCoroutine(AnimCurve(rendererToHide, initialColor.a, finalAlpha));
            }
            if (!hideChildRenderers || childRenderers == null) return;
            for (var i = 0; i < childRenderers.Length; i++) {
                var childRenderer = childRenderers[i];
                var childInitialColor = childInitialColors[i];
                StartCoroutine(AnimCurve(childRenderer, childInitialColor.a, finalAlpha, true, i));
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!LayerManager.CheckIfInLayer(other.gameObject, targetLayerMask)) return;
            if (!gameObject.activeInHierarchy) return;
            if (rendererToHide != null || tilemap != null) {
                StartCoroutine(AnimCurve(rendererToHide, finalAlpha, initialColor.a));
            }
            if (!hideChildRenderers || childRenderers == null) return;
            for (var i = 0; i < childRenderers.Length; i++) {
                var childRenderer = childRenderers[i];
                var childInitialColor = childInitialColors[i];
                StartCoroutine(AnimCurve(childRenderer, finalAlpha, childInitialColor.a, true, i));
            }
        }

        private IEnumerator AnimCurve(SpriteRenderer targetRenderer, float initialPosition, float finalPosition, bool isChildRenderer = false, int childIndex = 0) {
            var i = 0f;
            var rate = 1f / time;
            while (i < 1) {
                i += rate * Time.deltaTime;
                var resultValue = Mathf.Lerp(initialPosition, finalPosition, curve.Evaluate(i));
                if (!isChildRenderer) {
                    currentColor.a = resultValue;
                    if (tilemap != null) tilemap.color = currentColor;
                    if (targetRenderer != null) targetRenderer.color = currentColor;
                }
                else {
                    childCurrentColors[childIndex].a = resultValue;
                    childRenderers[childIndex].color = childCurrentColors[childIndex];
                }
                yield return 0;
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            StopAllCoroutines();
        }
    }
}