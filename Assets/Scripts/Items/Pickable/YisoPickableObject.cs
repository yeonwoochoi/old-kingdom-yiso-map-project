using System.Collections;
using Character.Core;
using Controller.Map;
using Core.Behaviour;
using Core.Service;
using Core.Service.ObjectPool;
using DG.Tweening;
using Manager_Temp_;
using Sirenix.OdinInspector;
using TMPro;
using Tools.Event;
using Tools.Feedback;
using Tools.Feedback.Core;
using UnityEngine;

namespace Items.Pickable {
    public struct YisoPickableObjectEvent {
        public YisoPickableObject pickedItem;
        public GameObject picker;

        public YisoPickableObjectEvent(YisoPickableObject pickedItem, GameObject picker) {
            this.pickedItem = pickedItem;
            this.picker = picker;
        }

        static YisoPickableObjectEvent e;

        public static void Trigger(YisoPickableObject pickedItem, GameObject picker) {
            e.pickedItem = pickedItem;
            e.picker = picker;
            YisoEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// Initialization(Item, Money인 경우) => SetItem, SetMoney
    /// </summary>
    [AddComponentMenu("Yiso/Items/PickableObject")]
    public class YisoPickableObject : RunIBehaviour, IYisoEventListener<YisoMapChangeEvent>, IYisoEventListener<YisoBountyChangeEvent> {
        [Title("LifeTime")] public bool applyLifeTime = false;
        [ShowIf("applyLifeTime"), MinValue(0)] public float lifeTime = 200f;

        [Title("CoolTime")] public bool applyInitialDropCooldown = true;
        [ShowIf("applyInitialDropCooldown")] public float initialDropCooldown = 1f;

        [Title("UI")] public bool showNameText = true;
        [ShowIf("showNameText")] public SpriteRenderer itemSprite;
        [ShowIf("showNameText")] public GameObject itemNameHolder;
        [ShowIf("showNameText")] public TextMeshPro itemNameText;

        [Title("Feedbacks")] public YisoFeedBacks pickedFeedbacks;

        protected bool initialized = false;
        protected bool isBeingPicked = false;
        protected bool isMovingToPicker = false;
        protected SpriteRenderer[] spriteRenderers;
        protected readonly float MoveTime = 0.5f;
        protected int spawnStageId;
        protected int spawnMapId;

        public IYisoObjectPoolService PoolService => YisoServiceProvider.Instance.Get<IYisoObjectPoolService>();
        public bool IsPooled { get; set; } = true;
        public bool Pickable { get; protected set; } = false;

        #region Initialization

        protected override void Awake() {
            Initialization();
        }

        protected override void OnEnable() {
            base.OnEnable();
            this.YisoEventStartListening<YisoMapChangeEvent>();
            this.YisoEventStartListening<YisoBountyChangeEvent>();

            isMovingToPicker = false;
            isBeingPicked = false;

            if (GameManager.HasInstance) {
                spawnStageId = GameManager.Instance.CurrentStageId;
            }

            if (applyLifeTime) {
                StartCoroutine(ApplyLifeTimeCo());
            }

            if (applyInitialDropCooldown) {
                StartCoroutine(ApplyInitialCoolDownCo());
            }
            else {
                Pickable = true;
            }

            Fade(1f, 0.1f);
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.YisoEventStopListening<YisoMapChangeEvent>();
            this.YisoEventStopListening<YisoBountyChangeEvent>();
            moveTween?.Kill();
        }

        public virtual void Initialization() {
            if (!initialized) {
                if (!showNameText) HideObjectName();
                pickedFeedbacks?.Initialization(gameObject);
                spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
                isBeingPicked = false;
                initialized = true;
            }

            var meshRenderer = itemNameText.gameObject.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerID = LayerManager.UISortingLayer;
            meshRenderer.sortingOrder = 2;
        }

        #endregion

        #region Public API

        public virtual void ShowObjectName() {
            if (showNameText) itemNameHolder.SetActive(true);
        }

        public virtual void HideObjectName() {
            if (itemNameHolder.activeInHierarchy) itemNameHolder.SetActive(false);
        }

        public virtual void PickObject(GameObject picker) {
            if (!CheckIfPickable(picker)) {
                PickFail();
                return;
            }

            isBeingPicked = true;
            pickedFeedbacks?.PlayFeedbacks();
            YisoPickableObjectEvent.Trigger(this, picker);
            Pick(picker);
            HideObjectName();
            MoveToPicker(picker);
            PickSuccess();
        }

        /// <summary>
        /// Yiso Character Component 있어야만 주울 수 있음
        /// Condition State가 Normal 이어야만 주울 수 있음 (ex. Cutscene 재생하는 동안 못 주움) 
        /// Inventory가 꽉 차지 않았는지 확인
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckIfPickable(GameObject picker) {
            if (!initialized) return false;
            if (!Pickable) return false;
            if (isBeingPicked) return false;
            if (!picker.TryGetComponent<YisoCharacter>(out var character)) return false;
            if (character.conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Normal) return false;
            return true;
        }

        #endregion

        #region Pick

        protected Tween moveTween;

        protected virtual void MoveToPicker(GameObject picker) {
            if (isMovingToPicker) return;
            moveTween?.Kill();
            var progress = 0f;
            var startPosition = transform.position;
            moveTween = DOTween.Sequence()
                .OnStart(() => isMovingToPicker = true)
                .Join(FadeDO(0f, MoveTime))
                .OnUpdate(() => {
                    progress = moveTween.ElapsedPercentage();
                    var currentPosition = Vector3.Lerp(startPosition, picker.transform.position, progress);
                    var jumpProgress = progress * Mathf.PI;
                    var yOffset = Mathf.Sin(jumpProgress) * 1f * (1f - progress);
                    currentPosition.y += yOffset;
                    transform.position = currentPosition;
                })
                .OnComplete(() => {
                    if (IsPooled) {
                        PoolService.ReleaseObject(gameObject);
                    }
                    else {
                        Destroy(gameObject);
                    }
                });
        }

        /// <summary>
        /// 이걸 상속 받아서 다양하게 사용 가능
        /// ex. 바로 버프, 디버프를 줄수도 있음
        /// </summary>
        /// <param name="picker"></param>
        protected virtual void Pick(GameObject picker) {
        }

        /// <summary>
        /// Pick에 대한 Callback이니 필요하면 상속받아 쓰면 됨.
        /// </summary>
        protected virtual void PickSuccess() {
        }

        protected virtual void PickFail() {
        }

        #endregion

        #region LifeTime

        protected virtual IEnumerator ApplyLifeTimeCo() {
            if (!applyLifeTime) yield break;
            yield return new WaitForSeconds(lifeTime);
            gameObject.SetActive(false);
        }

        protected virtual IEnumerator ApplyInitialCoolDownCo() {
            if (!applyInitialDropCooldown) yield break;
            yield return new WaitForSeconds(initialDropCooldown);
            Pickable = true;
        }

        #endregion

        #region Fade

        protected virtual Tween FadeDO(float endValue, float duration) {
            var sequence = DOTween.Sequence();
            foreach (var spriteRenderer in spriteRenderers) {
                sequence.Join(spriteRenderer.DOFade(endValue, duration));
            }

            return sequence;
        }

        protected virtual void Fade(float endValue, float duration) {
            foreach (var spriteRenderer in spriteRenderers) {
                spriteRenderer.DOFade(endValue, duration);
            }
        }

        #endregion

        #region Event

        public void OnEvent(YisoMapChangeEvent e) {
            if (e.currentMap.Id == spawnMapId) return;
            if (IsPooled) {
                PoolService.ReleaseObject(gameObject);
            }
            else {
                Destroy(gameObject);
            }
        }

        public void OnEvent(YisoBountyChangeEvent e) {
            if (e.currentBounty.Id == spawnStageId) return;
            if (IsPooled) {
                PoolService.ReleaseObject(gameObject);
            }
            else {
                Destroy(gameObject);
            }
        }

        #endregion
    }
}