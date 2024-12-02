using System.Collections;
using System.Collections.Generic;
using Core.Behaviour;
using UnityEngine;

namespace Controller.Effect {
    [AddComponentMenu("Yiso/Controller/Effect/After Image Generator")]
    public class YisoAfterImageGenerator : RunIBehaviour {
        public bool active = false;
        public bool isPlayOnStart = false;
        public SpriteRenderer spriteRendererToCopyFrom;
        public float startingOpacity = 0.5f;
        public float fadeSpeed = 2f;

        private bool initialized = false;
        private bool lastActiveStatus = false;
        private GameObject parent;
        private Queue<SpriteRenderer> currentAfterImageQueue = new Queue<SpriteRenderer>();
        private Coroutine spawnCoroutine;
        private int queueCapacity = 20;

        protected override void OnEnable() {
            base.OnEnable();
            if (spriteRendererToCopyFrom == null) {
                parent = new GameObject("Afterimages");
                parent.transform.position = Vector3.zero;
                spriteRendererToCopyFrom = GetComponent<SpriteRenderer>();
            }
            else {
                parent = gameObject;
            }
        }

        protected override void Start() {
            if (isPlayOnStart) {
                Initialization();
                Play();
            }
        }

        public void Initialization() {
            lastActiveStatus = active;
            initialized = true;
        }

        public override void OnUpdate() {
            if (!initialized || Time.time == 0f) return;
            if (lastActiveStatus == active) return;

            lastActiveStatus = active;
            if (active) {
                Play();
            }
            else {
                Pause();
            }
        }

        public virtual void DestroyAfterImages() {
            Destroy(parent);
        }

        private void Pause() {
            if (spawnCoroutine != null) {
                StopCoroutine(spawnCoroutine);
            }
        }

        private void Play() {
            if (!initialized) return;
            if (spawnCoroutine != null) {
                StopCoroutine(spawnCoroutine);
            }

            spawnCoroutine = StartCoroutine(Spawn());
        }

        private IEnumerator Spawn() {
            while (initialized) {
                yield return null;
                InstantiateAfterImage();
            }
        }

        private void InstantiateAfterImage() {
            SpriteRenderer sr;
            if (currentAfterImageQueue.Count < queueCapacity) {
                GameObject afterimage = new GameObject("Afterimage");
                afterimage.transform.SetParent(parent.transform);
                SetTransform(afterimage.transform);
                afterimage.layer = spriteRendererToCopyFrom.gameObject.layer;

                sr = afterimage.AddComponent<SpriteRenderer>();
                SetSpriteRenderer(sr);

                currentAfterImageQueue.Enqueue(sr);
            }
            else {
                sr = GetSpriteRenderer();
                SetTransform(sr.gameObject.transform);
                SetSpriteRenderer(sr);
            }

            if (fadeSpeed > 0f) {
                StartCoroutine(FadeIn(sr));
            }
        }

        private void SetTransform(Transform afterimage) {
            afterimage.position = spriteRendererToCopyFrom.transform.position;
            afterimage.rotation = spriteRendererToCopyFrom.transform.rotation;
            afterimage.localScale = spriteRendererToCopyFrom.transform.localScale;
        }

        private void SetSpriteRenderer(SpriteRenderer sr) {
            sr.sprite = spriteRendererToCopyFrom.sprite;
            sr.material = spriteRendererToCopyFrom.material;
            sr.sortingLayerID = spriteRendererToCopyFrom.sortingLayerID;
            sr.sortingLayerName = spriteRendererToCopyFrom.sortingLayerName;
            sr.sortingOrder = spriteRendererToCopyFrom.sortingOrder;
            sr.flipX = spriteRendererToCopyFrom.flipX;
            sr.flipY = spriteRendererToCopyFrom.flipY;
            var srColor = spriteRendererToCopyFrom.color;
            sr.color = new Color(srColor.r, srColor.g, srColor.b, startingOpacity);
        }

        private IEnumerator FadeIn(SpriteRenderer sr) {
            var tmp = sr.color;

            while (sr.color.a > 0) {
                tmp.a -= Time.deltaTime * fadeSpeed;
                sr.color = tmp;

                yield return null;
            }

            ReturnSpriteRenderer(sr);
        }

        private SpriteRenderer GetSpriteRenderer() {
            var sr = currentAfterImageQueue.Dequeue();
            sr.gameObject.SetActive(true);
            return sr;
        }

        private void ReturnSpriteRenderer(SpriteRenderer sr) {
            sr.gameObject.SetActive(false);
            currentAfterImageQueue.Enqueue(sr);
        }
    }
}